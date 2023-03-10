namespace CircuitSharp.Core
{
    public interface ICircuitElement
    {
        Circuit Circuit { get; set; }

        void BeginStep(Circuit circuit);
        void Step(Circuit circuit);
        void Stamp(Circuit circuit);

        void Reset();

        int GetLeadNode(int leadIndex);
        void SetLeadNode(int leadIndex, int nodeIndex);

        // Lead count
        int GetLeadCount();
        int GetInternalLeadCount();

        // Lead voltage
        double GetLeadVoltage(int leadX);
        void SetLeadVoltage(int leadX, double voltage);

        // Current
        double GetCurrent();
        void SetCurrent(int voltSourceIndex, double cValue);

        // Voltage
        double GetVoltageDelta();
        int GetVoltageSourceCount();
        void SetVoltageSource(int leadX, int voltSourceIndex);

        // Connection
        bool LeadsAreConnected(int leadX, int leadY);
        bool LeadIsGround(int leadX);

        // State
        bool IsWire();
        bool NonLinear();

        ScopeFrame GetScopeFrame(double time);
    }
}