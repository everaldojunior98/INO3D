using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Components.Base;
using Assets.Scripts.Managers;
using SharpCircuit;
using UnityEngine;
using static SharpCircuit.Circuit;

namespace Assets.Scripts.Components
{
    public class Protoboard400 : InoComponent
    {
        #region Overrides

        public override void GenerateCircuitElement()
        {
            LeadByPortName = new Dictionary<string, Lead>();

            var portsByGroup = GeneratedPorts.GroupBy(port =>
            {
                if (port.PortName.Contains("VCC"))
                    return "VCC";
                if (port.PortName.Contains("GND"))
                    return "GND";
                return port.PortName.Substring(1);
            });

            var ports = new List<List<InoPort>>();
            foreach (var group in portsByGroup)
            {
                var currentGroup = new List<InoPort>();
                foreach (var port in group)
                {
                    currentGroup.Add(port);
                    var isVcc = group.Key.Contains("VCC");
                    var isGnd = group.Key.Contains("GND");
                    if ((currentGroup.Count == 25 && (isVcc || isGnd)) || (currentGroup.Count == 5 && !isVcc && !isGnd))
                    {
                        ports.Add(currentGroup);
                        currentGroup = new List<InoPort>();
                    }
                }
            }

            foreach (var group in ports.Where(p => p.Any(port => port.IsConnected())))
            {
                var output = SimulationManager.Instance.CreateElement<Output>();
                foreach (var inoPort in group)
                    LeadByPortName.Add(inoPort.PortName, output.leadIn);
            }
        }

        public override void OnSimulationTick()
        {
        }

        public override void DrawPropertiesWindow()
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