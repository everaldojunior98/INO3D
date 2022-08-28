using System;
using Assets.Scripts.Components.Base;
using UnityEngine;

namespace Assets.Scripts.Components
{
    public class Resistor : InoComponent
    {
        #region Overrides

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