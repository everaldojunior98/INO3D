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

        public readonly Lead[] Leads;

        #endregion

        #region Fields

        private readonly int inputsCount;
        private readonly int outputCount;

        private const string InterpreterEntryPoint = "__cinit";
        private const string LoopEntryPoint = "main";
        private readonly CInterpreter interpreter;

        private BkBxPin[] pins;

        private double sleepTime;

        private double currentTime;
        private double lastTime;

        #endregion

        #region Constructor

        public BkBx(int inputs, int outputs, string code)
        {
            inputsCount = inputs;
            outputCount = outputs;

            Leads = new Lead[inputs + outputs];
            for (var i = 0; i < Leads.Length; i++)
                Leads[i] = new Lead(this, i);

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

            for (var i = 0; i < pins.Length; i++)
            {
                if (i < inputsCount)
                    pins[i] = new BkBxPin($"IN{i + 1}", BlackBoxPinMode.Input);
                else
                    pins[i] = new BkBxPin($"OUT{i - inputsCount + 1}", BlackBoxPinMode.Output);
            }

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
            return inputsCount + outputCount;
        }

        #endregion
    }
}