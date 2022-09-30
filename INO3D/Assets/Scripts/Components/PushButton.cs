using System;
using Assets.Scripts.Components.Base;
using UnityEngine;

namespace Assets.Scripts.Components
{
    public class PushButton : InoComponent
    {
        #region Fields

        #endregion

        #region Overrides

        public override void GenerateCircuitElement()
        {

        }

        public override void OnSimulationTick()
        {

        }

        public override void DrawPropertiesWindow()
        {

        }

        protected override void SetupPorts()
        {
            DefaultHeight = 0.18f;
            Pins.Add(Tuple.Create("A", new Vector3(0, 0, 0.0196f)));
            Pins.Add(Tuple.Create("B", new Vector3(0, 0, -0.0192f)));
        }

        public override SaveFile Save()
        {
            var saveFile = new PushButtonSaveFile
            {
                PrefabName = "Led",

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
            if (saveFile is PushButtonSaveFile pushButtonSaveFile)
            {
                transform.position = new Vector3(pushButtonSaveFile.PositionX, pushButtonSaveFile.PositionY,
                    pushButtonSaveFile.PositionZ);
                transform.eulerAngles = new Vector3(pushButtonSaveFile.RotationX, pushButtonSaveFile.RotationY,
                    pushButtonSaveFile.RotationZ);
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