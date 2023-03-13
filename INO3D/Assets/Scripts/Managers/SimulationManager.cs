using System;
using System.Collections.Generic;
using System.Threading;
using Assets.Scripts.Components.Base;
using Assets.Scripts.Utils;
using SharpCircuit;
using UnityEngine;
using static SharpCircuit.Circuit;
using Debug = UnityEngine.Debug;
using Exception = System.Exception;

namespace Assets.Scripts.Managers
{
    public class SimulationManager : MonoBehaviour
    {
        #region Properties

        public static SimulationManager Instance { get; private set; }

        #endregion

        #region Fields

        private Circuit circuit;
        private volatile bool isSimulating;
        private volatile bool needAnalysis;

        private Thread simulationThread;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;
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

        public void NeedAnalysis()
        {
            needAnalysis = true;
        }

        public void StartSimulation()
        {
            ComponentsManager.Instance.DeselectPorts();
            ComponentsManager.Instance.DeselectComponent();

            circuit = new Circuit();

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

            
            if (simulationThread is {IsAlive: true})
            {
                simulationThread.Abort();
                simulationThread.Join();
            }

            isSimulating = true;
            simulationThread = new Thread(() =>
            {
                while (isSimulating)
                {
                    if (needAnalysis)
                    {
                        circuit.needAnalyze();
                        needAnalysis = false;
                    }
                    circuit.doTick();

                    try
                    {
                        foreach (var component in componentByHash)
                            component.Value.OnSimulationTick();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            });
            simulationThread.Start();
        }

        public void StopSimulation()
        {
            isSimulating = false;
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
            return circuit?.time ?? 0;
        }

        #endregion
    }
}