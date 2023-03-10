using CircuitSharp.Core;

namespace CircuitSharp.Components
{
    public class Inductor : CircuitElement
    {
        #region Properties

        public Lead LeadIn => Lead0;
        public Lead LeadOut => Lead1;

        #endregion

        #region Fields

        private double inductance;
        private bool isTrapezoidal;

        private readonly int[] nodes;
        private double compResistance;
        private double curSourceValue;

        #endregion

        #region Constructor

        public Inductor(double inductance, bool isTrapezoidal = true) : base()
        {
            nodes = new int[2];
            this.inductance = inductance;
            this.isTrapezoidal = isTrapezoidal;
        }

        #endregion

        #region Public Methods

        #region Get/Set Methods

        public double GetInductance()
        {
            return inductance;
        }

        public void SetInductance(double value)
        {
            inductance = value;
        }

        public bool GetIsTrapezoidal()
        {
            return isTrapezoidal;
        }

        public void SetIsTrapezoidal(bool value)
        {
            isTrapezoidal = value;
        }

        #endregion

        #region Overrides

        public override void Reset()
        {
            Current = LeadVolt[0] = LeadVolt[1] = 0;
        }

        public override void Stamp(Circuit circuit)
        {
            nodes[0] = LeadNode[0];
            nodes[1] = LeadNode[1];
            if (isTrapezoidal)
                compResistance = 2 * inductance / circuit.GetTimeStep();
            else
                compResistance = inductance / circuit.GetTimeStep(); // backward euler

            circuit.StampResistor(nodes[0], nodes[1], compResistance);
            circuit.StampRightSide(nodes[0]);
            circuit.StampRightSide(nodes[1]);
        }

        public override void BeginStep(Circuit circuit)
        {
            var diff = LeadVolt[0] - LeadVolt[1];
            if (isTrapezoidal)
                curSourceValue = diff / compResistance + Current;
            else
                curSourceValue = Current; // backward euler
        }

        public override bool NonLinear()
        {
            return true;
        }

        protected override void CalculateCurrent()
        {
            var diff = LeadVolt[0] - LeadVolt[1];
            if (compResistance > 0)
                Current = diff / compResistance + curSourceValue;
        }

        public override void Step(Circuit circuit)
        {
            var diff = LeadVolt[0] - LeadVolt[1];
            circuit.StampCurrentSource(nodes[0], nodes[1], curSourceValue);
        }

        #endregion

        #endregion
    }
}