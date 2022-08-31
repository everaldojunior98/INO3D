using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Components.Base;
using Assets.Scripts.Managers;
using CircuitSharp.Components;
using CircuitSharp.Core;
using UnityEngine;

namespace Assets.Scripts.Components
{
    public class Protoboard400 : InoComponent
    {
        #region Overrides

        public override void GenerateCircuitElement()
        {
            var vcc = new List<InoPort>();
            var gnd = new List<InoPort>();
            var lines = new Dictionary<int, List<InoPort>>();
            foreach (var inoPort in GeneratedPorts)
            {
                if (inoPort.PortName.Contains("VCC"))
                {
                    vcc.Add(inoPort);
                }
                else if (inoPort.PortName.Contains("GND"))
                {
                    gnd.Add(inoPort);
                }
                else
                {
                    var portNumber = int.Parse(inoPort.PortName.Substring(1));
                    if (!lines.ContainsKey(portNumber))
                        lines.Add(portNumber, new List<InoPort>());
                    lines[portNumber].Add(inoPort);
                }
            }

            LeadByPortName = new Dictionary<string, Lead>();

            var vccOutput = SimulationManager.Instance.CreateElement<Output>();
            var vccCount = 0;
            foreach (var inoPort in vcc)
            {
                LeadByPortName.Add(inoPort.PortName, vccOutput.LeadIn);
                vccCount++;

                if (vccCount == 25)
                    vccOutput = SimulationManager.Instance.CreateElement<Output>();
            }

            var gndOutput = SimulationManager.Instance.CreateElement<Output>();
            var gndCount = 0;
            foreach (var inoPort in gnd)
            {
                LeadByPortName.Add(inoPort.PortName, gndOutput.LeadIn);
                gndCount++;

                if (gndCount == 25)
                    gndOutput = SimulationManager.Instance.CreateElement<Output>();
            }

            foreach (var pair in lines)
            {
                var output = SimulationManager.Instance.CreateElement<Output>();
                var count = 0;
                if(pair.Value.Any(port => port.IsConnected()))
                {
                    foreach (var inoPort in pair.Value)
                    {
                        LeadByPortName.Add(inoPort.PortName, output.LeadIn);
                        count++;

                        if (count == 5)
                            output = SimulationManager.Instance.CreateElement<Output>();
                    }
                }
            }
        }

        public override void OnSimulationTick()
        {
        }

        protected override void SetupPorts()
        {
            DefaultHeight = 0;
            //Setup dos pins do arduino
            var yOffset = 0.052f;

            //A
            for (var i = 0; i < 30; i++)
            {
                var index = i + 1;
                var pinName = $"A{index}";
                Ports.Add(Tuple.Create(pinName, new Vector3(-0.2808f, 0.1584f, 0.7524f - i * yOffset), PortType.None,
                    PinType.Female));
            }

            //B
            for (var i = 0; i < 30; i++)
            {
                var index = i + 1;
                var pinName = $"B{index}";
                Ports.Add(Tuple.Create(pinName, new Vector3(-0.229f, 0.1584f, 0.7524f - i * yOffset), PortType.None,
                    PinType.Female));
            }

            //C
            for (var i = 0; i < 30; i++)
            {
                var index = i + 1;
                var pinName = $"C{index}";
                Ports.Add(Tuple.Create(pinName, new Vector3(-0.1766f, 0.1584f, 0.7524f - i * yOffset),
                    PortType.None, PinType.Female));
            }

            //D
            for (var i = 0; i < 30; i++)
            {
                var index = i + 1;
                var pinName = $"D{index}";
                Ports.Add(Tuple.Create(pinName, new Vector3(-0.1248f, 0.1584f, 0.7524f - i * yOffset),
                    PortType.None, PinType.Female));
            }

            //E
            for (var i = 0; i < 30; i++)
            {
                var index = i + 1;
                var pinName = $"E{index}";
                Ports.Add(Tuple.Create(pinName, new Vector3(-0.0748f, 0.1584f, 0.7524f - i * yOffset),
                    PortType.None, PinType.Female));
            }

            //F
            for (var i = 0; i < 30; i++)
            {
                var index = i + 1;
                var pinName = $"F{index}";
                Ports.Add(Tuple.Create(pinName, new Vector3(0.0739f, 0.1584f, 0.7524f - i * yOffset),
                    PortType.None, PinType.Female));
            }

            //G
            for (var i = 0; i < 30; i++)
            {
                var index = i + 1;
                var pinName = $"G{index}";
                Ports.Add(Tuple.Create(pinName, new Vector3(0.1257f, 0.1584f, 0.7524f - i * yOffset),
                    PortType.None, PinType.Female));
            }

            //H
            for (var i = 0; i < 30; i++)
            {
                var index = i + 1;
                var pinName = $"H{index}";
                Ports.Add(Tuple.Create(pinName, new Vector3(0.1775f, 0.1584f, 0.7524f - i * yOffset),
                    PortType.None, PinType.Female));
            }

            //I
            for (var i = 0; i < 30; i++)
            {
                var index = i + 1;
                var pinName = $"I{index}";
                Ports.Add(Tuple.Create(pinName, new Vector3(0.2293f, 0.1584f, 0.7524f - i * yOffset),
                    PortType.None, PinType.Female));
            }

            //J
            for (var i = 0; i < 30; i++)
            {
                var index = i + 1;
                var pinName = $"J{index}";
                Ports.Add(Tuple.Create(pinName, new Vector3(0.2802f, 0.1584f, 0.7524f - i * yOffset),
                    PortType.None, PinType.Female));
            }

            //VCC E GND
            var vccCount = 0;
            var gndCount = 0;

            //Esquerda
            //VCC
            var startPos = 0.72f;
            for (var i = 0; i < 5; i++)
            {
                for (var j = 0; j < 5; j++)
                {
                    var pinName = $"VCC{vccCount + 1}";
                    Ports.Add(Tuple.Create(pinName, new Vector3(-0.4694f, 0.1584f, startPos - j * yOffset),
                        PortType.None, PinType.Female));
                    vccCount++;
                }

                startPos -= 0.31f;
            }

            //GND
            startPos = 0.72f;
            for (var i = 0; i < 5; i++)
            {
                for (var j = 0; j < 5; j++)
                {
                    var pinName = $"GND{gndCount + 1}";
                    Ports.Add(Tuple.Create(pinName, new Vector3(-0.416f, 0.1584f, startPos - j * yOffset),
                        PortType.None, PinType.Female));
                    gndCount++;
                }

                startPos -= 0.31f;
            }

            //Direita
            //VCC
            startPos = 0.72f;
            for (var i = 0; i < 5; i++)
            {
                for (var j = 0; j < 5; j++)
                {
                    var pinName = $"VCC{vccCount + 1}";
                    Ports.Add(Tuple.Create(pinName, new Vector3(0.41f, 0.1584f, startPos - j * yOffset),
                        PortType.None, PinType.Female));
                    vccCount++;
                }

                startPos -= 0.31f;
            }

            //GND
            startPos = 0.72f;
            for (var i = 0; i < 5; i++)
            {
                for (var j = 0; j < 5; j++)
                {
                    var pinName = $"GND{gndCount + 1}";
                    Ports.Add(Tuple.Create(pinName, new Vector3(0.461f, 0.1584f, startPos - j * yOffset),
                        PortType.None, PinType.Female));
                    gndCount++;
                }

                startPos -= 0.31f;
            }
        }

        public override SaveFile Save()
        {
            var saveFile = new Protoboard400SaveFile
            {
                PrefabName = "Protoboard400",

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
            if (saveFile is Protoboard400SaveFile protoboard400SaveFile)
            {
                transform.position = new Vector3(protoboard400SaveFile.PositionX, protoboard400SaveFile.PositionY,
                    protoboard400SaveFile.PositionZ);
                transform.eulerAngles = new Vector3(protoboard400SaveFile.RotationX, protoboard400SaveFile.RotationY,
                    protoboard400SaveFile.RotationZ);
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

        #endregion
    }
}