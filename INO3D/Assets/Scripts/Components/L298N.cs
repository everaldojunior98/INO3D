using System;
using System.Collections.Generic;
using Assets.Scripts.Components.Base;
using Assets.Scripts.Managers;
using CircuitSharp.Components.Chips;
using CircuitSharp.Core;
using UnityEngine;

namespace Assets.Scripts.Components
{
    public class L298N : InoComponent
    {
        #region Fields

        private BkBx l298N;

        #endregion

        #region Private Methods

        #endregion

        #region Overrides

        public override void GenerateCircuitElement()
        {
            var currentCode = @"
                void loop()
                {
                    double maxVoltage = 5;

	                double ena = read(0);
	                double in1 = read(1);
	                double in2 = read(2);
	                double in3 = read(3);
	                double in4 = read(4);
	                double enb = read(5);

                    double v12 = read(6);

                    double motorAVoltage = v12 * ena / maxVoltage;
                    if (in1 > maxVoltage / 2 && in2 < maxVoltage / 2)
	                {
                        write(8, motorAVoltage);
                        write(9, 0);
	                }
	                else if (in1 < maxVoltage / 2 && in2 > maxVoltage / 2)
	                {
                        write(8, 0);
                        write(9, motorAVoltage);
	                }
	                else if (in1 < maxVoltage / 2 && in2 < maxVoltage / 2)
	                {
                        write(8, 0);
                        write(9, 0);
	                }
	                else
	                {
                        write(8, 0);
                        write(9, 0);
	                }

                    double motorBVoltage = v12 * enb / maxVoltage;
                    if (in3 > maxVoltage / 2 && in4 < maxVoltage / 2)
	                {
                        write(10, motorBVoltage);
                        write(11, 0);
	                }
	                else if (in3 < maxVoltage / 2 && in4 > maxVoltage / 2)
	                {
                        write(10, 0);
                        write(11, motorBVoltage);
	                }
	                else if (in3 < maxVoltage / 2 && in4 < maxVoltage / 2)
	                {
                        write(10, 0);
                        write(11, 0);
	                }
	                else
	                {
                        write(10, 0);
                        write(11, 0);
	                }
                }
                ";

            l298N = SimulationManager.Instance.CreateElement<BkBx>(8, 4, currentCode);
            LeadByPortName = new Dictionary<string, Lead>
            {
                {"ENA", l298N.Leads[0]},
                {"IN1", l298N.Leads[1]},
                {"IN2", l298N.Leads[2]},
                {"IN3", l298N.Leads[3]},
                {"IN4", l298N.Leads[4]},
                {"ENB", l298N.Leads[5]},
                {"V12", l298N.Leads[6]},
                {"GND", l298N.Leads[7]},
                {"OUT1", l298N.Leads[8]},
                {"OUT2", l298N.Leads[9]},
                {"OUT3", l298N.Leads[10]},
                {"OUT4", l298N.Leads[11]}
            };
        }

        public override void OnSimulationTick()
        {
        }

        protected override void OnUpdate()
        {
        }

        public override void DrawPropertiesWindow()
        {
        }

        protected override void SetupPorts()
        {
            DefaultHeight = 0.006f;

            Ports.Add(Tuple.Create("ENA", new Vector3(0.226f,0.018f,0.035f), PortType.None, PinType.Female));
            Ports.Add(Tuple.Create("IN1", new Vector3(0.226f,0.018f,0.067f), PortType.None, PinType.Female));
            Ports.Add(Tuple.Create("IN2", new Vector3(0.226f,0.018f,0.100f), PortType.None, PinType.Female));
            Ports.Add(Tuple.Create("IN3", new Vector3(0.226f,0.018f,0.133f), PortType.None, PinType.Female));
            Ports.Add(Tuple.Create("IN4", new Vector3(0.226f,0.018f,0.166f), PortType.None, PinType.Female));
            Ports.Add(Tuple.Create("ENB", new Vector3(0.226f,0.018f,0.199f), PortType.None, PinType.Female));

            Ports.Add(Tuple.Create("V12", new Vector3(0.2198f, 0.1058f, -0.1739f), PortType.None, PinType.Female));
            Ports.Add(Tuple.Create("GND", new Vector3(0.2198f, 0.1058f, -0.0989f), PortType.None, PinType.Female));
            Ports.Add(Tuple.Create("OUT1", new Vector3(0.023f, 0.105f, -0.241f), PortType.None, PinType.Female));
            Ports.Add(Tuple.Create("OUT2", new Vector3(0.1f, 0.105f, -0.241f), PortType.None, PinType.Female));
            Ports.Add(Tuple.Create("OUT3", new Vector3(0.103f, 0.105f, 0.256f), PortType.None, PinType.Female));
            Ports.Add(Tuple.Create("OUT4", new Vector3(0.029f, 0.105f, 0.256f), PortType.None, PinType.Female));
        }

        public override SaveFile Save()
        {
            var saveFile = new L298NSaveFile
            {
                PrefabName = "L298N",

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
            if (saveFile is L298NSaveFile l298NSaveFile)
            {
                transform.position = new Vector3(l298NSaveFile.PositionX, l298NSaveFile.PositionY,
                    l298NSaveFile.PositionZ);
                transform.eulerAngles = new Vector3(l298NSaveFile.RotationX, l298NSaveFile.RotationY,
                    l298NSaveFile.RotationZ);
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