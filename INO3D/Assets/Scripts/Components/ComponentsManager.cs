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

        private Dictionary<string, Dictionary<string, List<string>>> componentsCategories;
        private Dictionary<string, Texture> iconByName;
        private Dictionary<string, GameObject> prefabByName;

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

            componentsCategories = new Dictionary<string, Dictionary<string, List<string>>>();

            componentsCategories.Add("Circuit", new Dictionary<string, List<string>>());
            componentsCategories["Circuit"].Add("Basics", new List<string>
            {
                "Resistor",
                "Protoboard400"
            });

            componentsCategories.Add("Arduino", new Dictionary<string, List<string>>());
            componentsCategories["Arduino"].Add("Boards", new List<string>
            {
                "ArduinoUno"
            });

            iconByName = new Dictionary<string, Texture>();
            prefabByName = new Dictionary<string, GameObject>();

            foreach (var category in componentsCategories)
            {
                iconByName.Add(category.Key, Resources.Load<Texture>("Icons/" + category.Key));

                foreach (var componentsList in category.Value.Values)
                {
                    foreach (var componentName in componentsList)
                    {
                        var path = "Components/" + componentName;
                        foreach (var o in Resources.LoadAll(path))
                        {
                            if (o is Texture icon)
                                iconByName.Add(componentName, icon);
                            else if (o is GameObject prefab)
                                prefabByName.Add(componentName, prefab);
                        }
                    }
                }
            }
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

                            var draggingOffset = hit.point - dragStartPosition;
                            selectedComponent.transform.position = new Vector3(
                                selectedComponent.transform.position.x + draggingOffset.x,
                                dragStartPosition.y + selectedComponent.DefaultHeight,
                                selectedComponent.transform.position.z + draggingOffset.z);
                            dragStartPosition = hit.point;
                            isDragging = true;
                        }
                }
            }

            if (Input.GetKeyUp(selectButton))
            {
                if (isDragging && selectedComponent != null)
                    selectedComponent.UpdatePinsConnection();
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
                selectedComponent.UpdatePinsConnection();
            }
        }

        #endregion

        #region Public Methods

        public Dictionary<string, Dictionary<string, List<string>>> GetComponentsCategories()
        {
            return componentsCategories;
        }

        public Texture GetIcon(string componentName)
        {
            return iconByName[componentName];
        }

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