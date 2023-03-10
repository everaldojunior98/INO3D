using CircuitSharp.Components.Base;
using CircuitSharp.Core;

namespace CircuitSharp.Components
{
    public class VoltageInput : Voltage
    {
        #region Properties

        public Lead LeadPos => Lead0;

        #endregion

        #region Constructor

        public VoltageInput(WaveType wave) : base(wave)
        {

        }

        #endregion

        #region Public Methods

        #region Overrides



        #endregion

        #endregion


        public override int GetLeadCount()
        {
            return 1;
        }

        public override double GetVoltageDelta()
        {
            return LeadVolt[0];
        }

        public override void Stamp(Circuit circuit)
        {
            if (GetWaveform() == WaveType.Dc)
            {
                circuit.StampVoltageSource(0, LeadNode[0], VoltSource, GetVoltage(circuit));
            }
            else
            {
                circuit.StampVoltageSource(0, LeadNode[0], VoltSource);
            }
        }

        public override void Step(Circuit circuit)
        {
            if (GetWaveform() != WaveType.Dc)
                circuit.UpdateVoltageSource(0, LeadNode[0], VoltSource, GetVoltage(circuit));
        }

        public override bool LeadIsGround(int n1)
        {
            return true;
        }
    }
}