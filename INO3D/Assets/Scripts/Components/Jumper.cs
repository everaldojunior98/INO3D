using Assets.Scripts.Components.Base;
using UnityEngine;

namespace Assets.Scripts.Components
{
    public class Jumper : InoComponent
    {
        #region Fields

        private const float Width = 0.02f;
        private const float MaxDistance = 0.001f;
        private const float JumperOffset = 0.18f;
        private const int NumberOfPoints = 20;

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

        private LineRenderer lineRenderer;
        private MeshCollider meshCollider;

        #endregion

        #region Unity Methods

        private void Update()
        {
            if (Vector3.Distance(inoPort1.transform.position, inoPort1Position) > MaxDistance ||
                Vector3.Distance(inoPort2.transform.position, inoPort2Position) > MaxDistance)
            {
                Generate(inoPort1, inoPort2);
            }
        }

        #endregion

        #region Overrides

        protected override void SetupPorts()
        {
            CanDrag = false;
            CanRotate = false;
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

        public void Generate(InoPort port1, InoPort port2)
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponentInChildren<LineRenderer>();
                lineRenderer.useWorldSpace = true;

                meshCollider = gameObject.AddComponent<MeshCollider>();

                lineRenderer.startWidth = Width;
                lineRenderer.endWidth = Width;
                lineRenderer.positionCount = NumberOfPoints;

                inoPort1 = port1;
                inoPort2 = port2;

                inoPort1.Disable();
                inoPort1.Connect(this);

                inoPort2.Disable();
                inoPort2.Connect(this);
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
            
            var p0 = new Vector3(inoPort1Position.x, inoPort1Position.y + JumperOffset, inoPort1Position.z);
            var p3 = new Vector3(inoPort2Position.x, inoPort2Position.y + JumperOffset, inoPort2Position.z);
            var height = Vector3.Distance(p0, p3) / 2f;

            var p1 = new Vector3(p0.x, height + p0.y + JumperOffset, p0.z);
            var p2 = new Vector3(p3.x, height + p3.y + JumperOffset, p3.z);

            for (var i = 0; i < NumberOfPoints; i++)
            {
                var t = i / (NumberOfPoints - 1.0f);
                var position = Mathf.Pow(1 - t, 3) * p0 + 3 * Mathf.Pow(1 - t, 2) * t * p1 +
                               3 * (1 - t) * t * t * p2 + t * t * t * p3;
                lineRenderer.SetPosition(i, position);
            }

            var mesh = new Mesh();
            lineRenderer.BakeMesh(mesh);
            meshCollider.sharedMesh = mesh;
        }

        #endregion
    }
}