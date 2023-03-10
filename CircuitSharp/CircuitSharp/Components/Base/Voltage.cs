using System;
using CircuitSharp.Core;

namespace CircuitSharp.Components.Base
{
    public class Voltage : CircuitElement
    {
        #region Fields

        public enum WaveType
        {
            Dc,
            Ac,
            Square,
            Triangle,
            Sawtooth,
            Pulse,
            Var
        }

        private double maxVoltage;
        private double frequency;
        private double bias;

        private WaveType waveform;
        private double phaseShift;
        private double dutyCycle;

        private double freqTimeZero;

        #endregion

        #region Constructor

        protected Voltage(WaveType wave) : base()
        {
            SetWaveform(wave);
            maxVoltage = 5;
            frequency = 40;
            dutyCycle = 0.5;
            Reset();
        }

        #endregion

        #region Public Methods

        #region Get/Set Methods

        public double GetMaxVoltage()
        {
            return maxVoltage;
        }

        public void SetMaxVoltage(double value)
        {
            maxVoltage = value;
        }

        public WaveType GetWaveform()
        {
            return waveform;
        }

        public void SetWaveform(WaveType type)
        {
            var oldWave = waveform;
            waveform = type;
            if (waveform == WaveType.Dc && oldWave != WaveType.Dc)
                bias = 0;
        }

        public double GetFrequency()
        {
            return frequency;
        }

        public double GetPhaseShift()
        {
            return phaseShift * 180 / Pi;
        }

        public void SetPhaseShift(double phase)
        {
            phaseShift = phase * Pi / 180;
        }

        public double GetDutyCycle()
        {
            return dutyCycle * 10;
        }

        public void SetDutyCycle(double cycle)
        {
            dutyCycle = cycle * 0.01;
        }

        public double GetBias()
        {
            return bias;
        }

        public void SetBias(double value)
        {
            bias = value;
        }

        #endregion

        #region Overrides

        public override void Stamp(Circuit circuit)
        {
            if (GetWaveform() == WaveType.Dc)
                circuit.StampVoltageSource(LeadNode[0], LeadNode[1], VoltSource, GetVoltage(circuit));
            else
                circuit.StampVoltageSource(LeadNode[0], LeadNode[1], VoltSource);
        }

        public override void Step(Circuit circuit)
        {
            if (GetWaveform() != WaveType.Dc)
                circuit.UpdateVoltageSource(LeadNode[0], LeadNode[1], VoltSource, GetVoltage(circuit));
        }

        public override int GetVoltageSourceCount()
        {
            return 1;
        }

        public override double GetVoltageDelta()
        {
            return LeadVolt[1] - LeadVolt[0];
        }

        #endregion

        public sealed override void Reset()
        {
            freqTimeZero = 0;
        }

        #endregion

        #region Private Methods

        private void SetFrequency(double newFreq, double timeStep, double time)
        {
            var oldFreq = frequency;
            frequency = newFreq;
            var maxFreq = 1 / (8 * timeStep);
            if (frequency > maxFreq)
                frequency = maxFreq;
            freqTimeZero = time - oldFreq * (time - freqTimeZero) / frequency;
        }

        private double TriangleFunc(double x)
        {
            if (x < Pi)
                return x * (2 / Pi) - 1;
            return 1 - (x - Pi) * (2 / Pi);
        }

        protected virtual double GetVoltage(Circuit circuit)
        {
            SetFrequency(GetFrequency(), circuit.GetTimeStep(), circuit.GetTime());
            var w = 2 * Pi * (circuit.GetTime() - freqTimeZero) * frequency + phaseShift;
            switch (GetWaveform())
            {
                case WaveType.Dc: return maxVoltage + bias;
                case WaveType.Ac: return Math.Sin(w) * maxVoltage + bias;
                case WaveType.Square: return bias + ((w % (2 * Pi) > (2 * Pi * dutyCycle)) ? -maxVoltage : maxVoltage);
                case WaveType.Triangle: return bias + TriangleFunc(w % (2 * Pi)) * maxVoltage;
                case WaveType.Sawtooth: return bias + (w % (2 * Pi)) * (maxVoltage / Pi) - maxVoltage;
                case WaveType.Pulse: return ((w % (2 * Pi)) < 1) ? maxVoltage + bias : bias;
                case WaveType.Var:
                default: return 0;
            }
        }

        #endregion
    }
}