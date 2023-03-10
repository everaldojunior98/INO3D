using CircuitSharp.Core;

namespace CircuitSharp.Components
{
    public class Output : CircuitElement
    {
        #region Properties

        public Lead LeadIn => Lead0;

        #endregion

        #region Public Methods

        #region Overrides

        public override int GetLeadCount()
        {
            return 1;
        }

        public override double GetVoltageDelta()
        {
            return LeadVolt[0];
        }

        #endregion

        #endregion
    }
}