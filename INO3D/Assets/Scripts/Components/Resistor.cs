using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Components.Base;
using Assets.Scripts.Managers;
using CircuitSharp.Core;
using UnityEngine;
using ResistorModel = CircuitSharp.Components.Resistor;

namespace Assets.Scripts.Components
{
    public class Resistor : InoComponent
    {
        #region Fields

        private int resistance;
        private int lastResistance;
        private ResistorModel resistor;

        private Material band1Material;
        private Material band2Material;
        private Material band3Material;
        private Material band4Material;

        private Color[] bandColors;
        private Color goldColor;
        private Color silverColor;

        #endregion

        #region Overrides

        public override void GenerateCircuitElement()
        {
            if (IsConnected())
            {
                resistor = SimulationManager.Instance.CreateElement<ResistorModel>(resistance);
                LeadByPortName = new Dictionary<string, Lead>
                {
                    {"A", resistor.LeadIn},
                    {"B", resistor.LeadOut}
                };

                foreach (var pair in ConnectedPorts)
                    SimulationManager.Instance.Connect(GetLead(pair.Key), pair.Value.GetLead());
            }
        }

        public override void OnSimulationTick()
        {
            //Debug.Log(resistor.GetVoltageDelta() + " :: " + resistor.GetCurrent());
        }

        public override void DrawPropertiesWindow()
        {
            UIManager.Instance.BeginPropertyBar();
            UIManager.Instance.GenerateIntPropertyField(LocalizationManager.Instance.Localize("Resistance"), ref resistance);
            UIManager.Instance.EndPropertyBar();
        }

        protected override void OnUpdate()
        {
            if (lastResistance != resistance)
            {
                UpdateResistorColors();
                lastResistance = resistance;
            }
        }

        protected override void SetupPorts()
        {
            resistance = 100;

            bandColors = new[]
            {
                ParseColor("#000000"),
                ParseColor("#644741"),
                ParseColor("#fd0001"),
                ParseColor("#fec100"),
                ParseColor("#ffff01"),
                ParseColor("#00b151"),
                ParseColor("#0070c1"),
                ParseColor("#7030a1"),
                ParseColor("#808080"),
                ParseColor("#ffffff")
            };
            goldColor = ParseColor("#e2b436");
            silverColor = ParseColor("#f0f0f0");

            var meshRenderer = GetComponentInChildren<MeshRenderer>();
            band1Material = meshRenderer.materials[1];
            band2Material = meshRenderer.materials[2];
            band3Material = meshRenderer.materials[3];
            band4Material = meshRenderer.materials[4];

            DefaultHeight = 0.18f;
            Pins.Add(Tuple.Create("A", new Vector3(0,0, 0.0808f)));
            Pins.Add(Tuple.Create("B", new Vector3(0, 0, -0.0779f)));
        }

        public override SaveFile Save()
        {
            var saveFile = new ResistorSaveFile
            {
                PrefabName = "Resistor",

                PositionX = transform.position.x,
                PositionY = transform.position.y,
                PositionZ = transform.position.z,

                RotationX = transform.eulerAngles.x,
                RotationY = transform.eulerAngles.y,
                RotationZ = transform.eulerAngles.z,

                Resistance = resistance
            };

            return saveFile;
        }

        public override void Load(SaveFile saveFile)
        {
            if (saveFile is ResistorSaveFile resistorSaveFile)
            {
                transform.position = new Vector3(resistorSaveFile.PositionX, resistorSaveFile.PositionY,
                    resistorSaveFile.PositionZ);
                transform.eulerAngles = new Vector3(resistorSaveFile.RotationX, resistorSaveFile.RotationY,
                    resistorSaveFile.RotationZ);
                resistance = resistorSaveFile.Resistance;
            }
        }

        public override void Delete()
        {
            DisconnectAllPorts();
            Destroy(gameObject);
        }

        #endregion

        #region Private Methods

        private void UpdateResistorColors()
        {
            var digits = resistance.ToString().ToCharArray().Select(c => int.Parse(c.ToString())).ToArray();

            band1Material.color = digits.Length > 0 ? bandColors[digits[0]] : bandColors[0];
            band2Material.color = digits.Length > 1 ? bandColors[digits[1]] : bandColors[0];
            band3Material.color = digits.Length > 2 ? bandColors[digits[2]] : bandColors[0];

            var exp = (resistance / 10).ToString().ToCharArray().Length - 2;
            if (digits.Length > 3)
                band4Material.color = bandColors[exp > 9 ? 9 : exp];
            else if (digits.Length == 2)
                band4Material.color = goldColor;
            else if (digits.Length == 1)
                band4Material.color = silverColor;
            else
                band4Material.color = bandColors[0];
        }

        private Color ParseColor(string htmlString)
        {
            return ColorUtility.TryParseHtmlString(htmlString, out var color) ? color : Color.black;
        }

        #endregion
    }
}