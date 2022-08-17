using System;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public class CameraController : MonoBehaviour
    {
        #region Properties

        public static CameraController Instance { get; private set; }

        #endregion

        #region Fields

        [Header("Speed")] [SerializeField] float rotateSpeed = 150f;
        [SerializeField] float zoomSpeed = 100f;
        [SerializeField] float dragSpeed = 20f;

        [Header("Zoom")] [SerializeField] float minDistance = 0.5f;
        [SerializeField] float maxDistance = 12f;

        private GameObject target;
        private Camera mainCamera;
        private float currentZoom;

        private Plane floorPlane;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            mainCamera = GetComponent<Camera>();

            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;

            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SetCameraAsPerspective();
        }

        private void Update()
        {
            if (UIManager.Instance.IsMouserOverUI())
                return;

            PerspectiveCameraControl();
        }

        #endregion

        #region Public Methods

        public Camera GetMainCamera()
        {
            return mainCamera;
        }

        #endregion

        #region Private Methods

        private void SetCameraAsPerspective()
        {
            floorPlane = new Plane(Vector3.up, 0);
            target = new GameObject("CAMERA_TARGET")
            {
                transform =
                {
                    position = Vector3.zero
                }
            };

            currentZoom = Vector3.Distance(transform.position, target.transform.position);

            var currentAngle = transform.eulerAngles.x * Mathf.Deg2Rad;
            var y = currentZoom * (float) Math.Sin(currentAngle);
            var z = currentZoom * (float) Math.Cos(currentAngle);

            transform.position = new Vector3(0, y, -z);
        }

        private void PerspectiveCameraControl()
        {
            currentZoom = Vector3.Distance(transform.position, target.transform.position);

            //Rotate
            if (Input.GetKey(KeyCode.Mouse1))
            {
                transform.RotateAround(target.transform.position, Vector3.up,
                    (Input.GetAxisRaw("Mouse X") * 0.001f * rotateSpeed));

                var angle = -(Input.GetAxisRaw("Mouse Y") * 0.001f * rotateSpeed);
                transform.RotateAround(target.transform.position, transform.right, angle);
            }

            //Drag
            if (Input.GetKey(KeyCode.Mouse2))
            {
                var direction = new Vector3(-Input.GetAxisRaw("Mouse X") * 0.001f * dragSpeed * currentZoom,
                    -Input.GetAxisRaw("Mouse Y") * 0.001f * dragSpeed * currentZoom, 0);

                var ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
                if (floorPlane.Raycast(ray, out var enter))
                {
                    transform.Translate(direction);

                    var hit = ray.GetPoint(enter);
                    target.transform.position = new Vector3(hit.x, 0, hit.z);
                }
            }

            //Zoom
            if (currentZoom >= minDistance && Input.GetAxis("Mouse ScrollWheel") > 0f ||
                currentZoom <= maxDistance && Input.GetAxis("Mouse ScrollWheel") < 0f)
                transform.Translate(0f, 0f, Input.GetAxis("Mouse ScrollWheel") * 0.001f * zoomSpeed,
                    Space.Self);
        }

        #endregion
    }
}