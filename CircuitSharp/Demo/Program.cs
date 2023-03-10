using System;
using System.Threading;
using CircuitSharp.Components;
using CircuitSharp.Components.Base;
using CircuitSharp.Components.Chips;
using CircuitSharp.Core;

namespace Demo
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var circuit = new Circuit(error =>
            {
                Console.WriteLine(error.Code);
            });

            var voltageInput = circuit.Create<VoltageInput>(Voltage.WaveType.Dc);
            voltageInput.SetMaxVoltage(10);
            var resistor = circuit.Create<Resistor>(100);
            var switchSpst = circuit.Create<SwitchSPST>();
            switchSpst.Open();

            var ground = circuit.Create<Ground>();

            circuit.Connect(voltageInput.LeadPos, resistor.LeadIn);
            circuit.Connect(resistor.LeadOut, switchSpst.LeadA);
            circuit.Connect(switchSpst.LeadB, ground.LeadIn);

            circuit.StartSimulation(() =>
            {
                Console.WriteLine($"{circuit.GetTime()}: {resistor.GetVoltageDelta()} {resistor.GetCurrent()}");
            });
            
            var t = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(500);
                    switchSpst.Close();
                    Thread.Sleep(500);
                    switchSpst.Open();
                }
            });
            t.Start();
            

            /*var blinkCode = @"
            void setup() 
            {
                Serial.begin(9600);
                pinMode(5, OUTPUT);
                pinMode(2, INPUT);
                attachInterrupt(digitalPinToInterrupt(2), blink, RISING);
            }

            void blink()
            {
                Serial.println(""INTERRUPCAO"");
            }

            void loop()
            {
                Serial.println(""HIGH"");
                digitalWrite(5, HIGH);
                delay(1000);
                Serial.println(""LOW"");
                digitalWrite(5, LOW);
                delay(1000);
            }
            ";

            var print = new Action<byte>(b =>
            {
                Console.Write((char) b);
            });

            var aTmega328 = circuit.Create<ATmega328P>(blinkCode, print);

            var resistor = circuit.Create<Resistor>(100);
            var ground = circuit.Create<Ground>();

            circuit.Connect(aTmega328.PD5Lead, resistor.LeadIn);
            circuit.Connect(resistor.LeadOut, ground.LeadIn);

            circuit.StartSimulation(() =>
            {
                Console.WriteLine(Math.Round(circuit.GetTime() * 1000) + " :: " + resistor.GetVoltageDelta());
            });*/

            Console.ReadLine();
        }
    }
}