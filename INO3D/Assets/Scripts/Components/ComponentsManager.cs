using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] LayerMask floorLayerMask;
        [SerializeField] KeyCode selectButton;

        [SerializeField] GameObject jumperPrefab;

        private InoComponent selectedComponent;
        private Camera mainCamera;

        private bool canDrag;
        private bool isDragging;
        private bool isAdding;

        private Vector3 dragStartPosition;

        private Material selectedMaterial;
        private Material unselectedMaterial;

        private List<InoPort> selectedPorts;

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
            selectedPorts = new List<InoPort>();
        }

        private void Update()
        {
            if(UIManager.Instance.IsMouserOverUI())
                return;

            if (Input.GetKeyDown(selectButton))
            {
                var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, inoLayerMask))
                {
                    var component = hit.transform.GetComponent<InoComponent>();
                    if (component != null)
                    {
                        SelectComponent(component);
                    }
                    else if(!isAdding)
                    {
                        DeselectComponent();
                    }
                }
                else
                {
                    DeselectComponent();
                }
            }

            if (Input.GetKey(selectButton))
            {
                if (canDrag)
                {
                    var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out var hit, float.MaxValue, floorLayerMask))
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

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DeselectComponent();
            }
            
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                if(selectedComponent == null)
                    return;

                var component = selectedComponent;
                DeselectComponent();
                component.Delete();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                if (selectedComponent == null || !selectedComponent.CanRotate)
                    return;

                selectedComponent.transform.eulerAngles += new Vector3(0, 45f, 0);
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
            selectedPorts.Add(port);

            if (selectedPorts.Count == 2)
            {
                var jumperGameObject = Instantiate(jumperPrefab);
                var jumper = jumperGameObject.GetComponent<Jumper>();
                jumper.Generate(selectedPorts[0], selectedPorts[1]);

                selectedPorts.Clear();
                SelectComponent(jumper);
            }
        }

        public void OnPortUnselected(InoPort port)
        {
            selectedPorts.Remove(port);
        }

        #endregion

        #region Private Methods

        private IEnumerator WaitFrameEnd()
        {
            yield return new WaitForEndOfFrame();
            isAdding = false;
        }

        private void SelectComponent(InoComponent component)
        {
            isAdding = true;
            isDragging = false;
            selectedComponent?.DisableHighlight();
            selectedComponent = component;
            canDrag = selectedComponent.CanDrag;
            selectedComponent.EnableHighlight();
            StartCoroutine(WaitFrameEnd());
        }

        private void DeselectComponent()
        {
            canDrag = false;
            isDragging = false;
            selectedComponent?.DisableHighlight();
            selectedComponent = null;
        }

        #endregion
    }
}