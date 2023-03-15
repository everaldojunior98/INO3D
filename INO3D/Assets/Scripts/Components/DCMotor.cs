using System;
using System.Collections.Generic;
using Assets.Scripts.Components.Base;
using Assets.Scripts.Managers;
using UnityEngine;
using static SharpCircuit.Circuit;
using DCMotorModel = SharpCircuit.elements.DCMotor;

namespace Assets.Scripts.Components
{
    public class DCMotor : InoComponent
    {
        #region Properties

        public GameObject AxisGameObject;

        #endregion

        #region Fields

        private DCMotorModel dcMotor;
        private float speed;

        #endregion

        #region Overrides

        public override void GenerateCircuitElement()
        {
            dcMotor = SimulationManager.Instance.CreateElement<DCMotorModel>(0.150, 5, 300, 0.02, 0.02, 0.005);
            LeadByPortName = new Dictionary<string, Lead>
            {
                {"A", dcMotor.leadIn},
                {"B", dcMotor.leadOut}
            };
        }

        public override void OnSimulationTick()
        {
            if (dcMotor == null)
                return;

            speed = (float) dcMotor.speed;
        }

        public override void DrawPropertiesWindow()
        {
        }

        protected override void OnUpdate()
        {
            if (SimulationManager.Instance.IsSimulating())
            {
                var rps = speed * 6f;
                AxisGameObject.transform.Rotate(new Vector3(0, 0, rps) * Time.deltaTime);
            }
        }

        protected override void SetupPorts()
        {
            DefaultHeight = 0.15f;

            Ports.Add(new Port("A", new Vector3(-0.1109f, 0.168f, -0.216f), PortType.None, PinType.SolderingPoint, false, true, new Vector3(0, 0, -1), new Vector3(0, 0.01f, 0)));
            Ports.Add(new Port("B", new Vector3(0.1031f, 0.168f, -0.216f), PortType.None, PinType.SolderingPoint, false, true, new Vector3(0, 0, -1), new Vector3(0, 0.01f, 0)));
        }

        public override SaveFile Save()
        {
            var saveFile = new DCMotorSaveFile
            {
                PrefabName = "DCMotor",

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
            if (saveFile is DCMotorSaveFile dcMotorSaveFile)
            {
                transform.position = new Vector3(dcMotorSaveFile.PositionX, dcMotorSaveFile.PositionY, dcMotorSaveFile.PositionZ);
                transform.eulerAngles = new Vector3(dcMotorSaveFile.RotationX, dcMotorSaveFile.RotationY, dcMotorSaveFile.RotationZ);
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