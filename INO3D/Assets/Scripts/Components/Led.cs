using System;
using System.Collections.Generic;
using Assets.Scripts.Components.Base;
using Assets.Scripts.Managers;
using SharpCircuit;
using UnityEngine;

namespace Assets.Scripts.Components
{
    public class Led : InoComponent
    {
        #region Properties

        public Light PointLight;

        #endregion

        #region Fields

        private const float MinLuminosity = 0.2f;
        private const float MaxLuminosity = 2f;

        private const float MaxCurrentToWarning = 0.5f;

        private const float MinCurrent = 0f;
        private const float MaxCurrent = 0.025f;

        private const float MinIntensity = 0f;
        private const float MaxIntensity = 1f;

        private readonly Vector3 warningPosition = new Vector3(0, 0.12f, 0);

        private LED led;

        private MeshRenderer meshRenderer;

        private float current;

        private List<Material> materials;
        private List<Color> colors;
        private int currentColor;
        private int lastColor = -1;

        private bool isOn;

        #endregion

        #region Overrides

        protected override void OnUpdate()
        {
            if (SimulationManager.Instance.IsSimulating())
            {
                if (current > MinCurrent)
                {
                    var clampedCurrent = Math.Min(current, MaxCurrent);
                    PointLight.gameObject.SetActive(true);
                    PointLight.intensity = Map(clampedCurrent, MinCurrent, MaxCurrent, MinIntensity, MaxIntensity);
                    PointLight.color = colors[currentColor];

                    meshRenderer.sharedMaterial.SetColor("_EmissionColor",
                        Color.white * Map(clampedCurrent, MinCurrent, MaxCurrent, MinLuminosity, MaxLuminosity));

                    isOn = true;
                }
                else
                {
                    PointLight.gameObject.SetActive(false);
                    meshRenderer.sharedMaterial.SetColor("_EmissionColor", Color.white * MinLuminosity);
                }

                if (current > MaxCurrentToWarning)
                    UIManager.Instance.ShowWarning(gameObject, warningPosition);
                else
                    UIManager.Instance.HideWarning(gameObject);
            }

            if (!SimulationManager.Instance.IsSimulating() && isOn)
            {
                PointLight.gameObject.SetActive(false);
                meshRenderer.sharedMaterial.SetColor("_EmissionColor", Color.white * MinLuminosity);
                isOn = false;
            }

            if (lastColor != currentColor)
            {
                meshRenderer.sharedMaterial = new Material(materials[currentColor]);
                lastColor = currentColor;
            }
        }

        public override void GenerateCircuitElement()
        {
            if (IsConnected())
            {
                led = SimulationManager.Instance.CreateElement<LED>();
                LeadByPortName = new Dictionary<string, Circuit.Lead>
                {
                    {"A", led.leadOut},
                    {"B", led.leadIn}
                };

                foreach (var pair in ConnectedPorts)
                    SimulationManager.Instance.Connect(GetLead(pair.Key), pair.Value.GetLead());
            }
        }

        public override void OnSimulationTick()
        {
            if(led == null)
                return;

            current = (float) led.getCurrent();
        }

        public override void DrawPropertiesWindow()
        {
            var colorNames = new[]
            {
                LocalizationManager.Instance.Localize("ColorRed"),
                LocalizationManager.Instance.Localize("ColorGreen"),
                LocalizationManager.Instance.Localize("ColorBlue"),
                LocalizationManager.Instance.Localize("ColorWhite"),
                LocalizationManager.Instance.Localize("ColorYellow")

            };
            UIManager.Instance.GenerateComboBoxPropertyField(LocalizationManager.Instance.Localize("Color"),
                ref currentColor, colorNames);
        }

        protected override void SetupPorts()
        {
            materials = new List<Material>
            {
                Resources.Load<Material>("3D Models\\Led\\Materials\\RedLed"),
                Resources.Load<Material>("3D Models\\Led\\Materials\\GreenLed"),
                Resources.Load<Material>("3D Models\\Led\\Materials\\BlueLed"),
                Resources.Load<Material>("3D Models\\Led\\Materials\\WhiteLed"),
                Resources.Load<Material>("3D Models\\Led\\Materials\\YellowLed")
            };

            foreach (var material in materials)
                material.SetColor("_EmissionColor", Color.white * MinLuminosity);

            colors = new List<Color>
            {
                Color.red,
                Color.green,
                Color.blue,
                Color.white,
                Color.yellow
            };

            meshRenderer = GetComponentInChildren<MeshRenderer>();
            
            DefaultHeight = 0.18f;
            Pins.Add(Tuple.Create("A", new Vector3(0, 0, 0.0196f)));
            Pins.Add(Tuple.Create("B", new Vector3(0, 0, -0.0192f)));
        }

        public override SaveFile Save()
        {
            var saveFile = new LedSaveFile
            {
                PrefabName = "Led",

                PositionX = transform.position.x,
                PositionY = transform.position.y,
                PositionZ = transform.position.z,

                RotationX = transform.eulerAngles.x,
                RotationY = transform.eulerAngles.y,
                RotationZ = transform.eulerAngles.z,

                CurrentColor = currentColor
            };

            return saveFile;
        }

        public override void Load(SaveFile saveFile)
        {
            if (saveFile is LedSaveFile ledSaveFileSaveFile)
            {
                transform.position = new Vector3(ledSaveFileSaveFile.PositionX, ledSaveFileSaveFile.PositionY,
                    ledSaveFileSaveFile.PositionZ);
                transform.eulerAngles = new Vector3(ledSaveFileSaveFile.RotationX, ledSaveFileSaveFile.RotationY,
                    ledSaveFileSaveFile.RotationZ);
                currentColor = ledSaveFileSaveFile.CurrentColor;
            }
        }

        public override void Delete()
        {
            DisconnectAllPorts();
            Destroy(gameObject);
        }

        #endregion

        #region Private Methods

        private float Map(float x, float inMin, float inMax, float outMin, float outMax)
        {
            return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }

        #endregion
    }
}