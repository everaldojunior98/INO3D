using System;
using System.Collections.Generic;
using System.Linq;
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
            Power,
            None
        }
        
        public enum PinType
        {
            Male,
            Female
        }

        #endregion

        #region Fields

        protected List<Tuple<string, Vector3, PortType, PinType>> Ports;
        protected List<InoPort> GeneratedPorts;

        protected List<Tuple<string, Vector3>> Pins;

        public bool CanDrag { get; protected set; }
        public bool CanRotate { get; protected set; }

        private List<Outline> outlines;

        private bool isConnected;

        private List<InoPort> connectedPorts;
        private List<Vector3> connectedPortsPositions;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            Ports = new List<Tuple<string, Vector3, PortType, PinType>>();
            GeneratedPorts = new List<InoPort>();

            Pins = new List<Tuple<string, Vector3>>();

            connectedPorts = new List<InoPort>();
            connectedPortsPositions = new List<Vector3>();

            CanDrag = true;
            CanRotate = true;
        }

        private void Start()
        {
            SetupPorts();
            GeneratePorts();
        }

        private void Update()
        {
            if (isConnected)
            {
                for (var i = 0; i < connectedPortsPositions.Count; i++)
                {
                    var connectedPinPosition = connectedPortsPositions[i];
                    if (connectedPorts[i] == null)
                    {
                        DisconnectAllPorts();
                        break;
                    }

                    var currentPinPosition = connectedPorts[i].transform.position;

                    if (connectedPinPosition != currentPinPosition)
                    {
                        UpdateComponentPosition();
                        break;
                    }
                }
            }
        }

        #endregion

        #region Abstract Methods

        protected abstract void SetupPorts();
        public abstract void Delete();

        #endregion

        #region Public Methods

        public void UpdatePinsConnection()
        {
            if (Pins.Count == 0)
                return;

            if (Pins != null && Pins.Count > 0)
            {
                var pins = new List<InoPort>();
                foreach (var pin in Pins)
                {
                    var contactGlobalPoint = RotatePointAroundPivot(transform.position + pin.Item2, transform.position,
                        transform.eulerAngles);
                    var ray = new Ray(contactGlobalPoint, Vector3.down);
                    if (Physics.Raycast(ray, out var hit))
                    {
                        var inoPort = hit.transform.GetComponent<InoPort>();
                        if (inoPort != null)
                            pins.Add(inoPort);
                    }
                }

                if (pins.Count == Pins.Count && pins.All(port => !port.IsConnected() || connectedPorts.Contains(port)))
                {
                    if (pins.Any(pin => !connectedPorts.Contains(pin)))
                    {
                        DisconnectAllPorts();

                        foreach (var connectedPort in pins)
                        {
                            connectedPort.Disable();
                            connectedPort.Connect(this);
                        }

                        connectedPorts = pins;
                        isConnected = true;
                    }

                    UpdateComponentPosition();
                }
                else
                {
                    DisconnectAllPorts();
                }
            }
        }

        public void EnableHighlight()
        {
            if (outlines == null)
            {
                outlines = new List<Outline>();
                foreach (var childRenderer in GetComponentsInChildren<Renderer>())
                {
                    var outline = childRenderer.gameObject.AddComponent<Outline>();
                    outlines.Add(outline);
                    OutlineEffect.Instance?.AddOutline(outline);
                }
                DisableHighlight();
            }

            foreach (var outline in outlines)
                outline.enabled = true;
        }

        public void DisableHighlight()
        {
            foreach (var outline in outlines)
                outline.enabled = false;
        }

        public bool IsConnected()
        {
            return isConnected;
        }

        #endregion

        #region Protected Methods

        protected void DisconnectAllPorts()
        {
            isConnected = false;
            foreach (var connectedPort in connectedPorts)
            {
                connectedPort.Enable();
                connectedPort.Disconnect();
            }
            connectedPorts.Clear();
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

                GeneratedPorts.Add(inoPort);
            }
        }

        private void UpdateComponentPosition()
        {
            var positions = Vector3.zero;
            connectedPortsPositions.Clear();
            foreach (var connectedPort in connectedPorts)
            {
                positions += connectedPort.transform.position;
                connectedPortsPositions.Add(connectedPort.transform.position);
            }

            if (connectedPorts.Count > 1)
            {
                var firstPosition = connectedPorts.First().transform.position;
                var lastPosition = connectedPorts.Last().transform.position;

                var middlePosition = positions / connectedPorts.Count;
                //transform.position = new Vector3(middlePosition.x, middlePosition.y, middlePosition.z);
                transform.position = new Vector3(middlePosition.x, transform.position.y, middlePosition.z);
                transform.rotation = Quaternion.LookRotation(firstPosition - lastPosition, Vector3.up);
            }
        }

        private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            var dir = point - pivot;
            dir = Quaternion.Euler(angles) * dir;
            point = dir + pivot;
            return point;
        }

        #endregion
    }
}