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
                #define IN1 0
                #define IN2 1
                #define IN3 2
                #define IN4 3
                #define IN5 4
                #define IN6 5
                #define IN7 6
                #define IN8 7
                #define IN9 8
                #define IN10 9
                #define OUT1 10
                #define OUT2 11
                #define OUT3 12
                #define OUT4 13
                #define OUT5 14
                #define OUT6 15
                #define OUT7 16
                #define OUT8 17
                #define OUT9 18
                #define OUT10 19
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