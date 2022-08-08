using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region Singleton

    public static CameraController Instance { get; private set; }

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();

        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    #endregion

    #region Fields

    [SerializeField] Transform target;

    [Header("Speed")]
    [SerializeField] float rotateSpeed = 150f;
    [SerializeField] float zoomSpeed = 100f;
    [SerializeField] float dragSpeed = 20f;

    [Header("Zoom")]
    [SerializeField] float minDistance = 0.5f;
    [SerializeField] float maxDistance = 12f;

    private Camera mainCamera;
    private float currentZoom;

    private float lastAngle;

    #endregion

    #region Unity Methods

    private void Start()
    {
        lastAngle = transform.eulerAngles.x;
        SetCameraAsPerspective();
    }

    private void Update()
    {
        PerspectiveCameraControl();
    }

    #endregion

    #region Private Methods

    private void SetCameraAsPerspective()
    {
        var cameraLookingPoint = Vector3.zero;
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out var hit))
            cameraLookingPoint = hit.point;

        mainCamera.orthographic = false;
        var defaultAngleRad = Mathf.Deg2Rad * lastAngle;
        transform.eulerAngles = new Vector3(lastAngle, 0, 0);

        target.position = new Vector3(cameraLookingPoint.x, target.position.y, cameraLookingPoint.z);
        currentZoom = Vector3.Distance(transform.position, target.position);

        var x = currentZoom * Mathf.Cos(defaultAngleRad);
        var y = currentZoom * Mathf.Sin(defaultAngleRad);

        transform.position = new Vector3(cameraLookingPoint.x, y, -x);
    }

    private void PerspectiveCameraControl()
    {
        currentZoom = Vector3.Distance(transform.position, target.position);

        //Rotate
        if (Input.GetKey(KeyCode.Mouse1))
        {
            transform.RotateAround(target.position, Vector3.up,
                (Input.GetAxisRaw("Mouse X") * Time.deltaTime * rotateSpeed * 10));

            var angle = -(Input.GetAxisRaw("Mouse Y") * Time.deltaTime * rotateSpeed * 10);
            transform.RotateAround(target.position, transform.right, angle);
        }

        //Drag
        if (Input.GetKey(KeyCode.Mouse2))
        {
            var direction = new Vector3(-Input.GetAxisRaw("Mouse X") * Time.deltaTime * dragSpeed * currentZoom,
                -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * dragSpeed * currentZoom, 0);
            transform.Translate(direction);
            target.Translate(direction);
            target.position = new Vector3(target.position.x, 0, target.position.z);
        }

        //Zoom
        if (currentZoom >= minDistance && Input.GetAxis("Mouse ScrollWheel") > 0f ||
            currentZoom <= maxDistance && Input.GetAxis("Mouse ScrollWheel") < 0f)
            transform.Translate(0f, 0f, Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomSpeed * 10,
                Space.Self);
    }

    #endregion
}