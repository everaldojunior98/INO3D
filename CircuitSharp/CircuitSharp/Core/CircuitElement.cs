namespace CircuitSharp.Core
{
    public abstract class CircuitElement : ICircuitElement
    {
        #region Properties

        protected Lead Lead0 => new Lead(this, 0);
        protected Lead Lead1 => new Lead(this, 1);

        public Circuit Circuit { get; set; }

        #endregion

        #region Fields

        protected const double Pi = 3.14159265358979323846;

        protected int VoltSource;
        protected double Current;
        protected int[] LeadNode;
        protected double[] LeadVolt;

        #endregion

        #region Constructor

        protected CircuitElement()
        {
            AllocLeads();
        }

        #endregion

        #region Protected Methods

        protected virtual void CalculateCurrent()
        {
        }

        #endregion

        #region Public Methods

        public int GetLeadNode(int leadIndex)
        {
            if (LeadNode == null)
                AllocLeads();
            return LeadNode[leadIndex];
        }

        public void SetLeadNode(int leadIndex, int nodeIndex)
        {
            if (LeadNode == null)
                AllocLeads();
            LeadNode[leadIndex] = nodeIndex;
        }

        public virtual void BeginStep(Circuit circuit)
        {
        }

        public virtual void Step(Circuit circuit)
        {
        }

        public virtual void Stamp(Circuit circuit)
        {
        }

        public virtual void Reset()
        {
            for (var i = 0; i != GetLeadCount() + GetInternalLeadCount(); i++)
                LeadVolt[i] = 0;
        }

        public virtual int GetLeadCount()
        {
            return 2;
        }

        public virtual int GetInternalLeadCount()
        {
            return 0;
        }

        public virtual double GetLeadVoltage(int leadX)
        {
            return LeadVolt[leadX];
        }

        public virtual void SetLeadVoltage(int leadX, double voltage)
        {
            LeadVolt[leadX] = voltage;
            CalculateCurrent();
        }

        public virtual double GetCurrent()
        {
            return Current;
        }

        public virtual void SetCurrent(int voltSourceIndex, double current)
        {
            Current = current;
        }

        public virtual double GetVoltageDelta()
        {
            return LeadVolt[0] - LeadVolt[1];
        }

        public virtual int GetVoltageSourceCount()
        {
            return 0;
        }

        public virtual void SetVoltageSource(int leadX, int voltSourceIndex)
        {
            VoltSource = voltSourceIndex;
        }

        public virtual bool LeadsAreConnected(int leadX, int leadY)
        {
            return true;
        }

        public virtual bool LeadIsGround(int leadX)
        {
            return false;
        }

        public virtual bool IsWire()
        {
            return false;
        }

        public virtual bool NonLinear()
        {
            return false;
        }

        public ScopeFrame GetScopeFrame(double time)
        {
            return new ScopeFrame
            {
                Time = time,
                Current = GetCurrent(),
                Voltage = GetVoltageDelta(),
            };
        }

        #endregion

        #region Protected Methods

        protected void AllocLeads()
        {
            LeadNode = new int[GetLeadCount() + GetInternalLeadCount()];
            LeadVolt = new double[GetLeadCount() + GetInternalLeadCount()];
        }

        #endregion
    }
}