using System;
using System.Collections.Generic;
using Assets.Scripts.Components.Base;
using Assets.Scripts.Managers;
using CircuitSharp.Components;
using CircuitSharp.Core;
using UnityEngine;

namespace Assets.Scripts.Components
{
    public class PushButton : InoComponent
    {
        #region Fields

        private SwitchSPST switchSpst;

        private Animator animator;

        #endregion

        #region Unity Methods

        private void OnMouseDown()
        {
            if (SimulationManager.Instance.IsSimulating())
            {
                animator.SetBool("Down", true);
                switchSpst.Close();
            }
        }

        private void OnMouseUp()
        {
            animator.SetBool("Down", false);
            if (SimulationManager.Instance.IsSimulating())
                switchSpst.Open();
        }

        #endregion

        #region Overrides

        public override void GenerateCircuitElement()
        {
            if (IsConnected())
            {
                switchSpst = SimulationManager.Instance.CreateElement<SwitchSPST>();
                switchSpst.Open();

                LeadByPortName = new Dictionary<string, Lead>
                {
                    {"A", switchSpst.LeadA},
                    {"C", switchSpst.LeadA},
                    {"B", switchSpst.LeadB},
                    {"D", switchSpst.LeadB}
                };

                foreach (var pair in ConnectedPorts)
                    SimulationManager.Instance.Connect(GetLead(pair.Key), pair.Value.GetLead());
            }
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
            Pins.Add(Tuple.Create("A", new Vector3(-0.0737f, 0.0196f, 0.0516f)));
            Pins.Add(Tuple.Create("C", new Vector3(0.073f, 0.0196f, 0.0516f)));
            Pins.Add(Tuple.Create("B", new Vector3(-0.0737f, 0.0196f, -0.0516f)));
            Pins.Add(Tuple.Create("D", new Vector3(0.073f, 0.0196f, -0.0516f)));

            animator = GetComponent<Animator>();
        }

        public override SaveFile Save()
        {
            var saveFile = new PushButtonSaveFile
            {
                PrefabName = "PushButton",

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