using System;
using Assets.Scripts.Camera;
using UnityEngine;

namespace Assets.Scripts.Simulation
{
    public class CircuitWarning : MonoBehaviour
    {
        #region Properties

        public float Min = 0.5f;
        public float Max = 1f;
        public float Frequency = 1f;
        
        #endregion

        #region Fields

        private Transform cameraTransform;
        private CameraController cameraController;
        private UnityEngine.Camera cam;

        private SpriteRenderer spriteRenderer;
        private Color color;

        private float currentAlpha;
        private bool increase = true;

        #endregion

        #region Unity Methods

        private void Start()
        {
            cam = UnityEngine.Camera.main;
            cameraTransform = cam.transform;
            cameraController = cam.GetComponent<CameraController>();

            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            color = spriteRenderer.color;
            currentAlpha = color.a;
        }

        private void Update()
        {
            if (cameraController.GetCameraProjection() == CameraController.CameraProjection.Orthographic)
            {
                transform.localScale = Vector3.one * Math.Max(0.03f, 2 * cam.orthographicSize / 80f);
                transform.rotation = Quaternion.Euler(-90, 180, 0);
            }
            else
            {
                transform.localScale = Vector3.one * Math.Max(0.03f, Vector3.Distance(transform.position, cameraController.transform.position) / 80f);
                transform.LookAt(cameraTransform);
            }

            if (currentAlpha >= Max) 
                increase = false;
            if (currentAlpha <= Min) 
                increase = true;

            currentAlpha = increase
                ? currentAlpha += Time.deltaTime * Frequency
                : currentAlpha -= Time.deltaTime * Frequency;
            color.a = currentAlpha;
            spriteRenderer.color = color;
        }

        #endregion
    }
}