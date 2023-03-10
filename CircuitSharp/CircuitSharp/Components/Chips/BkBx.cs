using System.Linq;
using CircuitSharp.Components.Base;
using CircuitSharp.Core;
using CircuitSharp.Machines;
using CLanguage;
using CLanguage.Interpreter;
using static CircuitSharp.Components.Base.BkBxPin;

namespace CircuitSharp.Components.Chips
{
    public class BkBx : CircuitElement
    {
        #region Properties

        public new Lead IN1Lead => new Lead(this, 0);
        public new Lead IN2Lead => new Lead(this, 1);
        public new Lead IN3Lead => new Lead(this, 2);
        public new Lead IN4Lead => new Lead(this, 3);
        public new Lead IN5Lead => new Lead(this, 4);
        public new Lead IN6Lead => new Lead(this, 5);
        public new Lead IN7Lead => new Lead(this, 6);
        public new Lead IN8Lead => new Lead(this, 7);
        public new Lead IN9Lead => new Lead(this, 8);
        public new Lead IN10Lead => new Lead(this, 9);
        public new Lead OUT1Lead => new Lead(this, 10);
        public new Lead OUT2Lead => new Lead(this, 11);
        public new Lead OUT3Lead => new Lead(this, 12);
        public new Lead OUT4Lead => new Lead(this, 13);
        public new Lead OUT5Lead => new Lead(this, 14);
        public new Lead OUT6Lead => new Lead(this, 15);
        public new Lead OUT7Lead => new Lead(this, 16);
        public new Lead OUT8Lead => new Lead(this, 17);
        public new Lead OUT9Lead => new Lead(this, 18);
        public new Lead OUT10Lead => new Lead(this, 19);

        #endregion

        #region Fields

        private const string InterpreterEntryPoint = "__cinit";
        private const string LoopEntryPoint = "main";
        private readonly CInterpreter interpreter;

        private BkBxPin[] pins;

        private double sleepTime;

        private double currentTime;
        private double lastTime;

        #endregion

        #region Constructor

        public BkBx(string code)
        {
            var machine = new BlackBoxMachineInfo(this);
            var fullCode = code + "\n\nvoid main() { while(1){loop();}}";
            interpreter = CLanguageService.CreateInterpreter(fullCode, machine);
            interpreter.CpuSpeed = 10 ^ 9;

            SetupPins();
        }

        #endregion

        #region Public Methods

        public double ReadPin(short pin)
        {
            if (IsValidPin(pin) && pins[pin].Mode == BlackBoxPinMode.Input)
                return pins[pin].Voltage;

            return 0;
        }

        public void WritePin(short pin, double value)
        {
            if (IsValidPin(pin) && pins[pin].Mode == BlackBoxPinMode.Output)
                pins[pin].Voltage = value;
        }

        #endregion

        #region Private Methods

        private void SetupPins()
        {
            pins = new BkBxPin[GetLeadCount()];

            pins[0] = new BkBxPin("IN1", BlackBoxPinMode.Input);
            pins[1] = new BkBxPin("IN2", BlackBoxPinMode.Input);
            pins[2] = new BkBxPin("IN3", BlackBoxPinMode.Input);
            pins[3] = new BkBxPin("IN4", BlackBoxPinMode.Input);
            pins[4] = new BkBxPin("IN5", BlackBoxPinMode.Input);
            pins[5] = new BkBxPin("IN6", BlackBoxPinMode.Input);
            pins[6] = new BkBxPin("IN7", BlackBoxPinMode.Input);
            pins[7] = new BkBxPin("IN8", BlackBoxPinMode.Input);
            pins[8] = new BkBxPin("IN9", BlackBoxPinMode.Input);
            pins[9] = new BkBxPin("IN10", BlackBoxPinMode.Input);

            pins[10] = new BkBxPin("OUT1", BlackBoxPinMode.Output);
            pins[11] = new BkBxPin("OUT2", BlackBoxPinMode.Output);
            pins[12] = new BkBxPin("OUT3", BlackBoxPinMode.Output);
            pins[13] = new BkBxPin("OUT4", BlackBoxPinMode.Output);
            pins[14] = new BkBxPin("OUT5", BlackBoxPinMode.Output);
            pins[15] = new BkBxPin("OUT6", BlackBoxPinMode.Output);
            pins[16] = new BkBxPin("OUT7", BlackBoxPinMode.Output);
            pins[17] = new BkBxPin("OUT8", BlackBoxPinMode.Output);
            pins[18] = new BkBxPin("OUT9", BlackBoxPinMode.Output);
            pins[19] = new BkBxPin("OUT10", BlackBoxPinMode.Output);

            interpreter.Reset(InterpreterEntryPoint);
            interpreter.Run();

            AllocLeads();

            interpreter.Reset(LoopEntryPoint);
        }

        private bool IsValidPin(short pin)
        {
            return pin >= 0 && pin < pins.Length;
        }

        #endregion

        #region Overrides

        public override void SetCurrent(int lead, double current)
        {
            for (var i = 0; i != GetLeadCount(); i++)
                if (pins[i].Mode == BlackBoxPinMode.Output && pins[i].VoltSourceId == lead)
                    pins[i].Current = current;
        }

        public override void SetVoltageSource(int j, int vs)
        {
            for (var i = 0; i != GetLeadCount(); i++)
            {
                var pin = pins[i];
                if (pin.Mode == BlackBoxPinMode.Output && j-- == 0)
                    pin.VoltSourceId = vs;
            }
        }

        public override void Stamp(Circuit circuit)
        {
            for (var i = 0; i != GetLeadCount(); i++)
            {
                var pin = pins[i];
                if (pin.Mode == BlackBoxPinMode.Output)
                    circuit.StampVoltageSource(0, LeadNode[i], pin.VoltSourceId);
            }
        }

        public override void Step(Circuit circuit)
        {
            currentTime = circuit.GetTime();

            var tickTime = (currentTime - lastTime) * 1E6;
            sleepTime -= tickTime;

            for (var i = 0; i != GetLeadCount(); i++)
            {
                var pin = pins[i];
                if (pin.Mode == BlackBoxPinMode.Input)
                    pin.Voltage = LeadVolt[i];
            }

            if (sleepTime <= 0 || sleepTime - tickTime <= 0)
            {
                sleepTime = 0;
                interpreter.Step((int)tickTime);
            }

            for (var i = 0; i != GetLeadCount(); i++)
            {
                var pin = pins[i];
                if (pin.Mode == BlackBoxPinMode.Output)
                    circuit.UpdateVoltageSource(0, i, pin.VoltSourceId, pin.Voltage);
            }

            lastTime = circuit.GetTime();
        }

        public override void Reset()
        {
            for (var i = 0; i != GetLeadCount(); i++)
            {
                pins[i].Voltage = 0;
                pins[i].Current = 0;

                LeadVolt[i] = 0;
            }

            interpreter.Reset(LoopEntryPoint);
            sleepTime = 0;
        }

        public override bool LeadsAreConnected(int lead1, int lead2)
        {
            return false;
        }

        public override bool LeadIsGround(int lead)
        {
            return pins[lead].Mode == BlackBoxPinMode.Output;
        }

        public override int GetVoltageSourceCount()
        {
            return pins.Count(pin => pin.Mode == BlackBoxPinMode.Output);
        }

        public override int GetLeadCount()
        {
            return 20;
        }

        #endregion
    }
}