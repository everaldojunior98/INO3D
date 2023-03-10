using CircuitSharp.Core;

namespace CircuitSharp.Components
{
    public class Capacitor : CircuitElement
    {
        #region Properties

        public Lead LeadIn => Lead0;
        public Lead LeadOut => Lead1;

        #endregion

        #region Fields

        private const double DefaultCapacitance = 1E-5;
        
        private double capacitance;
        private bool isTrapezoidal;

        private double compResistance;
        private double voltDiff;
        private double curSourceValue;

        #endregion

        #region Constructor

        public Capacitor(double capacitance, bool isTrapezoidal = true) : base()
        {
            this.capacitance = capacitance < 0 ? DefaultCapacitance : capacitance;
            this.isTrapezoidal = isTrapezoidal;
        }

        #endregion

        #region Public Methods

        #region Get/Set Methods

        public double GetCapacitance()
        {
            return capacitance;
        }

        public void SetCapacitance(double value)
        {
            capacitance = value < 0 ? DefaultCapacitance : value;
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

        public override void SetLeadVoltage(int leadX, double voltage)
        {
            base.SetLeadVoltage(leadX, voltage);
            voltDiff = LeadVolt[0] - LeadVolt[1];
        }

        public override void Reset()
        {
            Current = 0;
            // Put small charge on caps when reset to start oscillators
            voltDiff = 1E-3;
        }

        public override void Stamp(Circuit circuit)
        {
            // Capacitor companion model using trapezoidal approximation
            // (Norton equivalent) consists of a current source in
            // parallel with a resistor. Trapezoidal is more accurate
            // than backward euler but can cause oscillatory behavior
            // if RC is small relative to the timeStep.
            if (isTrapezoidal)
                compResistance = circuit.GetTimeStep() / (2 * capacitance);
            else
                compResistance = circuit.GetTimeStep() / capacitance;

            circuit.StampResistor(LeadNode[0], LeadNode[1], compResistance);
            circuit.StampRightSide(LeadNode[0]);
            circuit.StampRightSide(LeadNode[1]);
        }

        public override void BeginStep(Circuit circuit)
        {
            if (isTrapezoidal)
                curSourceValue = -voltDiff / compResistance - Current;
            else
                curSourceValue = -voltDiff / compResistance;
        }

        protected override void CalculateCurrent()
        {
            var diff = LeadVolt[0] - LeadVolt[1];
            // We check compResistance because this might get called
            // before stamp(CirSim sim), which sets compResistance,
            // causing infinite current
            if (compResistance > 0)
                Current = diff / compResistance + curSourceValue;
        }

        public override void Step(Circuit circuit)
        {
            circuit.StampCurrentSource(LeadNode[0], LeadNode[1], curSourceValue);
        }

        #endregion

        #endregion
    }
}