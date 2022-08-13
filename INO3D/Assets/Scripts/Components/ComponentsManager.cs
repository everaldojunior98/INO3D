using Assets.Scripts.Components.Base;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Components
{
    public class ComponentsManager : MonoBehaviour
    {
        #region Properties

        public static ComponentsManager Instance { get; private set; }

        #endregion

        #region Fields

        [SerializeField] LayerMask inoLayerMask;
        [SerializeField] KeyCode selectButton;

        private InoComponent selectedComponent;
        private Camera mainCamera;

        private bool canDrag;
        private bool isDragging;

        private Vector3 dragStartPosition;

        private Material selectedMaterial;
        private Material unselectedMaterial;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            else
                Instance = this;

            selectedMaterial = Resources.Load("Materials/PortRed", typeof(Material)) as Material;
            unselectedMaterial = Resources.Load("Materials/PortGreen", typeof(Material)) as Material;
        }

        private void Start()
        {
            mainCamera = CameraController.Instance.GetMainCamera();
        }

        private void Update()
        {
            if (Input.GetKeyDown(selectButton))
            {
                var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, inoLayerMask))
                {
                    var component = hit.transform.GetComponent<InoComponent>();
                    if (component != null)
                    {
                        canDrag = true;
                        isDragging = false;
                        selectedComponent = component;
                    }
                    else
                    {
                        canDrag = false;
                        isDragging = false;
                        selectedComponent = null;
                    }
                }
                else
                {
                    canDrag = false;
                    isDragging = false;
                    selectedComponent = null;
                }
            }

            if (Input.GetKey(selectButton))
            {
                if (canDrag)
                {
                    var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out var hit, float.MaxValue, ~inoLayerMask))
                        if (selectedComponent != null)
                        {
                            if (!isDragging)
                                dragStartPosition = hit.point;

                            selectedComponent.transform.position += hit.point - dragStartPosition;
                            dragStartPosition = hit.point;
                            isDragging = true;
                        }
                }
            }

            if (Input.GetKeyUp(selectButton))
            {
                canDrag = false;
                isDragging = false;
            }
        }

        #endregion

        #region Public Methods

        public Material GetSelectedMaterial()
        {
            return selectedMaterial;
        }

        public Material GetUnselectedMaterial()
        {
            return unselectedMaterial;
        }

        public void OnPortSelected(InoPort port)
        {

        }

        public void OnPortUnselected(InoPort port)
        {

        }

        #endregion
    }
}