using System;
using Assets.Scripts.Components.Base;
using UnityEngine;

namespace Assets.Scripts.Components
{
    public class ArduinoUno : InoComponent
    {
        protected override void SetupPorts()
        {
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

        public override void Delete()
        {
            foreach (var inoPort in GeneratedPorts)
            {
                if (inoPort.IsConnected())
                {
                    var connectedComponent = inoPort.GetConnectedComponent();
                    connectedComponent.Delete();
                }
            }

            Destroy(gameObject);
        }
    }
}