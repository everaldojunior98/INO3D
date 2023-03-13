namespace Assets.Scripts.CustomElements.Chips
{
    public class BkBxPin
    {
        #region Properties

        public enum BlackBoxPinMode
        {
            Input,
            Output
        }

        public readonly BlackBoxPinMode Mode;

        public double Voltage;
        public double Current;

        public int VoltSourceId;

        #endregion

        #region Fields

        private readonly string name;

        #endregion

        #region Constructor

        public BkBxPin(string name, BlackBoxPinMode mode)
        {
            this.name = name;
            Mode = mode;
        }

        #endregion

        #region Public Methods

        public string GetName()
        {
            return name;
        }

        public double GetVoltage()
        {
            return Voltage;
        }

        #endregion
    }
}