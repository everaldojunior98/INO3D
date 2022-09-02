using System;
using System.Collections.Generic;
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

        private ResistorModel resistor;

        #endregion

        #region Overrides

        public override void GenerateCircuitElement()
        {
            if (IsConnected())
            {
                resistor = SimulationManager.Instance.CreateElement<ResistorModel>(100);
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

        protected override void SetupPorts()
        {
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
                RotationZ = transform.eulerAngles.z
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
            }
        }

        public override void Delete()
        {
            DisconnectAllPorts();
            Destroy(gameObject);
        }

        #endregion
    }
}