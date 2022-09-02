using System;
using System.Collections.Generic;
using Assets.Scripts.Components.Base;
using Assets.Scripts.Utils;
using CircuitSharp.Core;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class SimulationManager : MonoBehaviour
    {
        #region Properties

        public static SimulationManager Instance { get; private set; }

        #endregion

        #region Fields

        private Circuit circuit;
        private bool isSimulating;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isSimulating = !isSimulating;
            }
        }

        private void OnApplicationQuit()
        {
            StopSimulation();
        }

        #endregion

        #region Public Methods

        public bool IsSimulating()
        {
            return isSimulating;
        }

        public void StartSimulation()
        {
            ComponentsManager.Instance.DeselectPorts();
            ComponentsManager.Instance.DeselectComponent();

            circuit = new Circuit(error =>
            {
                isSimulating = false;
                Debug.LogError(error.Code);
            });

            var components = new HashSet<string>();
            var dependencyByComponent = new HashSet<Tuple<string, string>>();
            var componentByHash = new Dictionary<string, InoComponent>();

            foreach (var inoComponent in FindObjectsOfType<InoComponent>())
            {
                components.Add(inoComponent.Hash);
                componentByHash.Add(inoComponent.Hash, inoComponent);

                foreach (var dependency in inoComponent.GetDependencies())
                    dependencyByComponent.Add(Tuple.Create(inoComponent.Hash, dependency));
            }

            foreach (var hash in DependencySorter.Sort(components, dependencyByComponent))
                componentByHash[hash].GenerateCircuitElement();

            circuit.StartSimulation(() =>
            {
                try
                {
                    foreach (var component in componentByHash)
                        component.Value.OnSimulationTick();
                }
                catch
                {
                    // ignored
                }
            });

            isSimulating = true;
        }

        public void StopSimulation()
        {
            isSimulating = false;
            circuit?.StopSimulation();
        }

        public T CreateElement<T>(params object[] args) where T : class, ICircuitElement
        {
            return circuit.Create<T>(args);
        }

        public void Connect(Lead left, Lead right)
        {
            circuit.Connect(left, right);
        }

        public double GetTime()
        {
            return circuit?.GetTime() ?? 0;
        }

        #endregion
    }
}
