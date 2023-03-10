using CircuitSharp.Core;

namespace CircuitSharp.Components
{
    public class SwitchSPST : CircuitElement
    {
        #region Properties

        public Lead LeadA => Lead0;
        public Lead LeadB => Lead1;

        public bool IsOpen => position == 1;

        #endregion

        #region Fields

        private int position;

        #endregion

        #region Constructor

        public SwitchSPST() : base()
        {
            position = 0;
        }

        #endregion

        #region Public Methods

        public void Open()
        {
            position = 1;
            Circuit.NeedAnalyze();
        }

        public void Close()
        {
            position = 0;
            Circuit.NeedAnalyze();
        }

        #endregion

        #region Overrides

        protected override void CalculateCurrent()
        {
            if (position == 1)
                Current = 0;
        }

        public override void Stamp(Circuit circuit)
        {
            if (position == 0)
                circuit.StampVoltageSource(LeadNode[0], LeadNode[1], VoltSource, 0);
        }

        public override int GetVoltageSourceCount()
        {
            return position == 1 ? 0 : 1;
        }

        public override bool LeadsAreConnected(int leadX, int leadY)
        {
            return position == 0;
        }

        public override bool IsWire()
        {
            return true;
        }

        #endregion
    }
}