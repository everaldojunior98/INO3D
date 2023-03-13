using System;
using Assets.Scripts.CustomElements.Chips;
using CLanguage;

namespace Assets.Scripts.CustomElements.Machines
{
    public class ATmega328PMachineInfo : MachineInfo
    {
        #region Fields

        private readonly ATmega328P aTmega328P;
        private Random randomGenerator;

        #endregion

        #region Constructor

        public ATmega328PMachineInfo(ATmega328P chip)
        {
            aTmega328P = chip;
            randomGenerator = new Random();

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
                #define HIGH 1
                #define LOW 0
                #define INPUT 0
                #define INPUT_PULLUP 2
                #define OUTPUT 1
                #define CHANGE 1
                #define RISING 2
                #define FALLING 3
                #define A0 14
                #define A1 15
                #define A2 16
                #define A3 17
                #define A4 18
                #define A5 19
                #define DEC 10
                #define HEX 16
                #define OCT 8
                #define BIN 2
                typedef bool boolean;
                typedef unsigned char byte;
                typedef unsigned short word;
                struct SerialClass
                {
                    void begin (int baud);
                    void end ();
                    int available ();
                    int peek ();
                    int read ();
                    void flush ();
                    int print (const char *value);
                    int println (const char *value);
                    int print (bool value);
                    int print (bool value, int format);
                    int println (bool value);
                    int println (bool value, int format);
                    int print (double value);
                    int print (double value, int format);
                    int println (double value);
                    int println (double value, int format);
                };
                struct SerialClass Serial;
                ";

            #region Digital / Analog I/O

            AddInternalFunction("void pinMode (int pin, int mode)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int16Value;
                var mode = interpreter.ReadArg(1).Int16Value;
                aTmega328P.SetPinMode(pin, mode);
            });

            AddInternalFunction("int digitalRead (int pin)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int16Value;
                var value = aTmega328P.ReadDigitalPin(pin);
                interpreter.Push(value);
            });

            AddInternalFunction("void digitalWrite (int pin, int value)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int16Value;
                var value = interpreter.ReadArg(1).Int16Value;
                aTmega328P.WriteDigitalPin(pin, value);
            });

            AddInternalFunction("int analogRead (int pin)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int16Value;
                var value = aTmega328P.ReadAnalogPin(pin);
                interpreter.Push(value);
            });

            AddInternalFunction("void analogWrite (int pin, int value)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int16Value;
                var value = interpreter.ReadArg(1).Int16Value;
                aTmega328P.WriteAnalogPin(pin, value);
            });

            #endregion

            #region Advanced I/O

            AddInternalFunction("void noTone (int pin)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int16Value;
                aTmega328P.NoTone(pin);
            });

            AddInternalFunction("void tone (int pin, int frequency)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int16Value;
                var frequency = interpreter.ReadArg(0).Int32Value;
                aTmega328P.Tone(pin, frequency);
            });

            #endregion

            #region Time

            AddInternalFunction("void delay (unsigned long ms)", interpreter =>
            {
                var value = (int) interpreter.ReadArg(0).UInt64Value;
                aTmega328P.Delay(value);
            });

            AddInternalFunction("void delayMicroseconds (unsigned long us)", interpreter =>
            {
                var value = (int) interpreter.ReadArg(0).UInt64Value;
                aTmega328P.DelayMicroseconds(value);
            });

            AddInternalFunction("unsigned long micros ()", interpreter => { interpreter.Push(aTmega328P.Micros()); });

            AddInternalFunction("long millis ()", interpreter => { interpreter.Push(aTmega328P.Millis()); });

            #endregion

            #region Math

            AddInternalFunction("double abs (double x)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Float64Value;
                interpreter.Push(Math.Abs(x));
            });

            AddInternalFunction("double constrain (double x, double a, double b)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Float64Value;
                var a = interpreter.ReadArg(1).Float64Value;
                var b = interpreter.ReadArg(2).Float64Value;

                if (x > b)
                    interpreter.Push(b);
                else if (x < a)
                    interpreter.Push(a);
                else
                    interpreter.Push(x);
            });

            AddInternalFunction("double map (double x, double inMin, double inMax, double outMin, double outMax)",
                interpreter =>
                {
                    var x = interpreter.ReadArg(0).Float64Value;
                    var inMin = interpreter.ReadArg(1).Float64Value;
                    var inMax = interpreter.ReadArg(2).Float64Value;
                    var outMin = interpreter.ReadArg(3).Float64Value;
                    var outMax = interpreter.ReadArg(4).Float64Value;

                    interpreter.Push((x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin);
                });

            AddInternalFunction("double max (double x, double y)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Float64Value;
                var y = interpreter.ReadArg(1).Float64Value;

                interpreter.Push(x > y ? x : y);
            });

            AddInternalFunction("double min (double x, double y)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Float64Value;
                var y = interpreter.ReadArg(1).Float64Value;

                interpreter.Push(x < y ? x : y);
            });

            AddInternalFunction("double pow (double x, double y)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Float64Value;
                var y = interpreter.ReadArg(1).Float64Value;

                interpreter.Push(Math.Pow(x, y));
            });

            AddInternalFunction("double sq (double x)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Float64Value;

                interpreter.Push(x * x);
            });

            AddInternalFunction("double sqrt (double x)", interpreter =>
            {
                var x = interpreter.ReadArg(0).Float64Value;

                interpreter.Push(Math.Sqrt(x));
            });

            #endregion

            #region Trigonometry

            AddInternalFunction("double cos (double rad)", interpreter =>
            {
                var rad = interpreter.ReadArg(0).Float64Value;
                interpreter.Push(Math.Cos(rad));
            });

            AddInternalFunction("double sin (double rad)", interpreter =>
            {
                var rad = interpreter.ReadArg(0).Float64Value;
                interpreter.Push(Math.Sin(rad));
            });

            AddInternalFunction("double tan (double rad)", interpreter =>
            {
                var rad = interpreter.ReadArg(0).Float64Value;
                interpreter.Push(Math.Tan(rad));
            });

            #endregion

            #region Characters

            AddInternalFunction("bool isAlpha (char thisChar)", interpreter =>
            {
                var thisChar = interpreter.ReadArg(0).CharValue;
                interpreter.Push(char.IsLetter(thisChar));
            });

            AddInternalFunction("bool isAlphaNumeric (const char thisChar)", interpreter =>
            {
                var thisChar = interpreter.ReadArg(0).CharValue;
                interpreter.Push(char.IsLetterOrDigit(thisChar));
            });

            AddInternalFunction("bool isAscii (const char thisChar)", interpreter =>
            {
                var thisChar = interpreter.ReadArg(0).CharValue;
                interpreter.Push(thisChar <= sbyte.MaxValue);
            });

            AddInternalFunction("bool isControl (const char thisChar)", interpreter =>
            {
                var thisChar = interpreter.ReadArg(0).CharValue;
                interpreter.Push(char.IsControl(thisChar));
            });

            AddInternalFunction("bool isGraph (const char thisChar)", interpreter =>
            {
                var thisChar = interpreter.ReadArg(0).CharValue;
                interpreter.Push(!char.IsControl(thisChar) || char.IsWhiteSpace(thisChar));
            });

            AddInternalFunction("bool isHexadecimalDigit (const char thisChar)", interpreter =>
            {
                var thisChar = interpreter.ReadArg(0).CharValue;
                interpreter.Push(Uri.IsHexDigit(thisChar));
            });

            AddInternalFunction("bool isLowerCase (const char thisChar)", interpreter =>
            {
                var thisChar = interpreter.ReadArg(0).CharValue;
                interpreter.Push(char.IsLower(thisChar));
            });

            AddInternalFunction("bool isPrintable (const char thisChar)", interpreter =>
            {
                var thisChar = interpreter.ReadArg(0).CharValue;
                interpreter.Push(!char.IsControl(thisChar));
            });

            AddInternalFunction("bool isPunct (const char thisChar)", interpreter =>
            {
                var thisChar = interpreter.ReadArg(0).CharValue;
                interpreter.Push(char.IsPunctuation(thisChar));
            });

            AddInternalFunction("bool isSpace (const char thisChar)", interpreter =>
            {
                var thisChar = interpreter.ReadArg(0).CharValue;
                interpreter.Push(char.IsWhiteSpace(thisChar) || thisChar == '\f' || thisChar == '\n' ||
                                 thisChar == '\r' || thisChar == '\t' || thisChar == '\v');
            });

            AddInternalFunction("bool isUpperCase (const char thisChar)", interpreter =>
            {
                var thisChar = interpreter.ReadArg(0).CharValue;
                interpreter.Push(char.IsUpper(thisChar));
            });

            AddInternalFunction("bool isWhitespace (const char thisChar)", interpreter =>
            {
                var thisChar = interpreter.ReadArg(0).CharValue;
                interpreter.Push(char.IsWhiteSpace(thisChar));
            });

            #endregion

            #region Random Numbers

            AddInternalFunction("void randomSeed (unsigned long seed)", interpreter =>
            {
                var seed = interpreter.ReadArg(0).Int64Value;
                if (seed > 0)
                    randomGenerator = new Random((int) seed);
            });

            AddInternalFunction("long random (long max)", interpreter =>
            {
                var max = interpreter.ReadArg(0).Int64Value;
                interpreter.Push(randomGenerator.Next((int) max));
            });

            AddInternalFunction("long random (long min, long max)", interpreter =>
            {
                var min = interpreter.ReadArg(0).Int64Value;
                var max = interpreter.ReadArg(1).Int64Value;
                interpreter.Push(randomGenerator.Next((int) min, (int) max));
            });

            #endregion

            #region Bits and Bytes

            AddInternalFunction("long bit (long n)", interpreter =>
            {
                var n = (int) interpreter.ReadArg(0).Int64Value;
                interpreter.Push(1 << n);
            });

            AddInternalFunction("long bitClear (long x, long n)", interpreter =>
            {
                var x = (int) interpreter.ReadArg(0).Int64Value;
                var n = (int) interpreter.ReadArg(1).Int64Value;

                interpreter.Push(x & ~(1 << n));
            });

            AddInternalFunction("long bitRead (long x, long n)", interpreter =>
            {
                var x = (int) interpreter.ReadArg(0).Int64Value;
                var n = (int) interpreter.ReadArg(1).Int64Value;

                interpreter.Push((x >> n) & 1);
            });

            AddInternalFunction("long bitSet (long x, long n)", interpreter =>
            {
                var x = (int) interpreter.ReadArg(0).Int64Value;
                var n = (int) interpreter.ReadArg(1).Int64Value;

                interpreter.Push(x | (1 << n));
            });

            AddInternalFunction("long bitWrite (long x, long n, long b)", interpreter =>
            {
                var x = (int) interpreter.ReadArg(0).Int64Value;
                var n = (int) interpreter.ReadArg(1).Int64Value;
                var b = (int) interpreter.ReadArg(2).Int64Value;

                if (b == 1)
                    interpreter.Push(x | (1 << n));
                else
                    interpreter.Push(x & ~(1 << n));
            });

            AddInternalFunction("long highByte (long x)", interpreter =>
            {
                var x = (int) interpreter.ReadArg(0).Int64Value;
                interpreter.Push((byte) (x >> 8));
            });

            AddInternalFunction("long lowByte (long x)", interpreter =>
            {
                var x = (int) interpreter.ReadArg(0).Int64Value;
                interpreter.Push((byte) x);
            });

            #endregion

            #region External Interrupts

            AddInternalFunction("int digitalPinToInterrupt (int pin)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int32Value;
                interpreter.Push(aTmega328P.DigitalPinToInterrupt((short) pin));
            });

            AddInternalFunction("void attachInterrupt (int pin, void (*isr)(), int mode)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int32Value;
                var isr = interpreter.ReadArg(1);
                var mode = interpreter.ReadArg(2).Int32Value;

                aTmega328P.AttachInterrupt(pin, () => interpreter.RunFunction(isr, 1000000), mode);
            });

            AddInternalFunction("void detachInterrupt (int pin)", interpreter =>
            {
                var pin = interpreter.ReadArg(0).Int32Value;
                aTmega328P.DetachInterrupt(pin);
            });

            #endregion

            #region Serial

            AddInternalFunction("void SerialClass::begin (int baud)", interpreter =>
            {
                var baud = interpreter.ReadArg(0).Int16Value;
                aTmega328P.SerialBegin(baud);
            });

            AddInternalFunction("void SerialClass::end ()", interpreter => { aTmega328P.SerialEnd(); });

            AddInternalFunction("int SerialClass::available ()",
                interpreter => { interpreter.Push(aTmega328P.SerialAvailable()); });

            AddInternalFunction("int SerialClass::peek ()",
                interpreter => { interpreter.Push(aTmega328P.SerialPeek()); });

            AddInternalFunction("int SerialClass::read ()",
                interpreter => { interpreter.Push(aTmega328P.SerialRead()); });

            AddInternalFunction("void SerialClass::flush ()", interpreter => { aTmega328P.SerialFlush(); });

            AddInternalFunction("int SerialClass::print (const char *value)", interpreter =>
            {
                interpreter.Push(aTmega328P.SerialPrint(interpreter.ReadString(interpreter.ReadArg(0).PointerValue),
                    -1));
            });

            AddInternalFunction("int SerialClass::println (const char *value)", interpreter =>
            {
                interpreter.Push(
                    aTmega328P.SerialPrintln(interpreter.ReadString(interpreter.ReadArg(0).PointerValue), -1));
            });

            AddInternalFunction("int SerialClass::print (bool value)",
                interpreter => { interpreter.Push(aTmega328P.SerialPrint(interpreter.ReadArg(0).Int32Value, -1)); });

            AddInternalFunction("int SerialClass::print (bool value, int format)", interpreter =>
            {
                interpreter.Push(aTmega328P.SerialPrint(interpreter.ReadArg(0).Int32Value,
                    interpreter.ReadArg(1).Int16Value));
            });

            AddInternalFunction("int SerialClass::println (bool value)",
                interpreter => { interpreter.Push(aTmega328P.SerialPrintln(interpreter.ReadArg(0).Int32Value, -1)); });

            AddInternalFunction("int SerialClass::println (bool value, int format)", interpreter =>
            {
                interpreter.Push(aTmega328P.SerialPrintln(interpreter.ReadArg(0).Int32Value,
                    interpreter.ReadArg(1).Int16Value));
            });

            AddInternalFunction("int SerialClass::print (double value)",
                interpreter => { interpreter.Push(aTmega328P.SerialPrint(interpreter.ReadArg(0).Float64Value, -1)); });

            AddInternalFunction("int SerialClass::print (double value, int format)", interpreter =>
            {
                interpreter.Push(aTmega328P.SerialPrint(interpreter.ReadArg(0).Float64Value,
                    interpreter.ReadArg(1).Int16Value));
            });

            AddInternalFunction("int SerialClass::println (double value)",
                interpreter =>
                {
                    interpreter.Push(aTmega328P.SerialPrintln(interpreter.ReadArg(0).Float64Value, -1));
                });

            AddInternalFunction("int SerialClass::println (double value, int format)", interpreter =>
            {
                interpreter.Push(aTmega328P.SerialPrintln(interpreter.ReadArg(0).Float64Value,
                    interpreter.ReadArg(1).Int16Value));
            });

            #endregion
        }

        #endregion
    }
}