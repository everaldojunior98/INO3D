using CircuitSharp.Core;

namespace CircuitSharp.Components
{
    public class Ground : CircuitElement
    {
        #region Properties

        public Lead LeadIn => Lead0;

        #endregion

        #region Constructor

        public Ground() : base()
        {

        }

        #endregion

        #region Public Methods

        #region Overrides

        public override void SetCurrent(int voltSourceIndex, double c)
        {
            Current = -c;
        }

        public override void Stamp(Circuit circuit)
        {
            circuit.StampVoltageSource(0, LeadNode[0], VoltSource, 0);
        }

        public override double GetVoltageDelta()
        {
            return 0;
        }

        public override int GetVoltageSourceCount()
        {
            return 1;
        }

        public override bool LeadIsGround(int n1)
        {
            return true;
        }

        public override int GetLeadCount()
        {
            return 1;
        }

        #endregion

        #endregion
    }
}