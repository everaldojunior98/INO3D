using System;
using System.Collections.Generic;
using System.Linq;
using CircuitSharp.Components.Base;
using CircuitSharp.Components.Chips.Utils;
using CircuitSharp.Core;
using CircuitSharp.Machines;
using CLanguage;
using CLanguage.Interpreter;
using static CircuitSharp.Components.Base.Pin;

namespace CircuitSharp.Components.Chips
{
    public class ATmega328P : CircuitElement
    {
        #region Constants

        public const short A0 = 14;
        public const short A1 = 15;
        public const short A2 = 16;
        public const short A3 = 17;
        public const short A4 = 18;
        public const short A5 = 19;

        private const double MaxVoltage = 5;
        private const double MinVoltage = 0;

        private const double AnalogWriteResolution = 8;
        private const double AnalogReadResolution = 10;

        private const double PwmFrequency = 490;

        #endregion

        #region Properties

        public new Lead PD0Lead => new Lead(this, 0);
        public new Lead PD1Lead => new Lead(this, 1);
        public new Lead PD2Lead => new Lead(this, 2);
        public new Lead PD3Lead => new Lead(this, 3);
        public new Lead PD4Lead => new Lead(this, 4);
        public new Lead PD5Lead => new Lead(this, 5);
        public new Lead PD6Lead => new Lead(this, 6);
        public new Lead PD7Lead => new Lead(this, 7);
        public new Lead PB0Lead => new Lead(this, 8);
        public new Lead PB1Lead => new Lead(this, 9);
        public new Lead PB2Lead => new Lead(this, 10);
        public new Lead PB3Lead => new Lead(this, 11);
        public new Lead PB4Lead => new Lead(this, 12);
        public new Lead PB5Lead => new Lead(this, 13);
        public new Lead PC0Lead => new Lead(this, 14);
        public new Lead PC1Lead => new Lead(this, 15);
        public new Lead PC2Lead => new Lead(this, 16);
        public new Lead PC3Lead => new Lead(this, 17);
        public new Lead PC4Lead => new Lead(this, 18);
        public new Lead PC5Lead => new Lead(this, 19);
        public new Lead PC6Lead => new Lead(this, 20);
        public new Lead VCCLead => new Lead(this, 21);
        public new Lead GNDLead => new Lead(this, 22);

        #endregion

        #region Fields

        private readonly double pwmPeriod;

        private const string InterpreterEntryPoint = "__cinit";
        private const string SetupEntryPoint = "setup";
        private const string LoopEntryPoint = "main";
        private readonly CInterpreter interpreter;

        private Pin[] pins;
        private Dictionary<int, Tuple<Action, int>> interruptions;

        private double[] pinTimeOn;
        private double[] pinTotalTime;

        private double sleepTime;

        private double frequency;
        private double freqTimeZero;

        private double currentTime;
        private double lastTime;

        private readonly Serial serial;

        #endregion

        #region Constructor

        public ATmega328P(string code, Action<byte> onSendSerialData)
        {
            serial = new Serial(64)
            {
                OnArduinoSend = onSendSerialData
            };

            var machine = new ATmega328PMachineInfo(this);
            var fullCode = code + "\n\nvoid main() { while(1){loop();}}";
            interpreter = CLanguageService.CreateInterpreter(fullCode, machine);
            interpreter.CpuSpeed = 10 ^ 9;

            frequency = PwmFrequency;
            pwmPeriod = 1 / PwmFrequency;
            SetupPins();
        }

        #endregion

        #region Public Methods

        public double GetPinVoltage(short pin)
        {
            if (IsValidPin(pin))
                return pins[pin].GetVoltage();
            return 0;
        }

        public void Delay(int value)
        {
            sleepTime = value * 1000;
        }
        
        public void DelayMicroseconds(int value)
        {
            sleepTime = value;
        }

        public ulong Micros()
        {
            return (ulong) Math.Round(currentTime);
        }

        public ulong Millis()
        {
            return (ulong) Math.Round(currentTime * 1000);
        }

        public void SerialBegin(int baud)
        {
            serial.Begin(baud);
        }
        
        public void SerialEnd()
        {
            serial.End();
        }
        
        public int SerialAvailable()
        {
            return serial.Available();
        }
        
        public int SerialPeek()
        {
            return serial.Peek();
        }
        
        public int SerialRead()
        {
            return serial.Read();
        }

        public void SerialFlush()
        {
            serial.Flush();
        }

        public int SerialPrint(object value, int format)
        {
           return serial.Print(value, format);
        }

        public int SerialPrintln(object value, int format)
        {
            return serial.Println(value, format);
        }

        public void WriteToArduino(string value)
        {
            serial.WriteToArduino(value);
        }

        public void SetPinMode(short pin, int mode)
        {
            if (mode >= 0 && mode <= 2 && IsValidPin(pin))
                pins[pin].Mode = (PinMode) mode;
        }

        public bool ReadDigitalPin(short pin)
        {
            if (IsValidPin(pin))
                return pins[pin].GetVoltage() >= MaxVoltage / 2;

            return false;
        }

        public void WriteDigitalPin(short pin, short value)
        {
            if (IsValidPin(pin))
                pins[pin].DutyCycle = value > 0 ? 1 : 0;
        }

        public int ReadAnalogPin(short pin)
        {
            if (IsValidPin(pin) && pins[pin].GetType() == PinType.Analog)
            {
                var maxInputValue = Math.Pow(2, AnalogReadResolution) - 1;
                var value = (int) (pins[pin].GetVoltage() * maxInputValue / MaxVoltage);
                return value;
            }

            return 0;
        }

        public void WriteAnalogPin(short pin, short value)
        {
            if (IsValidPin(pin))
            {
                var analogValue = Math.Min(Math.Max(value, MinVoltage),
                    Math.Pow(2, AnalogWriteResolution) - 1);

                var dutyCycle = Math.Round(analogValue / (Math.Pow(2, AnalogWriteResolution) - 1), 2);
                if (pins[pin].GetType() == PinType.Digital)
                    dutyCycle = dutyCycle >= 0.5 ? 1 : 0;

                pins[pin].DutyCycle = dutyCycle;
            }
        }

        public void NoTone(short pin)
        {
            if (IsValidPin(pin))
            {
                pins[pin].DutyCycle = 0;
                pins[pin].Frequency = PwmFrequency;
            }
        }

        public void Tone(short pin, int freq)
        {
            if (IsValidPin(pin))
            {
                pins[pin].DutyCycle = 0.5;
                pins[pin].Frequency = freq;
            }
        }

        public int DigitalPinToInterrupt(short pin)
        {
            if (IsValidPin(pin) && pins[pin].IsInterruptPin)
                return pins[pin].InterruptIndex;
            return -1;
        }

        public void AttachInterrupt(int pin, Action isr, int mode)
        {
            if (pins.Any(p => p.IsInterruptPin && p.InterruptIndex == pin))
                interruptions[pin] = Tuple.Create(isr, mode);
        }

        public void DetachInterrupt(int pin)
        {
            if (interruptions.ContainsKey(pin))
                interruptions.Remove(pin);
        }

        #endregion

        #region Private Methods

        private bool IsValidPin(short pin)
        {
            return pin >= 0 && pin < pins.Length && !pins[pin].IsControlPin;
        }

        private void SetupPins()
        {
            interruptions = new Dictionary<int, Tuple<Action, int>>();
            pinTimeOn = new double[GetLeadCount()];
            pinTotalTime = new double[GetLeadCount()];
            pins = new Pin[GetLeadCount()];

            //Digital Pins
            //0 - RXD
            pins[0] = new Pin("PD0", MaxVoltage, PwmFrequency, PinType.Digital);
            //1 - TXD
            pins[1] = new Pin("PD1",  MaxVoltage, PwmFrequency, PinType.Digital);
            //2
            pins[2] = new Pin("PD2",  MaxVoltage, PwmFrequency, PinType.Digital)
            {
                IsInterruptPin = true,
                InterruptIndex = 0
            };
            //3
            pins[3] = new Pin("PD3",  MaxVoltage, PwmFrequency, PinType.DigitalPwm)
            {
                IsInterruptPin = true,
                InterruptIndex = 1
            };
            //4
            pins[4] = new Pin("PD4",  MaxVoltage, PwmFrequency, PinType.DigitalPwm);
            //5
            pins[5] = new Pin("PD5",  MaxVoltage, PwmFrequency, PinType.DigitalPwm);
            //6
            pins[6] = new Pin("PD6",  MaxVoltage, PwmFrequency, PinType.DigitalPwm);
            //7
            pins[7] = new Pin("PD7",  MaxVoltage, PwmFrequency, PinType.Digital);
            //8
            pins[8] = new Pin("PB0",  MaxVoltage, PwmFrequency, PinType.Digital);
            //9
            pins[9] = new Pin("PB1",  MaxVoltage, PwmFrequency, PinType.DigitalPwm);
            //10
            pins[10] = new Pin("PB2",  MaxVoltage, PwmFrequency, PinType.DigitalPwm);
            //11
            pins[11] = new Pin("PB3",  MaxVoltage, PwmFrequency, PinType.DigitalPwm);
            //12
            pins[12] = new Pin("PB4",  MaxVoltage, PwmFrequency, PinType.Digital);
            //13
            pins[13] = new Pin("PB5",  MaxVoltage, PwmFrequency, PinType.Digital);

            //Analog Pins
            //A0
            pins[14] = new Pin("PC0",  MaxVoltage, PwmFrequency, PinType.Analog);
            //A1
            pins[15] = new Pin("PC1",  MaxVoltage, PwmFrequency, PinType.Analog);
            //A2
            pins[16] = new Pin("PC2",  MaxVoltage, PwmFrequency, PinType.Analog);
            //A3
            pins[17] = new Pin("PC3",  MaxVoltage, PwmFrequency, PinType.Analog);
            //A4
            pins[18] = new Pin("PC4",  MaxVoltage, PwmFrequency, PinType.Analog);
            //A5
            pins[19] = new Pin("PC5",  MaxVoltage, PwmFrequency, PinType.Analog);

            //Control Pins
            //Reset
            pins[20] = new Pin("PC6",  MaxVoltage, PwmFrequency, PinType.Analog)
            {
                Mode = PinMode.Input,
                IsControlPin = true
            };
            //VCC
            pins[21] = new Pin("VCC",  MaxVoltage, PwmFrequency, PinType.Digital)
            {
                Mode = PinMode.Output,
                DutyCycle = 1,
                IsControlPin = true
            };
            //GND
            pins[22] = new Pin("GND",  MaxVoltage, PwmFrequency, PinType.Digital)
            {
                Mode = PinMode.Output,
                IsControlPin = true
            };

            interpreter.Reset(InterpreterEntryPoint);
            interpreter.Run();

            interpreter.Reset(SetupEntryPoint);
            interpreter.Run();

            AllocLeads();

            interpreter.Reset(LoopEntryPoint);
        }
        private double GetVoltage(Circuit circuit, Pin pin)
        {
            var newFreq = pin.Frequency;
            var timeStep = circuit.GetTimeStep();
            var time = circuit.GetTime();

            var oldFreq = frequency;
            frequency = newFreq;
            var maxFreq = 1 / (8 * timeStep);
            if (frequency > maxFreq)
                frequency = maxFreq;
            freqTimeZero = time - oldFreq * (time - freqTimeZero) / frequency;

            var w = 2 * Pi * (circuit.GetTime() - freqTimeZero) * frequency;
            return w % (2 * Pi) > 2 * Pi * pin.DutyCycle ? 0 : MaxVoltage;
        }

        #endregion

        #region Overrides

        public override void SetCurrent(int lead, double current)
        {
            for (var i = 0; i != GetLeadCount(); i++)
                if (pins[i].Mode == PinMode.Output && pins[i].VoltSourceId == lead)
                    pins[i].Current = current;
        }

        public override void SetVoltageSource(int j, int vs)
        {
            for (var i = 0; i != GetLeadCount(); i++)
            {
                var pin = pins[i];
                if (pin.Mode == PinMode.Output && j-- == 0)
                    pin.VoltSourceId = vs;
            }
        }

        public override void Stamp(Circuit circuit)
        {
            for (var i = 0; i != GetLeadCount(); i++)
            {
                var pin = pins[i];
                if (pin.Mode == PinMode.Output)
                    circuit.StampVoltageSource(0, LeadNode[i], pin.VoltSourceId);
            }
        }

        public override void Step(Circuit circuit)
        {
            currentTime = circuit.GetTime();

            var tickTime = (currentTime - lastTime) * 1E6;
            sleepTime -= tickTime;

            for (var i = 0; i != GetLeadCount(); i++)
            {
                var pin = pins[i];
                if (pin.Mode == PinMode.Input || pin.Mode == PinMode.InputPullup)
                {
                    pinTotalTime[i] += circuit.GetTimeStep();
                    if (LeadVolt[i] > MaxVoltage / 2)
                        pinTimeOn[i] += circuit.GetTimeStep();

                    if (pinTotalTime[i] >= pwmPeriod)
                    {
                        var oldState = pin.GetVoltage() > MaxVoltage / 2;
                        pin.DutyCycle = Math.Round(pinTimeOn[i] / pinTotalTime[i], 2);
                        var currentState = pin.GetVoltage() > MaxVoltage / 2;

                        pinTotalTime[i] = 0;
                        pinTimeOn[i] = 0;

                        if (pin.IsInterruptPin && interruptions.ContainsKey(pin.InterruptIndex))
                        {
                            var interruption = interruptions[pin.InterruptIndex];
                            if (interruption.Item2 == 0) //LOW
                            {
                                if (!currentState)
                                    interruption.Item1();
                            }
                            else if (interruption.Item2 == 1) //CHANGE
                            {
                                if (currentState != oldState)
                                    interruption.Item1();
                            }
                            else if (interruption.Item2 == 2) //RISING 
                            {
                                if (!oldState && currentState)
                                    interruption.Item1();
                            }
                            else if (interruption.Item2 == 3) //FALLING 
                            {
                                if (oldState && !currentState)
                                    interruption.Item1();
                            }
                        }
                    }
                }
            }

            if (sleepTime <= 0 || sleepTime - tickTime <= 0)
            {
                sleepTime = 0;
                interpreter.Step((int) tickTime);
            }

            for (var i = 0; i != GetLeadCount(); i++)
            {
                var pin = pins[i];
                if (pin.Mode == PinMode.Output)
                    circuit.UpdateVoltageSource(0, i, pin.VoltSourceId, GetVoltage(circuit, pin));
            }

            lastTime = circuit.GetTime();
        }

        public override void Reset()
        {
            for (var i = 0; i != GetLeadCount(); i++)
            {
                if (!pins[i].IsControlPin)
                    pins[i].DutyCycle = 0;
                pins[i].Current = 0;

                pinTotalTime[i] = 0;
                pinTimeOn[i] = 0;

                LeadVolt[i] = 0;
            }

            interpreter.Reset(SetupEntryPoint);
            interpreter.Run();

            interpreter.Reset(LoopEntryPoint);
            sleepTime = 0;
        }

        public override bool LeadsAreConnected(int lead1, int lead2)
        {
            return false;
        }

        public override bool LeadIsGround(int lead)
        {
            return pins[lead].Mode == PinMode.Output;
        }

        public override int GetVoltageSourceCount()
        {
            return pins.Count(pin => pin.Mode == PinMode.Output);
        }

        public override int GetLeadCount()
        {
            return 23;
        }

        #endregion
    }
}