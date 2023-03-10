namespace CircuitSharp.Core
{
    public class Lead
    {
        #region Properties

        public CircuitElement Element { get; }
        public int Index { get; }

        #endregion

        #region Constructor

        public Lead(CircuitElement element, int index)
        {
            Element = element;
            Index = index;
        }

        #endregion
    }
}