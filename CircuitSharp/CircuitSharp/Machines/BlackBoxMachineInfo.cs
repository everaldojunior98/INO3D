using CircuitSharp.Components.Chips;
using CLanguage;

namespace CircuitSharp.Machines
{
    public class BlackBoxMachineInfo : MachineInfo
    {
        #region Fields

        private readonly BkBx blackBox;

        #endregion

        #region Constructor

        public BlackBoxMachineInfo(BkBx chip)
        {
            blackBox = chip;

            CharSize = 1;
            ShortIntSize = 2;
            IntSize = 2;
            LongIntSize = 4;
            LongLongIntSize = 8;
            FloatSize = 4;
            DoubleSize = 8;
            LongDoubleSize = 8;
            PointerSize = 2;
            HeaderCode = @"
                typedef bool boolean;
                typedef unsigned char byte;
                typedef unsigned short word;
                ";

            #region I/O

            AddInternalFunction("double read (int pin)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int16Value;
                var value = blackBox.ReadPin(pin);
                interpreter.Push(value);
            });

            AddInternalFunction("void write (int pin, double value)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int16Value;
                var value = interpreter.ReadArg(1).Float64Value;
                blackBox.WritePin(pin, value);
            });

            #endregion
        }

        #endregion
    }
}