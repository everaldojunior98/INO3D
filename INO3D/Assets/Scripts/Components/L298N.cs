using System;
using System.Collections.Generic;
using Assets.Scripts.Components.Base;
using Assets.Scripts.CustomElements.Chips;
using Assets.Scripts.Managers;
using UnityEngine;
using static SharpCircuit.Circuit;

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

            Ports.Add(new Port("ENA", new Vector3(0.226f,0.018f,0.035f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("IN1", new Vector3(0.226f,0.018f,0.067f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("IN2", new Vector3(0.226f,0.018f,0.100f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("IN3", new Vector3(0.226f,0.018f,0.133f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("IN4", new Vector3(0.226f,0.018f,0.166f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("ENB", new Vector3(0.226f,0.018f,0.199f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));

            Ports.Add(new Port("V12", new Vector3(0.221f, 0.1058f, -0.172f), PortType.None, PinType.Female, false, true, new Vector3(1,0,0), new Vector3(0, -0.09f, 0), Vector3.zero));
            Ports.Add(new Port("GND", new Vector3(0.2211f, 0.1058f, -0.0971f), PortType.None, PinType.Female, false, true, new Vector3(1, 0, 0), new Vector3(0, -0.09f, 0), Vector3.zero));
            Ports.Add(new Port("OUT1", new Vector3(0.0261f, 0.105f, -0.2392f), PortType.None, PinType.Female, false, true, new Vector3(0, 0, -1), new Vector3(0, -0.09f, 0), Vector3.zero));
            Ports.Add(new Port("OUT2", new Vector3(0.1006f, 0.105f, -0.239f), PortType.None, PinType.Female, false, true, new Vector3(0, 0, -1), new Vector3(0, -0.09f, 0), Vector3.zero));
            Ports.Add(new Port("OUT3", new Vector3(0.1048f, 0.105f, 0.2572f), PortType.None, PinType.Female, false, true, new Vector3(0, 0, 1), new Vector3(0, -0.09f, 0), Vector3.zero));
            Ports.Add(new Port("OUT4", new Vector3(0.0296f, 0.105f, 0.2572f), PortType.None, PinType.Female, false, true, new Vector3(0, 0, 1), new Vector3(0, -0.09f, 0), Vector3.zero));
        }

        public override SaveFile Save()
        {
            var saveFile = new L298NSaveFile
            {
                PrefabName = "L298N"
            };

            return saveFile;
        }

        public override void Load(SaveFile saveFile)
        {
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