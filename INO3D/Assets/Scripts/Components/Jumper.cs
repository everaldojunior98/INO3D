using System.Collections.Generic;
using Assets.Scripts.Components.Base;
using Assets.Scripts.Managers;
using CircuitSharp.Components;
using CircuitSharp.Core;
using UnityEngine;

namespace Assets.Scripts.Components
{
    public class Jumper : InoComponent
    {
        #region Fields

        private const float Radius = 0.011f;
        private const float MaxDistance = 0.001f;

        private const float RigidJumperOffset = 0.03f;
        private const float NonRigidJumperOffset = 0.18f;
        
        private const int NumberOfBezierPoints = 20;

        private const int CrossSegments = 10;

        [SerializeField] GameObject malePrefab;
        [SerializeField] GameObject femalePrefab;

        private GameObject jumper1;
        private MeshRenderer jumper1MeshRenderer;
        private BoxCollider jumper1BoxCollider;

        private GameObject jumper2;
        private MeshRenderer jumper2MeshRenderer;
        private BoxCollider jumper2BoxCollider;

        private InoPort inoPort1;
        private Vector3 inoPort1Position;

        private InoPort inoPort2;
        private Vector3 inoPort2Position;

        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;

        private bool isRigid;
        private bool lastIsRigid;

        private int currentColor;
        private int lastColor;

        private List<Material> materials;

        #endregion

        #region Overrides

        public override void GenerateCircuitElement()
        {
            var wire = SimulationManager.Instance.CreateElement<Wire>();
            LeadByPortName = new Dictionary<string, Lead>
            {
                {"A", wire.LeadIn},
                {"B", wire.LeadOut}
            };

            foreach (var pair in ConnectedPorts)
                SimulationManager.Instance.Connect(GetLead(pair.Key), pair.Value.GetLead());
        }

        public override void OnSimulationTick()
        {
        }

        public override void DrawPropertiesWindow()
        {
            var colors = new []
            {
                LocalizationManager.Instance.Localize("ColorBlack"),
                LocalizationManager.Instance.Localize("ColorBlue"),
                LocalizationManager.Instance.Localize("ColorBrown"),
                LocalizationManager.Instance.Localize("ColorGray"),
                LocalizationManager.Instance.Localize("ColorOrange"),
                LocalizationManager.Instance.Localize("ColorRed"),
                LocalizationManager.Instance.Localize("ColorTurquoise"),
                LocalizationManager.Instance.Localize("ColorWhite"),
                LocalizationManager.Instance.Localize("ColorYellow")
            };
            UIManager.Instance.GenerateComboBoxPropertyField(LocalizationManager.Instance.Localize("Color"),
                ref currentColor, colors);
            UIManager.Instance.GenerateCheckboxPropertyField(LocalizationManager.Instance.Localize("IsRigid"),
                ref isRigid);
        }

        protected override void OnUpdate()
        {
            if (inoPort1 == null || inoPort2 == null)
                return;

            if (Vector3.Distance(inoPort1.transform.position, inoPort1Position) > MaxDistance ||
                Vector3.Distance(inoPort2.transform.position, inoPort2Position) > MaxDistance || isRigid != lastIsRigid)
            {
                Generate(inoPort1, inoPort2, currentColor, isRigid);
            }

            if (lastColor != currentColor)
            {
                meshRenderer.sharedMaterial = materials[currentColor];
                lastColor = currentColor;
            }
        }

        protected override void SetupPorts()
        {
            CanDrag = false;
            CanRotate = false;
            DefaultHeight = 0;
        }

        public override SaveFile Save()
        {
            var saveFile = new JumperSaveFile
            {
                PrefabName = "Jumper",

                Port1PositionX = inoPort1Position.x,
                Port1PositionY = inoPort1Position.y,
                Port1PositionZ = inoPort1Position.z,

                Port2PositionX = inoPort2Position.x,
                Port2PositionY = inoPort2Position.y,
                Port2PositionZ = inoPort2Position.z,

                CurrentColor = currentColor,
                IsRigid = isRigid
            };

            return saveFile;
        }

        public override void Load(SaveFile saveFile)
        {
            if (saveFile is JumperSaveFile jumperSave)
            {
                currentColor = jumperSave.CurrentColor;
                isRigid = jumperSave.IsRigid;
            }
        }

        public override void Delete()
        {
            inoPort1.Enable();
            inoPort1.Disconnect();

            inoPort2.Enable();
            inoPort2.Disconnect();

            Destroy(gameObject);
        }

        #endregion

        #region Public Methods

        public void Generate(InoPort port1, InoPort port2, int color, bool rigid)
        {
            if (meshRenderer == null)
            {
                materials = new List<Material>
                {
                    Resources.Load<Material>("3D Models\\Jumper\\Materials\\BlackWire"),
                    Resources.Load<Material>("3D Models\\Jumper\\Materials\\BlueWire"),
                    Resources.Load<Material>("3D Models\\Jumper\\Materials\\BrownWire"),
                    Resources.Load<Material>("3D Models\\Jumper\\Materials\\GrayWire"),
                    Resources.Load<Material>("3D Models\\Jumper\\Materials\\OrangeWire"),
                    Resources.Load<Material>("3D Models\\Jumper\\Materials\\RedWire"),
                    Resources.Load<Material>("3D Models\\Jumper\\Materials\\TurquoiseWire"),
                    Resources.Load<Material>("3D Models\\Jumper\\Materials\\WhiteWire"),
                    Resources.Load<Material>("3D Models\\Jumper\\Materials\\YellowWire")
                };

                meshRenderer = GetComponentInChildren<MeshRenderer>();
                meshCollider = gameObject.AddComponent<MeshCollider>();

                meshRenderer.sharedMaterial = materials[color];
                currentColor = color;

                inoPort1 = port1;
                inoPort2 = port2;

                inoPort1.Disable();
                inoPort1.Connect(this);

                inoPort2.Disable();
                inoPort2.Connect(this);

                ConnectedPorts = new Dictionary<string, InoPort>
                {
                    {"A", inoPort1},
                    {"B", inoPort2}
                };
            }

            if (jumper1 == null)
            {
                jumper1 = Instantiate(port1.PinType == PinType.Female ? malePrefab : femalePrefab, transform);
                jumper1MeshRenderer = jumper1.GetComponent<MeshRenderer>();
                jumper1BoxCollider = gameObject.AddComponent<BoxCollider>();
                jumper1BoxCollider.size = jumper1MeshRenderer.bounds.size;
            }
            jumper1.transform.position = port1.transform.position;
            jumper1.transform.eulerAngles = new Vector3(0, port1.transform.eulerAngles.y, 90);
            jumper1BoxCollider.center = new Vector3(port1.transform.position.x,
                port1.transform.position.y + jumper1MeshRenderer.bounds.size.y / 2f,
                port1.transform.position.z);

            if (jumper2 == null)
            {
                jumper2 = Instantiate(port2.PinType == PinType.Female ? malePrefab : femalePrefab, transform);
                jumper2MeshRenderer = jumper2.GetComponent<MeshRenderer>();
                jumper2BoxCollider = gameObject.AddComponent<BoxCollider>();
                jumper2BoxCollider.size = jumper2MeshRenderer.bounds.size;
            }
            jumper2.transform.position = port2.transform.position;
            jumper2.transform.eulerAngles = new Vector3(0, port2.transform.eulerAngles.y, 90);
            jumper2BoxCollider.center = new Vector3(port2.transform.position.x,
                port2.transform.position.y + jumper2MeshRenderer.bounds.size.y / 2f,
                port2.transform.position.z);

            inoPort1Position = port1.transform.position;
            inoPort2Position = port2.transform.position;

            var points = new List<Vector3>();
            if (rigid)
            {
                jumper1.SetActive(false);
                jumper1BoxCollider.isTrigger = true;
                jumper2.SetActive(false);
                jumper1BoxCollider.isTrigger = true;

                var initialPoint = new Vector3(inoPort1Position.x, inoPort1Position.y, inoPort1Position.z);
                var finalPoint = new Vector3(inoPort2Position.x, inoPort2Position.y, inoPort2Position.z);
                var middlePoint = (initialPoint + finalPoint) / 2;

                var controlPoint1 = new Vector3(initialPoint.x, initialPoint.y + RigidJumperOffset, initialPoint.z);
                var controlPoint2 = new Vector3(initialPoint.x, initialPoint.y + RigidJumperOffset, initialPoint.z);

                var controlPoint3 = new Vector3(finalPoint.x, finalPoint.y + RigidJumperOffset, finalPoint.z);
                var controlPoint4 = new Vector3(finalPoint.x, finalPoint.y + RigidJumperOffset, finalPoint.z);

                for (var i = 0; i < NumberOfBezierPoints / 2; i++)
                {
                    var t = i / (NumberOfBezierPoints - 1.0f);
                    var position = Mathf.Pow(1 - t, 3) * initialPoint + 3 * Mathf.Pow(1 - t, 2) * t * controlPoint1 + 3 * (1 - t) * t * t * controlPoint2 + t * t * t * middlePoint;
                    points.Add(position);
                }

                for (var i = NumberOfBezierPoints / 2; i < NumberOfBezierPoints; i++)
                {
                    var t = i / (NumberOfBezierPoints - 1.0f);
                    var position = Mathf.Pow(1 - t, 3) * middlePoint + 3 * Mathf.Pow(1 - t, 2) * t * controlPoint3 + 3 * (1 - t) * t * t * controlPoint4 + t * t * t * finalPoint;
                    points.Add(position);
                }
            }
            else
            {
                jumper1.SetActive(true);
                jumper1BoxCollider.isTrigger = false;
                jumper2.SetActive(true);
                jumper2BoxCollider.isTrigger = false;

                var initialPoint = new Vector3(inoPort1Position.x, inoPort1Position.y + NonRigidJumperOffset, inoPort1Position.z);
                var finalPoint = new Vector3(inoPort2Position.x, inoPort2Position.y + NonRigidJumperOffset, inoPort2Position.z);
                var height = Vector3.Distance(initialPoint, finalPoint) / 2f;

                var controlPoint1 = new Vector3(initialPoint.x, height + initialPoint.y + NonRigidJumperOffset, initialPoint.z);
                var controlPoint2 = new Vector3(finalPoint.x, height + finalPoint.y + NonRigidJumperOffset, finalPoint.z);

                for (var i = 0; i < NumberOfBezierPoints; i++)
                {
                    var t = i / (NumberOfBezierPoints - 1.0f);
                    var position = Mathf.Pow(1 - t, 3) * initialPoint + 3 * Mathf.Pow(1 - t, 2) * t * controlPoint1 + 3 * (1 - t) * t * t * controlPoint2 + t * t * t * finalPoint;
                    points.Add(position);
                }
            }

            var crossPoints = new Vector3[CrossSegments];
            var theta = 2.0f * Mathf.PI / CrossSegments;
            for (var c = 0; c < CrossSegments; c++)
                crossPoints[c] = new Vector3(Mathf.Cos(theta * c), Mathf.Sin(theta * c), 0);

            var vertices = new Vector3[points.Count + 2];

            var v0Offset = (points[0] - points[1]) * 0.01f;
            vertices[0] = v0Offset + points[0];
            var v1Offset = (points[points.Count - 1] - points[points.Count - 2]) * 0.01f;
            vertices[vertices.Length - 1] = v1Offset + points[points.Count - 1];

            for (var p = 0; p < points.Count; p++)
                vertices[p + 1] = points[p];

            var meshVertices = new Vector3[vertices.Length * CrossSegments];
            var uvs = new Vector2[vertices.Length * CrossSegments];
            var triangles = new int[vertices.Length * CrossSegments * 6];
            var lastVertices = new int[CrossSegments];
            var theseVertices = new int[CrossSegments];
            var rotation = Quaternion.identity;

            for (var p = 0; p < vertices.Length; p++)
            {
                if (p < vertices.Length - 1)
                {
                    var eulerAngles = Quaternion.FromToRotation(Vector3.forward, vertices[p + 1] - vertices[p]).eulerAngles;
                    rotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, 0);
                }

                for (var c = 0; c < CrossSegments; c++)
                {
                    var vertexIndex = p * CrossSegments + c;
                    meshVertices[vertexIndex] = vertices[p] + rotation * crossPoints[c] * Radius;
                    uvs[vertexIndex] = new Vector2((float) c / CrossSegments, (float) p / vertices.Length);

                    lastVertices[c] = theseVertices[c];
                    theseVertices[c] = p * CrossSegments + c;
                }

                if (p > 0)
                {
                    for (var c = 0; c < CrossSegments; c++)
                    {
                        var start = (p * CrossSegments + c) * 6;
                        triangles[start] = lastVertices[c];
                        triangles[start + 1] = lastVertices[(c + 1) % CrossSegments];
                        triangles[start + 2] = theseVertices[c];
                        triangles[start + 3] = triangles[start + 2];
                        triangles[start + 4] = triangles[start + 1];
                        triangles[start + 5] = theseVertices[(c + 1) % CrossSegments];
                    }
                }
            }

            var mesh = GetComponentInChildren<MeshFilter>().mesh;
            if (!mesh)
                mesh = new Mesh();

            mesh.vertices = meshVertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            meshCollider.sharedMesh = mesh;
            isRigid = rigid;
            lastIsRigid = rigid;
        }

        #endregion
    }
}