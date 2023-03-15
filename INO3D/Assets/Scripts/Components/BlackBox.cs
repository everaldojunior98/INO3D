using System;
using System.Collections.Generic;
using Assets.Scripts.Components.Base;
using Assets.Scripts.CustomElements.Chips;
using Assets.Scripts.Managers;
using UnityEngine;
using static SharpCircuit.Circuit;

namespace Assets.Scripts.Components
{
    public class BlackBox : InoComponent
    {
        #region Fields

        private string header;
        private string currentCode;
        private BkBx bkBx;

        #endregion

        #region Private Methods

        #endregion

        #region Overrides

        public override void GenerateCircuitElement()
        {
            header = @"
                #define IN1 0
                #define IN2 1
                #define IN3 2
                #define IN4 3
                #define IN5 4
                #define IN6 5
                #define IN7 6
                #define IN8 7
                #define IN9 8
                #define IN10 9
                #define OUT1 10
                #define OUT2 11
                #define OUT3 12
                #define OUT4 13
                #define OUT5 14
                #define OUT6 15
                #define OUT7 16
                #define OUT8 17
                #define OUT9 18
                #define OUT10 19
                ";

            bkBx = SimulationManager.Instance.CreateElement<BkBx>(10, 10, header + currentCode);
            LeadByPortName = new Dictionary<string, Lead>
            {
                {"IN1", bkBx.Leads[0]},
                {"IN2", bkBx.Leads[1]},
                {"IN3", bkBx.Leads[2]},
                {"IN4", bkBx.Leads[3]},
                {"IN5", bkBx.Leads[4]},
                {"IN6", bkBx.Leads[5]},
                {"IN7", bkBx.Leads[6]},
                {"IN8", bkBx.Leads[7]},
                {"IN9", bkBx.Leads[8]},
                {"IN10", bkBx.Leads[9]},
                {"OUT1", bkBx.Leads[10]},
                {"OUT2", bkBx.Leads[11]},
                {"OUT3", bkBx.Leads[12]},
                {"OUT4", bkBx.Leads[13]},
                {"OUT5", bkBx.Leads[14]},
                {"OUT6", bkBx.Leads[15]},
                {"OUT7", bkBx.Leads[16]},
                {"OUT8", bkBx.Leads[17]},
                {"OUT9", bkBx.Leads[18]},
                {"OUT10", bkBx.Leads[19]}
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
            UIManager.Instance.GenerateButtonPropertyField(LocalizationManager.Instance.Localize("EditCode"), () =>
            {
                UIManager.Instance.ShowEditCode(currentCode, newCode => currentCode = newCode);
            });
        }

        protected override void SetupPorts()
        {
            currentCode = "void loop()\n{\n\t\n}";
            DefaultHeight = 0.014f;

            Ports.Add(new Port("IN1", new Vector3(0.206f, 0.17f, -0.2202f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("IN2", new Vector3(0.157f, 0.17f, -0.2202f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("IN3", new Vector3(0.107f, 0.17f, -0.2202f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("IN4", new Vector3(0.059f, 0.17f, -0.2202f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("IN5", new Vector3(0.009f, 0.17f, -0.2202f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("IN6", new Vector3(-0.038f, 0.17f, -0.2202f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("IN7", new Vector3(-0.088f, 0.17f, -0.2202f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("IN8", new Vector3(-0.138f, 0.17f, -0.2202f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("IN9", new Vector3(-0.187f, 0.17f, -0.2202f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("IN10", new Vector3(-0.238f, 0.17f, -0.2202f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));

            Ports.Add(new Port("OUT1", new Vector3(0.209f, 0.17f, 0.22f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("OUT2", new Vector3(0.160f, 0.17f, 0.22f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("OUT3", new Vector3(0.111f, 0.17f, 0.22f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("OUT4", new Vector3(0.061f, 0.17f, 0.22f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("OUT5", new Vector3(0.011f, 0.17f, 0.22f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("OUT6", new Vector3(-0.036f, 0.17f, 0.22f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("OUT7", new Vector3(-0.085f, 0.17f, 0.22f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("OUT8", new Vector3(-0.136f, 0.17f, 0.22f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("OUT9", new Vector3(-0.186f, 0.17f, 0.22f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
            Ports.Add(new Port("OUT10", new Vector3(-0.234f, 0.17f, 0.22f), PortType.None, PinType.Female, false, false, Vector3.zero, Vector3.zero, Vector3.zero));
        }

        public override SaveFile Save()
        {
            var saveFile = new BlackBoxSaveFile
            {
                PrefabName = "BlackBox",

                PositionX = transform.position.x,
                PositionY = transform.position.y,
                PositionZ = transform.position.z,

                RotationX = transform.eulerAngles.x,
                RotationY = transform.eulerAngles.y,
                RotationZ = transform.eulerAngles.z,

                Code = currentCode
            };

            return saveFile;
        }

        public override void Load(SaveFile saveFile)
        {
            if (saveFile is BlackBoxSaveFile blackBoxSaveFile)
            {
                transform.position = new Vector3(blackBoxSaveFile.PositionX, blackBoxSaveFile.PositionY,
                    blackBoxSaveFile.PositionZ);
                transform.eulerAngles = new Vector3(blackBoxSaveFile.RotationX, blackBoxSaveFile.RotationY,
                    blackBoxSaveFile.RotationZ);

                currentCode = blackBoxSaveFile.Code;
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