using CircuitSharp.Core;

namespace CircuitSharp.Components
{
    public class Resistor : CircuitElement
    {
        #region Properties

        public Lead LeadIn => Lead0;
        public Lead LeadOut => Lead1;

        #endregion

        #region Fields

        private double resistance;

        #endregion

        #region Constructor

        public Resistor(double resistance) : base()
        {
            this.resistance = resistance;
        }

        #endregion

        #region Public Methods

        #region Get/Set Methods

        public double GetResistance()
        {
            return resistance;
        }

        public void SetResistance(double value)
        {
            resistance = value;
        }

        #endregion

        #region Overrides

        protected override void CalculateCurrent()
        {
            Current = (LeadVolt[0] - LeadVolt[1]) / resistance;
        }

        public override void Stamp(Circuit circuit)
        {
            circuit.StampResistor(LeadNode[0], LeadNode[1], resistance);
        }

        #endregion

        #endregion
    }
}