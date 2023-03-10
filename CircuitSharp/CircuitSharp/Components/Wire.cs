using CircuitSharp.Core;

namespace CircuitSharp.Components
{
    public class Wire : CircuitElement
    {
        #region Properties

        public Lead LeadIn => Lead0;
        public Lead LeadOut => Lead1;

        #endregion

        #region Public Methods

        #region Overrides

        public override void Stamp(Circuit circuit)
        {
            circuit.StampVoltageSource(LeadNode[0], LeadNode[1], VoltSource, 0);
        }

        public override int GetVoltageSourceCount()
        {
            return 1;
        }

        public override double GetVoltageDelta()
        {
            return LeadVolt[0];
        }

        public override bool IsWire()
        {
            return true;
        }

        #endregion

        #endregion
    }
}