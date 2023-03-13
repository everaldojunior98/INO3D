using System.Linq;
using Assets.Scripts.CustomElements.Machines;
using CLanguage;
using CLanguage.Interpreter;
using SharpCircuit;
using static Assets.Scripts.CustomElements.Chips.BkBxPin;
using static SharpCircuit.Circuit;

namespace Assets.Scripts.CustomElements.Chips
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
            pins = new BkBxPin[getLeadCount()];

            for (var i = 0; i < pins.Length; i++)
            {
                if (i < inputsCount)
                    pins[i] = new BkBxPin($"IN{i + 1}", BlackBoxPinMode.Input);
                else
                    pins[i] = new BkBxPin($"OUT{i - inputsCount + 1}", BlackBoxPinMode.Output);
            }

            interpreter.Reset(InterpreterEntryPoint);
            interpreter.Run();

            allocLeads();

            interpreter.Reset(LoopEntryPoint);
        }

        private bool IsValidPin(short pin)
        {
            return pin >= 0 && pin < pins.Length;
        }

        #endregion

        #region Overrides

        public override void setCurrent(int l, double c)
        {
            for (var i = 0; i != getLeadCount(); i++)
                if (pins[i].Mode == BlackBoxPinMode.Output && pins[i].VoltSourceId == l)
                    pins[i].Current = c;
        }

        public override void setVoltageSource(int j, int vs)
        {
            for (var i = 0; i != getLeadCount(); i++)
            {
                var pin = pins[i];
                if (pin.Mode == BlackBoxPinMode.Output && j-- == 0)
                    pin.VoltSourceId = vs;
            }
        }

        public override void stamp(Circuit circuit)
        {
            for (var i = 0; i != getLeadCount(); i++)
            {
                var pin = pins[i];
                if (pin.Mode == BlackBoxPinMode.Output)
                    circuit.stampVoltageSource(0, lead_node[i], pin.VoltSourceId);
            }
        }

        public override void step(Circuit circuit)
        {
            currentTime = circuit.time;

            var tickTime = (currentTime - lastTime) * 1E6;
            sleepTime -= tickTime;

            for (var i = 0; i != getLeadCount(); i++)
            {
                var pin = pins[i];
                if (pin.Mode == BlackBoxPinMode.Input)
                    pin.Voltage = lead_volt[i];
            }

            if (sleepTime <= 0 || sleepTime - tickTime <= 0)
            {
                sleepTime = 0;
                interpreter.Step((int)tickTime);
            }

            for (var i = 0; i != getLeadCount(); i++)
            {
                var pin = pins[i];
                if (pin.Mode == BlackBoxPinMode.Output)
                    circuit.updateVoltageSource(0, i, pin.VoltSourceId, pin.Voltage);
            }

            lastTime = circuit.time;
        }

        public override void reset()
        {
            for (var i = 0; i != getLeadCount(); i++)
            {
                pins[i].Voltage = 0;
                pins[i].Current = 0;

                lead_volt[i] = 0;
            }

            interpreter.Reset(LoopEntryPoint);
            sleepTime = 0;
        }

        public override bool leadsAreConnected(int l1, int l2)
        {
            return false;
        }

        public override bool leadIsGround(int l)
        {
            return pins[l].Mode == BlackBoxPinMode.Output;
        }

        public override int getVoltageSourceCount()
        {
            return pins.Count(pin => pin.Mode == BlackBoxPinMode.Output);
        }

        public override int getLeadCount()
        {
            return inputsCount + outputCount;
        }

        #endregion
    }
}