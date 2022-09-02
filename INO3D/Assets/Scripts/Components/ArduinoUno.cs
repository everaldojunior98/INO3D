using System;
using System.Collections.Generic;
using Assets.Scripts.Components.Base;
using Assets.Scripts.Managers;
using CircuitSharp.Components.Chips;
using CircuitSharp.Core;
using UnityEngine;

namespace Assets.Scripts.Components
{
    public class ArduinoUno : InoComponent
    {
        public override void GenerateCircuitElement()
        {
            var code = @"
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
                Serial.println(""A"");
                digitalWrite(5, HIGH);
                delay(1000);
                digitalWrite(5, LOW);
                delay(1000);
            }
            ";

            var print = new Action<byte>(b =>
            {
                UIManager.Instance.AddLog(((char) b).ToString(), 0);
            });

            var aTmega328P = SimulationManager.Instance.CreateElement<ATmega328P>(code, print);
            LeadByPortName = new Dictionary<string, Lead>
            {
                {"0", aTmega328P.PD0Lead},
                {"1", aTmega328P.PD1Lead},
                {"2", aTmega328P.PD2Lead},
                {"3", aTmega328P.PD3Lead},
                {"4", aTmega328P.PD4Lead},
                {"5", aTmega328P.PD5Lead},
                {"6", aTmega328P.PD6Lead},
                {"7", aTmega328P.PD7Lead},
                {"8", aTmega328P.PB0Lead},
                {"9", aTmega328P.PB1Lead},
                {"10", aTmega328P.PB2Lead},
                {"11", aTmega328P.PB3Lead},
                {"12", aTmega328P.PB4Lead},
                {"13", aTmega328P.PB5Lead},
                {"A0", aTmega328P.PC0Lead},
                {"A1", aTmega328P.PC1Lead},
                {"A2", aTmega328P.PC2Lead},
                {"A3", aTmega328P.PC3Lead},
                {"A4", aTmega328P.PC4Lead},
                {"A5", aTmega328P.PC5Lead},
                {"VCC", aTmega328P.VCCLead},
                {"GND", aTmega328P.GNDLead}
            };
        }

        public override void OnSimulationTick()
        {
        }

        protected override void SetupPorts()
        {
            DefaultHeight = 0;

            Ports.Add(Tuple.Create("3.3V", new Vector3(0.092f, 0.2f, -0.488f), PortType.Power, PinType.Female));
            Ports.Add(Tuple.Create("5V", new Vector3(0.141f, 0.2f, -0.488f), PortType.Power, PinType.Female));
            Ports.Add(Tuple.Create("GND", new Vector3(0.191f, 0.2f, -0.488f), PortType.Power, PinType.Female));
            Ports.Add(Tuple.Create("GND", new Vector3(0.243f, 0.2f, -0.488f), PortType.Power, PinType.Female));
            Ports.Add(Tuple.Create("GND", new Vector3(-0.092f, 0.2f, 0.476f), PortType.Power, PinType.Female));

            Ports.Add(Tuple.Create("A0", new Vector3(0.395f, 0.2f, -0.488f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("A1", new Vector3(0.445f, 0.2f, -0.488f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("A2", new Vector3(0.498f, 0.2f, -0.488f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("A3", new Vector3(0.548f, 0.2f, -0.488f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("A4", new Vector3(0.597f, 0.2f, -0.488f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("A5", new Vector3(0.652f, 0.2f, -0.488f), PortType.Analog, PinType.Female));

            Ports.Add(Tuple.Create("13", new Vector3(-0.044f, 0.2f, 0.476f), PortType.Digital, PinType.Female));
            Ports.Add(Tuple.Create("12", new Vector3(0.008f, 0.2f, 0.476f), PortType.Digital, PinType.Female));
            Ports.Add(Tuple.Create("11", new Vector3(0.061f, 0.2f, 0.476f), PortType.DigitalPwm, PinType.Female));
            Ports.Add(Tuple.Create("10", new Vector3(0.109f, 0.2f, 0.476f), PortType.DigitalPwm, PinType.Female));
            Ports.Add(Tuple.Create("9", new Vector3(0.163f, 0.2f, 0.476f), PortType.DigitalPwm, PinType.Female));
            Ports.Add(Tuple.Create("8", new Vector3(0.212f, 0.2f, 0.476f), PortType.Digital, PinType.Female));
            Ports.Add(Tuple.Create("7", new Vector3(0.294f, 0.2f, 0.476f), PortType.Digital, PinType.Female));
            Ports.Add(Tuple.Create("6", new Vector3(0.346f, 0.2f, 0.476f), PortType.DigitalPwm, PinType.Female));
            Ports.Add(Tuple.Create("5", new Vector3(0.393f, 0.2f, 0.476f), PortType.DigitalPwm, PinType.Female));
            Ports.Add(Tuple.Create("4", new Vector3(0.446f, 0.2f, 0.476f), PortType.Digital, PinType.Female));
            Ports.Add(Tuple.Create("3", new Vector3(0.498f, 0.2f, 0.476f), PortType.DigitalPwm, PinType.Female));
            Ports.Add(Tuple.Create("2", new Vector3(0.546f, 0.2f, 0.476f), PortType.Digital, PinType.Female));
            Ports.Add(Tuple.Create("1", new Vector3(0.598f, 0.2f, 0.476f), PortType.Digital, PinType.Female));
            Ports.Add(Tuple.Create("0", new Vector3(0.647f, 0.2f, 0.476f), PortType.Digital, PinType.Female));
        }

        public override SaveFile Save()
        {
            var saveFile = new ArduinoUnoSaveFile
            {
                PrefabName = "ArduinoUno",

                PositionX = transform.position.x,
                PositionY = transform.position.y,
                PositionZ = transform.position.z,

                RotationX = transform.eulerAngles.x,
                RotationY = transform.eulerAngles.y,
                RotationZ = transform.eulerAngles.z
            };

            return saveFile;
        }

        public override void Load(SaveFile saveFile)
        {
            if (saveFile is ArduinoUnoSaveFile arduinoUnoSaveFile)
            {
                transform.position = new Vector3(arduinoUnoSaveFile.PositionX, arduinoUnoSaveFile.PositionY,
                    arduinoUnoSaveFile.PositionZ);
                transform.eulerAngles = new Vector3(arduinoUnoSaveFile.RotationX, arduinoUnoSaveFile.RotationY,
                    arduinoUnoSaveFile.RotationZ);
            }
        }

        public override void Delete()
        {
            foreach (var inoPort in GeneratedPorts)
            {
                if (inoPort.IsConnected())
                {
                    var connectedComponent = inoPort.GetConnectedComponent();
                    connectedComponent?.Delete();
                }
            }

            Destroy(gameObject);
        }
    }
}