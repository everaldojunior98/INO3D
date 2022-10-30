using System;
using System.Collections.Generic;
using Assets.Scripts.Components.Base;
using Assets.Scripts.Managers;
using CircuitSharp.Core;
using UnityEngine;
using LedModel = CircuitSharp.Components.Led;

namespace Assets.Scripts.Components
{
    public class Led : InoComponent
    {
        #region Fields

        private const float MinLuminosity = 0.2f;
        private const float MaxLuminosity = 2f;

        public Light PointLight;

        private LedModel led;

        private MeshRenderer meshRenderer;

        private float voltage;
        private float current;

        private List<Material> materials;
        private List<Color> colors;
        private int currentColor;
        private int lastColor;

        private bool isOn;

        #endregion

        #region Overrides

        protected override void OnUpdate()
        {
            if (SimulationManager.Instance.IsSimulating())
            {
                if (voltage > 1)
                {
                    PointLight.gameObject.SetActive(true);
                    PointLight.color = colors[currentColor];

                    meshRenderer.sharedMaterial.SetColor("_EmissionColor",
                        Color.white * Map(current, 0, 0.02f, MinLuminosity, MaxLuminosity));

                    isOn = true;
                }
                else
                {
                    PointLight.gameObject.SetActive(false);
                    meshRenderer.sharedMaterial.SetColor("_EmissionColor", Color.white * MinLuminosity);
                }
            }

            if (!SimulationManager.Instance.IsSimulating() && isOn)
            {
                PointLight.gameObject.SetActive(false);
                meshRenderer.sharedMaterial.SetColor("_EmissionColor", Color.white * MinLuminosity);
                isOn = false;
            }

            if (lastColor != currentColor)
            {
                meshRenderer.sharedMaterial = materials[currentColor];
                lastColor = currentColor;
            }
        }

        public override void GenerateCircuitElement()
        {
            if (IsConnected())
            {
                led = SimulationManager.Instance.CreateElement<LedModel>();
                LeadByPortName = new Dictionary<string, Lead>
                {
                    {"A", led.LeadOut},
                    {"B", led.LeadIn}
                };

                foreach (var pair in ConnectedPorts)
                    SimulationManager.Instance.Connect(GetLead(pair.Key), pair.Value.GetLead());
            }
        }

        public override void OnSimulationTick()
        {
            voltage = (float) Math.Abs(led.GetVoltageDelta());
            current = (float) Math.Abs(led.GetCurrent());
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