using System.Collections.Generic;
using Assets.Scripts.Components.Base;
using Assets.Scripts.Managers;
using SharpCircuit;
using UnityEngine;
using static SharpCircuit.Circuit;
using static SharpCircuit.Voltage;

namespace Assets.Scripts.Components
{
    public class Battery : InoComponent
    {
        #region Fields

        private VoltageInput voltageInput;
        private Ground ground;
        private float voltage = 9;

        #endregion

        #region Private Methods

        #endregion

        #region Overrides

        public override void GenerateCircuitElement()
        {
            voltageInput = SimulationManager.Instance.CreateElement<VoltageInput>(WaveType.DC);
            voltageInput.maxVoltage = voltage;

            ground = SimulationManager.Instance.CreateElement<Ground>();

            LeadByPortName = new Dictionary<string, Lead>
            {
                {"+", voltageInput.leadPos},
                {"-", ground.leadIn}
            };
        }

        public override void OnSimulationTick()
        {
        }

        protected override void OnUpdate()
        {
        }

        public override void DrawPropertiesWindow()
        {
        }

        protected override void SetupPorts()
        {
            DefaultHeight = 0.129f;

            Ports.Add(new Port("+", new Vector3(-0.4623f, 0.022f, -0.2582f), PortType.None, PinType.Female, false, true, new Vector3(0, 0, -1), Vector3.zero, Vector3.zero));
            Ports.Add(new Port("-", new Vector3(-0.4623f, -0.0263f, -0.2582f), PortType.None, PinType.Female, false, true, new Vector3(0, 0, -1), Vector3.zero, Vector3.zero));
        }

        public override SaveFile Save()
        {
            var saveFile = new BatterySaveFile
            {
                PrefabName = "Battery",
                Voltage = voltage
            };

            return saveFile;
        }

        public override void Load(SaveFile saveFile)
        {
            if (saveFile is BatterySaveFile batterySaveFile)
                voltage = batterySaveFile.Voltage;
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