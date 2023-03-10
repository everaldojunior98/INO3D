using System;

namespace CircuitSharp.Core
{
    public class Error
    {
        #region Properties

        public enum ErrorCode
        {
            E1, // NaN/Infinite matrix!
            E2, // Singular matrix!
            E3, // Convergence failed!
            E4, // No path for current source!
            E5, // Voltage source/wire loop with no resistance!
            E6, // Capacitor loop with no resistance!
            E7, // Matrix error
            E8, // AddElement array length mismatch
        }

        public ErrorCode Code { get; }
        public ICircuitElement Element { get; }

        #endregion

        #region Constructor

        public Error(ErrorCode code, ICircuitElement element)
        {
            Code = code;
            Element = element;
        }

        #endregion
    }

    public class CircuitException : Exception
    {
        public Error Error { get; }

        public CircuitException(Error error)
        {
            Error = error;
        }
    }
}