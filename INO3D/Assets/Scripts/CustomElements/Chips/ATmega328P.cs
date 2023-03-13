using Assets.Scripts.CustomElements.Machines;
using CLanguage.Interpreter;
using CLanguage;
using SharpCircuit;
using static Assets.Scripts.CustomElements.Chips.ATmegaPin;
using static SharpCircuit.Circuit;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Assets.Scripts.CustomElements.Chips
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

        private ATmegaPin[] pins;
        private Dictionary<int, Tuple<Action, int>> interruptions;

        private double[] pinTimeOn;
        private double[] pinTotalTime;

        private double sleepTime;

        private double frequency;
        private double freqTimeZero;

        private double currentTime;
        private double lastTime;

        private readonly ATmegaSerial serial;

        #endregion

        #region Constructor

        public ATmega328P(string code, Action<byte> onSendSerialData)
        {
            serial = new ATmegaSerial(64)
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
            return (ulong)Math.Round(currentTime);
        }

        public ulong Millis()
        {
            return (ulong)Math.Round(currentTime * 1000);
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
                pins[pin].Mode = (ATmegaPinMode)mode;
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
            if (IsValidPin(pin) && pins[pin].GetType() == ATmegaPinType.Analog)
            {
                var maxInputValue = Math.Pow(2, AnalogReadResolution) - 1;
                var value = (int)(pins[pin].GetVoltage() * maxInputValue / MaxVoltage);
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
                if (pins[pin].GetType() == ATmegaPinType.Digital)
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
            pinTimeOn = new double[getLeadCount()];
            pinTotalTime = new double[getLeadCount()];
            pins = new ATmegaPin[getLeadCount()];

            //Digital Pins
            //0 - RXD
            pins[0] = new ATmegaPin("PD0", MaxVoltage, PwmFrequency, ATmegaPinType.Digital);
            //1 - TXD
            pins[1] = new ATmegaPin("PD1", MaxVoltage, PwmFrequency, ATmegaPinType.Digital);
            //2
            pins[2] = new ATmegaPin("PD2", MaxVoltage, PwmFrequency, ATmegaPinType.Digital)
            {
                IsInterruptPin = true,
                InterruptIndex = 0
            };
            //3
            pins[3] = new ATmegaPin("PD3", MaxVoltage, PwmFrequency, ATmegaPinType.DigitalPwm)
            {
                IsInterruptPin = true,
                InterruptIndex = 1
            };
            //4
            pins[4] = new ATmegaPin("PD4", MaxVoltage, PwmFrequency, ATmegaPinType.DigitalPwm);
            //5
            pins[5] = new ATmegaPin("PD5", MaxVoltage, PwmFrequency, ATmegaPinType.DigitalPwm);
            //6
            pins[6] = new ATmegaPin("PD6", MaxVoltage, PwmFrequency, ATmegaPinType.DigitalPwm);
            //7
            pins[7] = new ATmegaPin("PD7", MaxVoltage, PwmFrequency, ATmegaPinType.Digital);
            //8
            pins[8] = new ATmegaPin("PB0", MaxVoltage, PwmFrequency, ATmegaPinType.Digital);
            //9
            pins[9] = new ATmegaPin("PB1", MaxVoltage, PwmFrequency, ATmegaPinType.DigitalPwm);
            //10
            pins[10] = new ATmegaPin("PB2", MaxVoltage, PwmFrequency, ATmegaPinType.DigitalPwm);
            //11
            pins[11] = new ATmegaPin("PB3", MaxVoltage, PwmFrequency, ATmegaPinType.DigitalPwm);
            //12
            pins[12] = new ATmegaPin("PB4", MaxVoltage, PwmFrequency, ATmegaPinType.Digital);
            //13
            pins[13] = new ATmegaPin("PB5", MaxVoltage, PwmFrequency, ATmegaPinType.Digital);

            //Analog Pins
            //A0
            pins[14] = new ATmegaPin("PC0", MaxVoltage, PwmFrequency, ATmegaPinType.Analog);
            //A1
            pins[15] = new ATmegaPin("PC1", MaxVoltage, PwmFrequency, ATmegaPinType.Analog);
            //A2
            pins[16] = new ATmegaPin("PC2", MaxVoltage, PwmFrequency, ATmegaPinType.Analog);
            //A3
            pins[17] = new ATmegaPin("PC3", MaxVoltage, PwmFrequency, ATmegaPinType.Analog);
            //A4
            pins[18] = new ATmegaPin("PC4", MaxVoltage, PwmFrequency, ATmegaPinType.Analog);
            //A5
            pins[19] = new ATmegaPin("PC5", MaxVoltage, PwmFrequency, ATmegaPinType.Analog);

            //Control Pins
            //Reset
            pins[20] = new ATmegaPin("PC6", MaxVoltage, PwmFrequency, ATmegaPinType.Analog)
            {
                Mode = ATmegaPinMode.Input,
                IsControlPin = true
            };
            //VCC
            pins[21] = new ATmegaPin("VCC", MaxVoltage, PwmFrequency, ATmegaPinType.Digital)
            {
                Mode = ATmegaPinMode.Output,
                DutyCycle = 1,
                IsControlPin = true
            };
            //GND
            pins[22] = new ATmegaPin("GND", MaxVoltage, PwmFrequency, ATmegaPinType.Digital)
            {
                Mode = ATmegaPinMode.Output,
                IsControlPin = true
            };

            interpreter.Reset(InterpreterEntryPoint);
            interpreter.Run();

            interpreter.Reset(SetupEntryPoint);
            interpreter.Run();

            allocLeads();

            interpreter.Reset(LoopEntryPoint);
        }

        private double GetVoltage(Circuit circuit, ATmegaPin pin)
        {
            var newFreq = pin.Frequency;
            var timeStep = circuit.timeStep;
            var time = circuit.time;

            var oldFreq = frequency;
            frequency = newFreq;
            var maxFreq = 1 / (8 * timeStep);
            if (frequency > maxFreq)
                frequency = maxFreq;
            freqTimeZero = time - oldFreq * (time - freqTimeZero) / frequency;

            var w = 2 * Math.PI * (circuit.time - freqTimeZero) * frequency;
            return w % (2 * Math.PI) > 2 * Math.PI * pin.DutyCycle ? 0 : MaxVoltage;
        }

        #endregion

        #region Overrides

        public override void setCurrent(int l, double c)
        {
            for (var i = 0; i != getLeadCount(); i++)
                if (pins[i].Mode == ATmegaPinMode.Output && pins[i].VoltSourceId == l)
                    pins[i].Current = c;
        }

        public override void setVoltageSource(int j, int vs)
        {
            for (var i = 0; i != getLeadCount(); i++)
            {
                var pin = pins[i];
                if (pin.Mode == ATmegaPinMode.Output && j-- == 0)
                    pin.VoltSourceId = vs;
            }
        }

        public override void stamp(Circuit circuit)
        {
            for (var i = 0; i != getLeadCount(); i++)
            {
                var pin = pins[i];
                if (pin.Mode == ATmegaPinMode.Output)
                    circuit.stampVoltageSource(0, lead_node[i], pin.VoltSourceId);
            }
        }

        public override void step(Circuit circuit)
        {
            currentTime = circuit.time;

            var tickTime = (currentTime - lastTime) * 1E6;
            sleepTime -= tickTime;

            for (var i = 0; i != getLeadCount(); i++)
            {
                var pin = pins[i];
                if (pin.Mode == ATmegaPinMode.Input || pin.Mode == ATmegaPinMode.InputPullup)
                {
                    pinTotalTime[i] += circuit.timeStep;
                    if (lead_volt[i] > MaxVoltage / 2)
                        pinTimeOn[i] += circuit.timeStep;

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
                interpreter.Step((int)tickTime);
            }

            for (var i = 0; i != getLeadCount(); i++)
            {
                var pin = pins[i];
                if (pin.Mode == ATmegaPinMode.Output)
                    circuit.updateVoltageSource(0, i, pin.VoltSourceId, GetVoltage(circuit, pin));
            }

            lastTime = circuit.time;
        }

        public override void reset()
        {
            for (var i = 0; i != getLeadCount(); i++)
            {
                if (!pins[i].IsControlPin)
                    pins[i].DutyCycle = 0;
                pins[i].Current = 0;

                pinTotalTime[i] = 0;
                pinTimeOn[i] = 0;

                lead_volt[i] = 0;
            }

            interpreter.Reset(SetupEntryPoint);
            interpreter.Run();

            interpreter.Reset(LoopEntryPoint);
            sleepTime = 0;
        }

        public override bool leadsAreConnected(int l1, int l2)
        {
            return false;
        }

        public override bool leadIsGround(int l)
        {
            return pins[l].Mode == ATmegaPinMode.Output;
        }

        public override int getVoltageSourceCount()
        {
            return pins.Count(pin => pin.Mode == ATmegaPinMode.Output);
        }

        public override int getLeadCount()
        {
            return 23;
        }

        #endregion
    }
}