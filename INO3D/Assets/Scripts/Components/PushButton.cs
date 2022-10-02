using System;
using Assets.Scripts.Components.Base;
using Assets.Scripts.Managers;
using UnityEngine;

namespace Assets.Scripts.Components
{
    public class PushButton : InoComponent
    {
        #region Fields

        private Animator animator;

        #endregion

        #region Unity Methods

        private void OnMouseDown()
        {
            if (SimulationManager.Instance.IsSimulating())
            {
                animator.SetBool("Down", true);
            }
        }

        private void OnMouseUp()
        {
            animator.SetBool("Down", false);
        }

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
            DebugPins = true;
            DefaultHeight = 0.18f;
            Pins.Add(Tuple.Create("A", new Vector3(-0.0737f, 0.0196f, 0.0516f)));
            Pins.Add(Tuple.Create("B", new Vector3(0.073f, 0.0196f, 0.0516f)));
            Pins.Add(Tuple.Create("C", new Vector3(-0.0737f, 0.0196f, -0.0516f)));
            Pins.Add(Tuple.Create("D", new Vector3(0.073f, 0.0196f, -0.0516f)));

            animator = GetComponent<Animator>();
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