using System;
using Assets.Scripts.Components.Base;
using Assets.Scripts.Managers;
using UnityEngine;

namespace Assets.Scripts.Components
{
    public class BlackBox : InoComponent
    {
        #region Fields

        private string currentCode;

        #endregion

        #region Private Methods

        #endregion

        #region Overrides

        public override void GenerateCircuitElement()
        {

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
            currentCode = "";
            DefaultHeight = 0.014f;

            Ports.Add(Tuple.Create("IN1", new Vector3(0.206f, 0.17f, -0.2202f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("IN2", new Vector3(0.157f, 0.17f, -0.2202f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("IN3", new Vector3(0.107f, 0.17f, -0.2202f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("IN4", new Vector3(0.059f, 0.17f, -0.2202f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("IN5", new Vector3(0.009f, 0.17f, -0.2202f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("IN6", new Vector3(-0.038f, 0.17f, -0.2202f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("IN7", new Vector3(-0.088f, 0.17f, -0.2202f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("IN8", new Vector3(-0.138f, 0.17f, -0.2202f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("IN9", new Vector3(-0.187f, 0.17f, -0.2202f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("IN10", new Vector3(-0.238f, 0.17f, -0.2202f), PortType.Analog, PinType.Female));

            Ports.Add(Tuple.Create("OUT1", new Vector3(0.209f, 0.17f, 0.22f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("OUT2", new Vector3(0.160f, 0.17f, 0.22f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("OUT3", new Vector3(0.111f, 0.17f, 0.22f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("OUT4", new Vector3(0.061f, 0.17f, 0.22f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("OUT5", new Vector3(0.011f, 0.17f, 0.22f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("OUT6", new Vector3(-0.036f, 0.17f, 0.22f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("OUT7", new Vector3(-0.085f, 0.17f, 0.22f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("OUT8", new Vector3(-0.136f, 0.17f, 0.22f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("OUT9", new Vector3(-0.186f, 0.17f, 0.22f), PortType.Analog, PinType.Female));
            Ports.Add(Tuple.Create("OUT10", new Vector3(-0.234f, 0.17f, 0.22f), PortType.Analog, PinType.Female));
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