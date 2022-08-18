using System;
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
        private GameObject jumper2;

        private InoPort inoPort1;
        private Vector3 inoPort1Position;

        private InoPort inoPort2;
        private Vector3 inoPort2Position;

        private LineRenderer lineRenderer;

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
        }

        #endregion

        #region Public Methods

        public void Generate(InoPort port1, InoPort port2)
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponentInChildren<LineRenderer>();
                lineRenderer.useWorldSpace = true;

                lineRenderer.startWidth = Width;
                lineRenderer.endWidth = Width;
                lineRenderer.positionCount = NumberOfPoints;
            }

            if (jumper1 == null)
                jumper1 = Instantiate(port1.PinType == PinType.Female ? malePrefab : femalePrefab, transform);
            jumper1.transform.position = port1.transform.position;

            if (jumper2 == null)
                jumper2 = Instantiate(port2.PinType == PinType.Female ? malePrefab : femalePrefab, transform);
            jumper2.transform.position = port2.transform.position;

            inoPort1 = port1;
            inoPort2 = port2;

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
        }

        #endregion
    }
}