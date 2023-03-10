using CircuitSharp.Core;

namespace CircuitSharp.Components
{
    public class CurrentSource : CircuitElement
    {
        #region Properties

        public Lead LeadIn => Lead0;
        public Lead LeadOut => Lead1;

        #endregion

        #region Fields

        private double sourceCurrent;

        #endregion

        #region Constructor

        public CurrentSource() : base()
        {
            sourceCurrent = 0.01;
        }

        #endregion

        #region Public Methods

        #region Overrides

        public override void Stamp(Circuit circuit)
        {
            circuit.StampCurrentSource(LeadNode[0], LeadNode[1], sourceCurrent);
        }

        public override double GetVoltageDelta()
        {
            return LeadVolt[1] - LeadVolt[0];
        }

        #endregion

        #endregion

        #region Private Methods

        #region Get/Set Methods

        public void SetSourceCurrent(double value)
        {
            sourceCurrent = value;
        }

        public double GetSourceCurrent()
        {
            return sourceCurrent;
        }

        #endregion

        #endregion
    }
}