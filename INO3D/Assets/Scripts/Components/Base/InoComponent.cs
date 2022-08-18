using System;
using System.Collections.Generic;
using cakeslice;
using UnityEngine;

namespace Assets.Scripts.Components.Base
{
    public abstract class InoComponent : MonoBehaviour
    {
        #region Enums

        public enum PortType
        {
            Analog,
            Digital,
            DigitalPwm,
            Power
        }
        
        public enum PinType
        {
            Male,
            Female
        }

        #endregion

        #region Fields

        protected List<Tuple<string, Vector3, PortType, PinType>> Ports;

        private List<Outline> outlines;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            Ports = new List<Tuple<string, Vector3, PortType, PinType>>();
        }

        private void Start()
        {
            outlines = new List<Outline>();
            foreach (var childRenderer in GetComponentsInChildren<Renderer>())
            {
                var outline = childRenderer.gameObject.AddComponent<Outline>();
                outlines.Add(outline);
                OutlineEffect.Instance?.AddOutline(outline);
            }
            DisableHighlight();

            SetupPorts();
            GeneratePorts();
        }

        #endregion

        #region Abstract Methods

        protected abstract void SetupPorts();

        #endregion

        #region Public Methods

        public void EnableHighlight()
        {
            foreach (var outline in outlines)
                outline.enabled = true;
        }

        public void DisableHighlight()
        {
            foreach (var outline in outlines)
                outline.enabled = false;
        }

        #endregion

        #region Private Methods

        private void GeneratePorts()
        {
            foreach (var tuple in Ports)
            {
                var port = new GameObject(tuple.Item1)
                {
                    transform =
                    {
                        parent = transform,
                        localPosition = tuple.Item2
                    }
                };

                var inoPort = port.AddComponent<InoPort>();
                inoPort.PortName = tuple.Item1;
                inoPort.PortType = tuple.Item3;
                inoPort.PinType = tuple.Item4;
            }
        }

        #endregion
    }
}