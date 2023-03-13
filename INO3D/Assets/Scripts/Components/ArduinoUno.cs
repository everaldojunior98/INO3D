using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Components.Base;
using Assets.Scripts.CustomElements.Chips;
using Assets.Scripts.Managers;
using SharpCircuit;
using UnityEngine;
using static SharpCircuit.Circuit;

namespace Assets.Scripts.Components
{
    public class ArduinoUno : InoComponent
    {
        #region Fields

        public string CurrentCode;

        public GameObject LedOnPointLight;
        public GameObject Led13PointLight;
        public GameObject LedRxPointLight;
        public GameObject LedTxPointLight;

        private ATmega328P aTmega328P;
        private Material ledOnMaterial;
        private Material ledOffMaterial;

        private MeshRenderer meshRenderer;

        private bool updateMaterials;
        private bool blinkTxLed;
        
        private bool lastSimulation;
        private bool lastLed13;

        #endregion

        #region Public Methods

        public void WriteSerial(string serial)
        {
            StartCoroutine(BlinkRxLed());
            aTmega328P.WriteToArduino(serial);
        }

        #endregion

        #region Private Methods

        private IEnumerator BlinkRxLed()
        {
            LedRxPointLight.SetActive(true);
            meshRenderer.sharedMaterials = new[]
            {
                meshRenderer.sharedMaterials[0],
                ledOnMaterial,
                meshRenderer.sharedMaterials[2],
                meshRenderer.sharedMaterials[3],
                meshRenderer.sharedMaterials[4]
            };

            yield return new WaitForSeconds(0.1f);

            LedRxPointLight.SetActive(false);
            meshRenderer.sharedMaterials = new[]
            {
                meshRenderer.sharedMaterials[0],
                ledOffMaterial,
                meshRenderer.sharedMaterials[2],
                meshRenderer.sharedMaterials[3],
                meshRenderer.sharedMaterials[4]
            };
        }

        private IEnumerator BlinkTxLed()
        {
            LedTxPointLight.SetActive(true);
            meshRenderer.sharedMaterials = new[]
            {
                meshRenderer.sharedMaterials[0],
                meshRenderer.sharedMaterials[1],
                ledOnMaterial,
                meshRenderer.sharedMaterials[3],
                meshRenderer.sharedMaterials[4]
            };

            yield return new WaitForSeconds(0.1f);

            LedTxPointLight.SetActive(false);
            meshRenderer.sharedMaterials = new[]
            {
                meshRenderer.sharedMaterials[0],
                meshRenderer.sharedMaterials[1],
                ledOffMaterial,
                meshRenderer.sharedMaterials[3],
                meshRenderer.sharedMaterials[4]
            };

            blinkTxLed = false;
        }

        #endregion

        #region Overrides

        public override void GenerateCircuitElement()
        {
            var print = new Action<byte>(b =>
            {
                if (!blinkTxLed)
                    blinkTxLed = true;
                UIManager.Instance.AddLog(((char) b).ToString(), 0);
            });

            aTmega328P = SimulationManager.Instance.CreateElement<ATmega328P>(CurrentCode, print);
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
                {"5V", aTmega328P.VCCLead},
                {"GND", aTmega328P.GNDLead}
            };
        }

        public override void OnSimulationTick()
        {
            if(aTmega328P == null)
                return;

            var led13State = aTmega328P.GetPinVoltage(13) > 2.5f;
            if (lastLed13 != led13State)
            {
                lastLed13 = led13State;
                updateMaterials = true;
            }
        }

        protected override void OnUpdate()
        {
            if (lastSimulation != SimulationManager.Instance.IsSimulating())
            {
                lastSimulation = SimulationManager.Instance.IsSimulating();
                updateMaterials = true;
            }

            if (updateMaterials)
            {
                meshRenderer.sharedMaterials = new[]
                {
                    meshRenderer.sharedMaterials[0],
                    meshRenderer.sharedMaterials[1],
                    meshRenderer.sharedMaterials[2],
                    SimulationManager.Instance.IsSimulating() && lastLed13 ? ledOnMaterial : ledOffMaterial,
                    SimulationManager.Instance.IsSimulating() ? ledOnMaterial : ledOffMaterial
                };

                Led13PointLight.SetActive(SimulationManager.Instance.IsSimulating() && lastLed13);
                LedOnPointLight.SetActive(SimulationManager.Instance.IsSimulating());

                updateMaterials = false;
            }

            if (blinkTxLed)
                StartCoroutine(BlinkTxLed());
        }

        public override void DrawPropertiesWindow()
        {
            UIManager.Instance.GenerateButtonPropertyField(LocalizationManager.Instance.Localize("EditCode"), () =>
            {
                UIManager.Instance.ShowEditCode(CurrentCode, newCode => CurrentCode = newCode);
            });
        }

        protected override void SetupPorts()
        {
            CurrentCode = "void setup()\n{\n\t\n}\n\n\nvoid loop()\n{\n\t\n}";

            ledOnMaterial = Resources.Load<Material>("3D Models\\Uno\\Materials\\LedOn");
            ledOffMaterial = Resources.Load<Material>("3D Models\\Uno\\Materials\\LedOff");

            meshRenderer = GetComponentInChildren<MeshRenderer>();

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
                RotationZ = transform.eulerAngles.z,

                Code = CurrentCode
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

                CurrentCode = arduinoUnoSaveFile.Code;
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