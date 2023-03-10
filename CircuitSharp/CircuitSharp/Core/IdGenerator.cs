namespace CircuitSharp.Core
{
    public class IdGenerator
    {
        #region Fields

        private long currentId;

        #endregion

        #region Constructor

        public IdGenerator()
        {
            currentId = 0;
        }

        #endregion

        #region Public Methods

        public long NextId()
        {
            currentId++;
            return currentId;
        }

        #endregion
    }
}