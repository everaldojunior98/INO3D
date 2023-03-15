using System.Collections.Generic;
using Assets.Scripts.Components.Base;
using Assets.Scripts.Managers;
using UnityEngine;
using static SharpCircuit.Circuit;
using DCMotorModel = SharpCircuit.elements.DCMotor;

namespace Assets.Scripts.Components
{
    public class RcCarChassis : InoComponent
    {
        #region Properties

        public GameObject LeftWheelGameObject;
        public GameObject RightWheelGameObject;
        public GameObject BackWheelGameObject;

        #endregion

        #region Fields

        private readonly float reductionRatio = 10;

        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private DCMotorModel leftMotor;
        private DCMotorModel rightMotor;
        private float leftSpeed;
        private float rightSpeed;

        private bool isRunning;

        #endregion

        #region Overrides

        public override void GenerateCircuitElement()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;

            leftMotor = SimulationManager.Instance.CreateElement<DCMotorModel>(0.150, 5, 300, 0.02, 0.02, 0.005);
            rightMotor = SimulationManager.Instance.CreateElement<DCMotorModel>(0.150, 5, 300, 0.02, 0.02, 0.005);
            LeadByPortName = new Dictionary<string, Lead>
            {
                {"A", leftMotor.leadIn},
                {"B", leftMotor.leadOut},
                {"C", rightMotor.leadIn},
                {"D", rightMotor.leadOut}
            };
        }

        public override void OnSimulationTick()
        {
            if (leftMotor == null || rightMotor == null)
                return;

            leftSpeed = (float) leftMotor.speed / reductionRatio;
            rightSpeed = (float) rightMotor.speed / reductionRatio;
        }

        public override void DrawPropertiesWindow()
        {
        }

        protected override void OnUpdate()
        {
            if (SimulationManager.Instance.IsSimulating())
            {
                isRunning = true;
                var movementSpeed = (leftSpeed + rightSpeed) / 2f / 22f;
                var turnSpeed = (rightSpeed - leftSpeed) / 1.5f;

                LeftWheelGameObject.transform.Rotate(new Vector3(0, 0, -leftSpeed * 6f) * Time.deltaTime);
                RightWheelGameObject.transform.Rotate(new Vector3(0, 0, rightSpeed * 6f) * Time.deltaTime);
                BackWheelGameObject.transform.Rotate(new Vector3(0, 0, -((leftSpeed + rightSpeed) / 2f) * 6f) * Time.deltaTime);

                transform.Rotate(new Vector3(0, -turnSpeed, 0) * Time.deltaTime);
                transform.Translate(movementSpeed * Vector3.forward * Time.deltaTime);
            }

            if (isRunning && !SimulationManager.Instance.IsSimulating())
            {
                isRunning = false;
                transform.position = initialPosition;
                transform.rotation = initialRotation;
            }
        }

        protected override void SetupPorts()
        {
            DefaultHeight = 0.72f;

            Ports.Add(new Port("A", new Vector3(-0.385f, -0.1862f, -1.0873f), PortType.None, PinType.SolderingPoint, false, true, new Vector3(0, 0, -1), new Vector3(0, 0.01f, 0), new Vector3(-90, 0, 0)));
            Ports.Add(new Port("B", new Vector3(-0.385f, -0.4253f, -1.088f), PortType.None, PinType.SolderingPoint, false, true, new Vector3(0, 0, -1), new Vector3(0, 0.01f, 0), new Vector3(-90, 0, 0)));

            Ports.Add(new Port("C", new Vector3(0.386f, -0.1883f, -1.081f), PortType.None, PinType.SolderingPoint, false, true, new Vector3(0, 0, -1), new Vector3(0, 0.01f, 0), new Vector3(-90, 0, 180)));
            Ports.Add(new Port("D", new Vector3(0.386f, -0.426f, -1.0823f), PortType.None, PinType.SolderingPoint, false, true, new Vector3(0, 0, -1), new Vector3(0, 0.01f, 0), new Vector3(-90, 0, 180)));
        }

        public override SaveFile Save()
        {
            var saveFile = new RcCarChassisSaveFile
            {
                PrefabName = "CarChassis",

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
            if (saveFile is RcCarChassisSaveFile carChassisSaveFile)
            {
                transform.position = new Vector3(carChassisSaveFile.PositionX, carChassisSaveFile.PositionY, carChassisSaveFile.PositionZ);
                transform.eulerAngles = new Vector3(carChassisSaveFile.RotationX, carChassisSaveFile.RotationY, carChassisSaveFile.RotationZ);
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