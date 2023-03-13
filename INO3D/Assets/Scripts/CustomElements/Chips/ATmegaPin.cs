namespace Assets.Scripts.CustomElements.Chips
{
    public class ATmegaPin
    {
        #region Properties

        public enum ATmegaPinType
        {
            Analog,
            Digital,
            DigitalPwm
        }

        public enum ATmegaPinMode
        {
            Input,
            Output,
            InputPullup,
        }

        public ATmegaPinMode Mode;

        public double DutyCycle;
        public double Current;

        public int VoltSourceId;
        public double Frequency;

        public bool IsControlPin;
        public bool IsInterruptPin;
        public int InterruptIndex;

        #endregion

        #region Fields

        private readonly string name;
        private readonly double maxVoltage;
        private readonly ATmegaPinType type;

        #endregion

        #region Constructor

        public ATmegaPin(string name, double maxVoltage, double frequency, ATmegaPinType type)
        {
            this.name = name;
            this.maxVoltage = maxVoltage;
            Frequency = frequency;
            this.type = type;
        }

        #endregion

        #region Public Methods

        public string GetName()
        {
            return name;
        }

        public new ATmegaPinType GetType()
        {
            return type;
        }

        public double GetVoltage()
        {
            return DutyCycle * maxVoltage;
        }

        #endregion
    }
}