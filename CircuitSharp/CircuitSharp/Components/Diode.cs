using System;
using CircuitSharp.Core;

namespace CircuitSharp.Components
{
    public class Diode : CircuitElement
    {
        #region Properties

        public Lead LeadIn => Lead0;
        public Lead LeadOut => Lead1;

        protected double ForwardDrop
        {
            get => forwardDrop;
            set
            {
                forwardDrop = value;
                Setup();
            }
        }

        protected double ZVoltage
        {
            get => zVoltage;
            set
            {
                zVoltage = value;
                Setup();
            }
        }

        #endregion

        #region Fields

        private const double DefaultDrop = 0.805904783;

        private double forwardDrop;
        private double zVoltage;

        private double leakage = 1e-14;

        private readonly int[] nodes;

        private double vt;
        private double vdCoef;
        private double zOffset;
        private double lastVoltDiff;
        private double criticalVoltage;

        #endregion

        #region Constructor

        protected Diode() : base()
        {
            nodes = new int[2];
            forwardDrop = DefaultDrop;
            ZVoltage = 0;
            Setup();
        }

        #endregion

        #region Public Methods

        #region Overrides

        public override void Step(Circuit circuit)
        {
            var voltDiff = LeadVolt[0] - LeadVolt[1];
            if (Math.Abs(voltDiff - lastVoltDiff) > 0.01)
                circuit.Converged = false;

            voltDiff = LimitStep(circuit, voltDiff, lastVoltDiff);
            lastVoltDiff = voltDiff;

            if (voltDiff >= 0 || zVoltage == 0)
            {
                var eval = Math.Exp(voltDiff * vdCoef);
                if (voltDiff < 0)
                    eval = 1;

                var geq = vdCoef * leakage * eval;
                var nc = (eval - 1) * leakage - geq * voltDiff;
                circuit.StampConductance(nodes[0], nodes[1], geq);
                circuit.StampCurrentSource(nodes[0], nodes[1], nc);
            }
            else
            {
                var geq = leakage * vdCoef * (Math.Exp(voltDiff * vdCoef) + Math.Exp((-voltDiff - zOffset) * vdCoef));
                var nc = leakage * (Math.Exp(voltDiff * vdCoef) - Math.Exp((-voltDiff - zOffset) * vdCoef) - 1) +
                         geq * -voltDiff;
                circuit.StampConductance(nodes[0], nodes[1], geq);
                circuit.StampCurrentSource(nodes[0], nodes[1], nc);
            }
        }

        public override void Stamp(Circuit circuit)
        {
            nodes[0] = LeadNode[0];
            nodes[1] = LeadNode[1];
            circuit.StampNonLinear(nodes[0]);
            circuit.StampNonLinear(nodes[1]);
        }

        protected override void CalculateCurrent()
        {
            var voltDiff = LeadVolt[0] - LeadVolt[1];
            if (voltDiff >= 0 || zVoltage == 0)
                Current = leakage * (Math.Exp(voltDiff * vdCoef) - 1);
            else
                Current = leakage * (Math.Exp(voltDiff * vdCoef) - Math.Exp((-voltDiff - zOffset) * vdCoef) - 1);
        }

        public override bool NonLinear()
        {
            return true;
        }

        public override void Reset()
        {
            lastVoltDiff = 0;
            LeadVolt[0] = 0;
            LeadVolt[1] = 0;
        }

        #endregion

        #endregion

        #region Private Methods

        private void Setup()
        {
            vdCoef = Math.Log(1 / leakage + 1) / forwardDrop;
            vt = 1 / vdCoef;

            criticalVoltage = vt * Math.Log(vt / (Math.Sqrt(2) * leakage));
            if (zVoltage == 0)
            {
                zOffset = 0;
            }
            else
            {
                var i = -0.005;
                zOffset = zVoltage - Math.Log(-(1 + i / leakage)) / vdCoef;
            }
        }

        private double LimitStep(Circuit circuit, double vNew, double vOld)
        {
            double arg;
            if (vNew > criticalVoltage && Math.Abs(vNew - vOld) > vt + vt)
            {
                if (vOld > 0)
                {
                    arg = 1 + (vNew - vOld) / vt;
                    if (arg > 0)
                    {
                        vNew = vOld + vt * Math.Log(arg);
                        var v0 = Math.Log(1e-6 / leakage) * vt;
                        vNew = Math.Max(v0, vNew);
                    }
                    else
                    {
                        vNew = criticalVoltage;
                    }
                }
                else
                {
                    vNew = vt * Math.Log(vNew / vt);
                }

                circuit.Converged = false;
            }
            else if (vNew < 0 && zOffset != 0)
            {
                vNew = -vNew - zOffset;
                vOld = -vOld - zOffset;
                if (vNew > criticalVoltage && Math.Abs(vNew - vOld) > vt + vt)
                {
                    if (vOld > 0)
                    {
                        arg = 1 + (vNew - vOld) / vt;
                        if (arg > 0)
                        {
                            vNew = vOld + vt * Math.Log(arg);
                            var v0 = Math.Log(1e-6 / leakage) * vt;
                            vNew = Math.Max(v0, vNew);
                        }
                        else
                        {
                            vNew = criticalVoltage;
                        }
                    }
                    else
                    {
                        vNew = vt * Math.Log(vNew / vt);
                    }

                    circuit.Converged = false;
                }

                vNew = -(vNew + zOffset);
            }

            return vNew;
        }

        #endregion
    }
}