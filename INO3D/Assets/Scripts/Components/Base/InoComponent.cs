using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Assets.Scripts.Managers;
using cakeslice;
using UnityEngine;
using static SharpCircuit.Circuit;
using Exception = System.Exception;

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

        public string Hash { get; private set; }

        protected List<Tuple<string, Vector3, PortType, PinType, bool, bool, Vector3>> Ports;
        protected List<InoPort> GeneratedPorts;
        protected Dictionary<string, Lead> LeadByPortName;

        protected List<Tuple<string, Vector3>> Pins;

        protected bool DebugPins;

        public bool CanDrag { get; protected set; }
        public bool CanRotate { get; protected set; }

        public float DefaultHeight  { get; protected set; }

        private List<Outline> outlines;

        private bool isConnected;

        protected Dictionary<string, InoPort> ConnectedPorts;
        private Dictionary<string, Vector3> connectedPortsPositions;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            Ports = new List<Tuple<string, Vector3, PortType, PinType, bool, bool, Vector3>>();
            GeneratedPorts = new List<InoPort>();

            Pins = new List<Tuple<string, Vector3>>();

            ConnectedPorts = new Dictionary<string, InoPort>();
            connectedPortsPositions = new Dictionary<string, Vector3>();

            CanDrag = true;
            CanRotate = true;
        }

        private void Start()
        {
            SetupPorts();
            GeneratePorts();
            transform.position = new Vector3(transform.position.x, DefaultHeight, transform.position.z);

            var creationTime = DateTime.Now.Ticks.ToString("yyMMddHHmmssff") + GetInstanceID();
            var sb = new StringBuilder();
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(creationTime));

                foreach (var b in hash)
                    sb.Append(b.ToString("X2"));
            }

            Hash = sb.ToString();
        }

        private void Update()
        {
            OnUpdate();
            if (isConnected)
            {
                foreach (var pair in connectedPortsPositions)
                {
                    if (!ConnectedPorts.ContainsKey(pair.Key))
                    {
                        DisconnectAllPorts();
                        break;
                    }

                    var currentPinPosition = ConnectedPorts[pair.Key].transform.position;
                    if (pair.Value != currentPinPosition)
                    {
                        UpdateComponentPosition();
                        break;
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if(!DebugPins)
                return;

            Gizmos.color = Color.red;
            foreach (var pin in Pins)
            {
                var contactGlobalPoint = RotatePointAroundPivot(
                    transform.position + pin.Item2 +
                    new Vector3(0, ComponentsManager.Instance.DefaultIndicatorSize.y, 0), transform.position,
                    transform.eulerAngles);
                var ray = new Ray(contactGlobalPoint, Vector3.down);
                Gizmos.DrawRay(ray);
            }
        }

        #endregion

        #region Virtual Methods

        protected virtual void OnUpdate()
        {

        }

        #endregion

        #region Abstract Methods

        public abstract void GenerateCircuitElement();
        public abstract void OnSimulationTick();
        public abstract void DrawPropertiesWindow();
        protected abstract void SetupPorts();
        public abstract SaveFile Save();
        public abstract void Load(SaveFile saveFile);
        public abstract void Delete();

        #endregion

        #region Public Methods

        public List<string> GetDependencies()
        {
            var dependencies = new List<string>();
            foreach (var connectedPort in ConnectedPorts)
            {
                var hash = connectedPort.Value.transform.parent.GetComponent<InoComponent>().Hash;
                if (!dependencies.Contains(hash))
                    dependencies.Add(hash);
            }
            return dependencies;
        }

        public void UpdatePinsConnection()
        {
            if (Pins.Count == 0)
                return;

            if (Pins != null && Pins.Count > 0)
            {
                var pins = new Dictionary<string, InoPort>();
                foreach (var pin in Pins)
                {
                    var contactGlobalPoint = RotatePointAroundPivot(
                        transform.position + pin.Item2 +
                        new Vector3(0, ComponentsManager.Instance.DefaultIndicatorSize.y, 0), transform.position,
                        transform.eulerAngles);
                    var ray = new Ray(contactGlobalPoint, Vector3.down);
                    if (Physics.Raycast(ray, out var hit))
                    {
                        var inoPort = hit.transform.GetComponent<InoPort>();
                        if (inoPort != null)
                            pins.Add(pin.Item1, inoPort);
                    }
                }

                if (pins.Count == Pins.Count && pins.All(port => !port.Value.IsConnected()))
                {
                    if (pins.Any(pin => !ConnectedPorts.ContainsKey(pin.Key) || ConnectedPorts[pin.Key] != pin.Value))
                    {
                        DisconnectAllPorts();

                        foreach (var connectedPort in pins)
                        {
                            connectedPort.Value.Disable();
                            connectedPort.Value.Connect(this);
                        }

                        ConnectedPorts = pins;
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

        public bool IsAttachable()
        {
            return Pins.Count > 0;
        }

        #endregion

        #region Protected Methods

        protected void DisconnectAllPorts()
        {
            isConnected = false;
            foreach (var connectedPort in ConnectedPorts)
            {
                connectedPort.Value.Enable();
                connectedPort.Value.Disconnect();
            }
            ConnectedPorts.Clear();
        }

        protected Lead GetLead(string portName)
        {
            if (LeadByPortName != null && LeadByPortName.ContainsKey(portName))
                return LeadByPortName[portName];
            throw new Exception("Unknow lead");
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
                inoPort.CanBeRigid = tuple.Item5;
                inoPort.IsTerminalBlock = tuple.Item6;
                inoPort.WireDirection = tuple.Item7;
                inoPort.Component = this;
                inoPort.GetLead = () => GetLead(tuple.Item1);

                GeneratedPorts.Add(inoPort);
            }
        }

        private void UpdateComponentPosition()
        {
            var positions = Vector3.zero;
            var firstPosition = Vector3.zero;
            var lastPosition = Vector3.zero;

            var orderedPositions = ConnectedPorts.OrderBy(kv => kv.Value.transform.position.y)
                .ThenBy(kv => kv.Value.transform.position.x).ToArray();

            connectedPortsPositions.Clear();
            foreach (var connectedPort in ConnectedPorts)
            {
                positions += connectedPort.Value.transform.position;
                connectedPortsPositions.Add(connectedPort.Key, connectedPort.Value.transform.position);
            }

            if (ConnectedPorts.Count == 2)
            {
                firstPosition = orderedPositions[0].Value.transform.position;
                lastPosition = orderedPositions[1].Value.transform.position;
            }
            else if (ConnectedPorts.Count == 4)
            {
                firstPosition = orderedPositions[0].Value.transform.position;
                lastPosition = orderedPositions[1].Value.transform.position;
            }

            var middlePosition = positions / ConnectedPorts.Count;
            transform.position = new Vector3(middlePosition.x, middlePosition.y, middlePosition.z);
            transform.rotation = Quaternion.LookRotation(firstPosition - lastPosition, Vector3.up);
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