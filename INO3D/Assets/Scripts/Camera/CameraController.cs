using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Managers;
using UnityEngine;
using UnityCamera = UnityEngine.Camera;

namespace Assets.Scripts.Camera
{
    public class CameraController : MonoBehaviour
    {
        #region Properties

        public enum CameraProjection
        {
            Orthographic,
            Perspective
        }

        public static CameraController Instance { get; private set; }

        #endregion

        #region Fields

        private const float ZoomFactor = 0.5f;
        private const float PanFactor = 0.02f;
        private const float RotateFactor = 1f;

        private const float Smoothness = 10f;

        private Transform orbitTransform;

        private Vector3 camLockOffset;
        private GameObject gameObjectToLock;

        private float lockDistanceToOrbit;
        private float distanceToOrbit;

        private Vector3 lastMouse;
        private UnityCamera cam;

        private float PanSpeed => LocalizationManager.Instance.GetCameraSensitivity() * PanFactor * 1.5f * (cam.orthographic ? cam.orthographicSize : distanceToOrbit);
        private float RotateSpeed => LocalizationManager.Instance.GetCameraSensitivity() * RotateFactor;
        private float ZoomSpeed => LocalizationManager.Instance.GetCameraSensitivity() * ZoomFactor * 1.5f * (cam.orthographic ? cam.orthographicSize : distanceToOrbit);

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            else
                Instance = this;
        }

        private void Start()
        {
            orbitTransform = GameObject.CreatePrimitive(PrimitiveType.Capsule).transform;
            orbitTransform.position = transform.position;
            orbitTransform.rotation = transform.rotation;
            lastMouse = Input.mousePosition;

            orbitTransform.GetComponent<Renderer>().enabled = false;
            orbitTransform.GetComponent<CapsuleCollider>().enabled = false;
            cam = GetComponent<UnityCamera>();
            Reset();
        }

        private void Update()
        {
            if (gameObjectToLock != null)
            {
                if (!UIManager.Instance.IsMouserOverUI())
                    lockDistanceToOrbit -= ZoomSpeed * Input.GetAxis("Mouse ScrollWheel");

                if (cam.orthographic)
                    cam.orthographicSize = lockDistanceToOrbit;
                else
                    transform.position = Vector3.Lerp(transform.position, orbitTransform.position - transform.forward * lockDistanceToOrbit, Smoothness * Time.deltaTime);
                orbitTransform.position = gameObjectToLock.transform.position + camLockOffset;
            }

            if (UIManager.Instance.IsMouserOverUI())
                return;

            UpdateMouse();

            if (Input.GetKeyDown(KeyCode.F))
            {
                var selectedComponent = ComponentsManager.Instance.GetSelectedComponent();
                if (selectedComponent != null)
                    FocusOnGameObject(new[] { selectedComponent.gameObject });
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                if (gameObjectToLock != null)
                {
                    SetTarget(null);
                }
                else
                {
                    var selectedComponent = ComponentsManager.Instance.GetSelectedComponent();
                    if (selectedComponent != null)
                        SetTarget(selectedComponent.gameObject);
                }
            }
            else if (Input.GetKeyDown(KeyCode.G))
            {
                Reset();
            }
            else if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (GetCameraProjection() == CameraProjection.Orthographic)
                    SetCameraAsPerspective();
                else
                    SetCameraAsOrthographic();
            }
        }

        #endregion

        #region Public Methods

        public void SetCameraAsOrthographic()
        {
            if(gameObjectToLock != null)
                return;

            transform.parent = orbitTransform;
            orbitTransform.rotation = Quaternion.Euler(90, 0, 0);
            transform.parent = null;

            cam.orthographicSize = distanceToOrbit / 1.5f;
            cam.orthographic = true;
            transform.position = new Vector3(transform.position.x, 10, transform.position.z);
        }

        public void SetCameraAsPerspective()
        {
            transform.position = orbitTransform.position - transform.forward * 1.5f * cam.orthographicSize;
            cam.orthographic = false;
        }

        public void SetTarget(GameObject newTarget)
        {
            if (newTarget != null)
            {
                gameObjectToLock = newTarget;
                SetCameraAsPerspective();
                FocusOnGameObject(new[] {newTarget});
            }
            else
            {
                gameObjectToLock = null;
            }
        }

        public CameraProjection GetCameraProjection()
        {
            return cam.orthographic ? CameraProjection.Orthographic : CameraProjection.Perspective;
        }

        public void Reset()
        {
            gameObjectToLock = null;
            var components = ComponentsManager.Instance.GetSceneComponents().Select(component => component.gameObject).ToArray();
            FocusOnGameObject(components);
        }

        #endregion

        #region Private Methods

        private void UpdateMouse()
        {
            var deltaMouse = Input.mousePosition - lastMouse;
            lastMouse = Input.mousePosition;

            var mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");
            var rotationEnabled = Input.GetMouseButton(1) && !cam.orthographic;
            var zoomEnabled = mouseScrollWheel != 0 && gameObjectToLock == null;
            var panEnabled = Input.GetMouseButton(2) && gameObjectToLock == null;

            distanceToOrbit = Vector3.Distance(transform.position, orbitTransform.position);
            if (zoomEnabled)
                Zoom(mouseScrollWheel);

            var mouseX = deltaMouse.x / 20;
            var mouseY = deltaMouse.y / 20;

            if (panEnabled)
            {
                PanRight(mouseX);
                PanUp(mouseY);
            }

            if (rotationEnabled)
            {
                RotateUp(-mouseY);
                RotateRight(-mouseX);
            }
        }

        private void FocusOnGameObject(GameObject[] objects)
        {
            var center = objects.Aggregate(Vector3.zero, (current, obj) => current + obj.transform.position) / objects.Length;

            var bounds = new Bounds(center, Vector3.zero);
            foreach (var meshRenderer in objects.SelectMany(obj => obj.GetComponentsInChildren<MeshRenderer>()))
                bounds.Encapsulate(meshRenderer.bounds);

            var radius = bounds.size.magnitude / 2f;
            var horizontalFov = 2f * Mathf.Atan(Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad / 2f) * cam.aspect) * Mathf.Rad2Deg;
            var fov = Mathf.Min(cam.fieldOfView, horizontalFov);
            var dist = radius / (Mathf.Sin(fov * Mathf.Deg2Rad / 2f));

            if(gameObjectToLock != null)
            {
                camLockOffset = bounds.center - gameObjectToLock.transform.position;
                lockDistanceToOrbit = dist;
            }

            if (float.IsNaN(bounds.center.x) || float.IsNaN(bounds.center.y) || float.IsNaN(bounds.center.z))
                return;

            cam.orthographicSize = radius;
            orbitTransform.position = bounds.center;
            transform.position = bounds.center - transform.forward * dist;
        }

        private void Zoom(float value)
        {
            if (cam.orthographic)
                cam.orthographicSize -= ZoomSpeed * value;
            else
            {
                cam.orthographicSize = Vector3.Distance(orbitTransform.position, transform.position);
                transform.Translate(ZoomSpeed * value * Vector3.forward);
            }
        }

        private void PanRight(float value)
        {
            transform.parent = orbitTransform;
            orbitTransform.Translate(value * PanSpeed * -Vector3.right, transform);
            transform.parent = null;
        }

        private void PanUp(float value)
        {
            transform.parent = orbitTransform;
            orbitTransform.Translate(value * PanSpeed * -Vector3.up, transform);
            transform.parent = null;
        }

        private void RotateRight(float value)
        {
            transform.parent = orbitTransform;
            orbitTransform.Rotate(value * RotateSpeed * -Vector3.up, Space.World);
            transform.parent = null;
        }

        private void RotateUp(float eventValue)
        {
            transform.parent = orbitTransform;
            orbitTransform.Rotate(eventValue * RotateSpeed * Vector3.right);
            transform.parent = null;
        }

        #endregion
    }
}