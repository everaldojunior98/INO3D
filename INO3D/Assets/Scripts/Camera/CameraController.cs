using System;
using Assets.Scripts.Managers;
using UnityEngine;
using UnityCamera = UnityEngine.Camera;

namespace Assets.Scripts.Camera
{
    public class CameraController : MonoBehaviour
    {
        #region Properties

        public static CameraController Instance { get; private set; }

        #endregion

        #region Fields

        public enum CameraProjection
        {
            Orthographic,
            Perspective
        }

        [Header("Speed")] [SerializeField] float rotateSpeed = 150f;
        [SerializeField] float zoomSpeed = 100f;
        [SerializeField] float dragSpeed = 20f;

        [Header("Zoom")] [SerializeField] float minDistance = 0.5f;
        [SerializeField] float maxDistance = 12f;

        private GameObject target;
        private UnityCamera mainCamera;
        private float currentZoom;
        private CameraProjection currentProjection;

        private Plane floorPlane;

        private float lastAngle;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            mainCamera = GetComponent<UnityCamera>();

            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;
        }

        private void Start()
        {
            floorPlane = new Plane(Vector3.up, 0);
            target = new GameObject("CAMERA_TARGET")
            {
                transform =
                {
                    position = Vector3.zero
                }
            };
            mainCamera.orthographicSize = 2;
            lastAngle = transform.eulerAngles.x;
            SetCameraAsPerspective();
        }

        private void Update()
        {
            if (UIManager.Instance.IsMouserOverUI())
                return;

            if (currentProjection == CameraProjection.Perspective)
                PerspectiveCameraControl();
            else if (currentProjection == CameraProjection.Orthographic)
                OrthographicCameraControl();

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (currentProjection == CameraProjection.Orthographic)
                    SetCameraAsPerspective();
                else
                    SetCameraAsOrthographic();
            }
        }

        #endregion

        #region Public Methods

        public float GetCurrentZoom()
        {
            return currentZoom;
        }

        public CameraProjection GetCameraProjection()
        {
            return currentProjection;
        }

        public UnityCamera GetMainCamera()
        {
            return mainCamera;
        }

        public void SetCameraAsPerspective()
        {
            var cameraLookingPoint = Vector3.zero;
            if (Physics.Raycast(transform.position, transform.forward, out var hit))
                cameraLookingPoint = hit.point;

            currentProjection = CameraProjection.Perspective;
            mainCamera.orthographic = false;
            transform.eulerAngles = new Vector3(lastAngle, 0, 0);

            target.transform.position = new Vector3(cameraLookingPoint.x, target.transform.position.y,
                Math.Abs(target.transform.position.z - cameraLookingPoint.z) > 1f
                    ? cameraLookingPoint.z
                    : target.transform.position.z);
            currentZoom = OrthographicSizeToZoom(mainCamera.orthographicSize);

            var defaultAngleRad = Mathf.Deg2Rad * lastAngle;
            var y = currentZoom * (float)Math.Sin(defaultAngleRad);
            var z = currentZoom * (float)Math.Cos(defaultAngleRad);

            transform.position = new Vector3(cameraLookingPoint.x, y, target.transform.position.z - z);
        }

        public void SetCameraAsOrthographic()
        {
            var cameraLookingPoint = Vector3.zero;
            if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out var hit))
                cameraLookingPoint = hit.point;

            currentProjection = CameraProjection.Orthographic;
            mainCamera.orthographic = true;
            transform.eulerAngles = new Vector3(90, 0, 0);

            transform.position = new Vector3(cameraLookingPoint.x, 10, cameraLookingPoint.z);

            var size = 0.594469f * currentZoom - 0.101753f;
            mainCamera.orthographicSize = size;
        }

        #endregion

        #region Private Methods

        private void PerspectiveCameraControl()
        {
            currentZoom = Vector3.Distance(transform.position, target.transform.position);
            var cameraSensitivity = LocalizationManager.Instance.GetCameraSensitivity();

            //Rotate
            if (Input.GetKey(KeyCode.Mouse1))
            {
                transform.RotateAround(target.transform.position, Vector3.up,
                    Input.GetAxisRaw("Mouse X") * 0.001f * rotateSpeed * cameraSensitivity);

                var angle = -(Input.GetAxisRaw("Mouse Y") * 0.001f * rotateSpeed * cameraSensitivity);
                transform.RotateAround(target.transform.position, transform.right, angle);
            }

            //Drag
            if (Input.GetKey(KeyCode.Mouse2))
            {
                var direction = new Vector3(-Input.GetAxisRaw("Mouse X") * 0.001f * dragSpeed * currentZoom * cameraSensitivity,
                    -Input.GetAxisRaw("Mouse Y") * 0.001f * dragSpeed * currentZoom * cameraSensitivity, 0);

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
                transform.Translate(0f, 0f, Input.GetAxis("Mouse ScrollWheel") * 0.001f * zoomSpeed * cameraSensitivity,
                    Space.Self);
        }

        private void OrthographicCameraControl()
        {
            var zoom = OrthographicSizeToZoom(mainCamera.orthographicSize);
            var cameraSensitivity = LocalizationManager.Instance.GetCameraSensitivity();

            //Drag
            if (Input.GetKey(KeyCode.Mouse2))
            {
                var direction = new Vector3(-Input.GetAxisRaw("Mouse X") * 0.001f * dragSpeed * zoom * cameraSensitivity,
                    -Input.GetAxisRaw("Mouse Y") * 0.001f * dragSpeed * zoom * cameraSensitivity, 0);

                var ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
                if (floorPlane.Raycast(ray, out var enter))
                {
                    transform.Translate(direction);

                    var hit = ray.GetPoint(enter);
                    target.transform.position = new Vector3(hit.x, 0, hit.z);
                }
            }

            //Zoom
            if (zoom >= minDistance && Input.GetAxis("Mouse ScrollWheel") > 0f ||
                zoom <= maxDistance && Input.GetAxis("Mouse ScrollWheel") < 0f)
                mainCamera.orthographicSize =
                    Mathf.Clamp(
                        mainCamera.orthographicSize -
                        Input.GetAxis("Mouse ScrollWheel") * 0.001f * zoomSpeed * cameraSensitivity,
                        minDistance, maxDistance);
        }

        private float OrthographicSizeToZoom(float orthographicSize)
        {
            return (orthographicSize + 0.101753f) / 0.594469f;
        }

        #endregion
    }
}