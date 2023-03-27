using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Assets.Scripts.Camera;
using Assets.Scripts.Components;
using Assets.Scripts.Components.Base;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class ComponentsManager : MonoBehaviour
    {
        #region Properties

        public static ComponentsManager Instance { get; private set; }

        #endregion

        #region Fields

        public Vector3 DefaultIndicatorSize = new Vector3(0.04f, 0.04f, 0.04f);

        public bool HasUnsavedChanges;
        public string CurrentProjectName;
        public string CurrentProjectPath;

        [SerializeField] LayerMask inoLayerMask;
        [SerializeField] LayerMask floorLayerMask;
        [SerializeField] KeyCode selectButton;

        [SerializeField] GameObject jumperPrefab;

        private int inoComponentLayerId;
        private int floorLayerMaskId;

        private Transform currentParent;

        private InoComponent selectedComponent;
        private int selectedComponentLayer;

        private UnityEngine.Camera mainCamera;

        private bool isMouseReleased;

        private bool hasSelectedComponentChangedPosition;
        private bool canDrag;
        private bool isDragging;
        private bool isAddingComponent;
        private bool isAddingJumper;

        private Vector3 addingComponentStartPosition;
        private Vector3 dragStartPosition;

        private Material selectedMaterial;
        private Material unselectedMaterial;

        private List<InoComponent> sceneComponents;
        private List<InoPort> selectedPorts;

        private Dictionary<string, Dictionary<string, List<string>>> componentsCategories;
        private Dictionary<string, Texture> iconByName;
        private Dictionary<string, GameObject> prefabByName;

        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        public static extern bool SetWindowText(IntPtr hwnd, string lpString);
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string className, string windowName);

        private IntPtr currentWindow = IntPtr.Zero;

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
                "Protoboard400",
                "Led",
                "PushButton",
            });

            componentsCategories.Add("Arduino", new Dictionary<string, List<string>>());
            componentsCategories["Arduino"].Add("Boards", new List<string>
            {
                "ArduinoUno",
            });
            
            componentsCategories.Add("Ino3D", new Dictionary<string, List<string>>());
            componentsCategories["Ino3D"].Add("Custom", new List<string>
            {
                "BlackBox",
                "CarChassis"
            });
            
            componentsCategories.Add("Electric", new Dictionary<string, List<string>>());
            componentsCategories["Electric"].Add("Basics", new List<string>
            {
                "L298N",
                "Battery"
            });
            componentsCategories["Electric"].Add("Motors", new List<string>
            {
                "DCMotor"
            });

            iconByName = new Dictionary<string, Texture>();
            prefabByName = new Dictionary<string, GameObject>();

            foreach (var texture in Resources.LoadAll<Texture>("Icons"))
                iconByName.Add(texture.name, texture);

            foreach (var category in componentsCategories)
            {
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

            CurrentProjectPath = "";
            HasUnsavedChanges = false;
            CurrentProjectName = "Untitled";
            UpdateWindowTitle(false);
        }

        private void Start()
        {
            inoComponentLayerId = (int) Math.Log(inoLayerMask.value, 2);
            floorLayerMaskId = (int) Math.Log(floorLayerMask.value, 2);

            mainCamera = UnityEngine.Camera.main;
            selectedPorts = new List<InoPort>();
            UpdateSceneComponents();
        }

        private void Update()
        {
            if (!isAddingComponent && UIManager.Instance.IsMouserOverUI() || SimulationManager.Instance.IsSimulating())
                return;

            if (Input.GetKeyDown(selectButton))
            {
                var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, inoLayerMask))
                {
                    var component = hit.transform.GetComponent<InoComponent>();
                    if (component != null)
                        SelectComponent(component);
                    else if(!isAddingJumper)
                        DeselectComponent();
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
                    if (Physics.Raycast(ray, out var hit, float.MaxValue, selectedComponentLayer))
                    {
                        if (selectedComponent != null && !IsParent(hit.transform, selectedComponent.transform))
                        {
                            if (!isDragging)
                                dragStartPosition = hit.point;

                            currentParent = hit.transform;

                            var draggingOffset = hit.point - dragStartPosition;
                            if (draggingOffset.magnitude > 0)
                            {
                                hasSelectedComponentChangedPosition = true;
                                selectedComponent.transform.position = new Vector3(
                                    selectedComponent.transform.position.x + draggingOffset.x,
                                    dragStartPosition.y + selectedComponent.DefaultHeight,
                                    selectedComponent.transform.position.z + draggingOffset.z);

                                if (!HasUnsavedChanges)
                                    UpdateWindowTitle(true);
                                HasUnsavedChanges = true;
                            }

                            dragStartPosition = hit.point;
                            isDragging = true;
                        }
                    }
                    else
                    {
                        currentParent = null;
                    }
                }
            }

            if (Input.GetKeyUp(selectButton))
                isMouseReleased = true;

            if (isMouseReleased && UIManager.Instance.ImGuiIsMouseUp())
            {
                if (isAddingComponent && selectedComponent != null)
                {
                    if (UIManager.Instance.IsMouserOverUI())
                    {
                        var component = selectedComponent;
                        DeselectComponent();
                        component.Delete();
                    }
                }

                if (isDragging && selectedComponent != null)
                {
                    if (hasSelectedComponentChangedPosition)
                        selectedComponent.UpdatePinsConnection();

                    if (!(selectedComponent is Jumper))
                    {
                        selectedComponent.transform.parent = currentParent;
                        currentParent = null;
                    }
                }

                canDrag = false;
                isDragging = false;
                isAddingComponent = false;
                isMouseReleased = false;
                hasSelectedComponentChangedPosition = false;
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

        public void UpdateSceneComponents()
        {
            sceneComponents = FindObjectsOfType<InoComponent>().ToList();
        }

        public List<InoComponent> GetSceneComponents()
        {
            return sceneComponents;
        }

        public void NewProject()
        {
            DeselectComponent();
            foreach (var inoComponent in FindObjectsOfType<InoComponent>())
                inoComponent.Delete();

            CurrentProjectPath = "";
            HasUnsavedChanges = false;
            CurrentProjectName = "Untitled";
            UpdateWindowTitle(false);
            UpdateSceneComponents();
            CameraController.Instance.Reset();
        }

        public void SaveProject(string path)
        {
            var components = new HashSet<string>();
            var dependencyByComponent = new HashSet<Tuple<string, string>>();
            var componentByHash = new Dictionary<string, InoComponent>();

            foreach (var inoComponent in FindObjectsOfType<InoComponent>())
            {
                components.Add(inoComponent.Hash);
                componentByHash.Add(inoComponent.Hash, inoComponent);

                foreach (var dependency in inoComponent.GetDependencies())
                    dependencyByComponent.Add(Tuple.Create(inoComponent.Hash, dependency));
            }

            var saveProject = new InoProjectSaveFile {Components = new List<SaveFile>()};
            foreach (var hash in DependencySorter.Sort(components, dependencyByComponent))
            {
                var saveFile = componentByHash[hash].Save();
                saveFile.Name = componentByHash[hash].Name;
                saveFile.Hash = componentByHash[hash].Hash;

                var parentComponent = GetParentComponent(componentByHash[hash].transform);
                if (parentComponent != null)
                {
                    saveFile.ParentHash = parentComponent.Hash;
                    saveFile.ParentName = componentByHash[hash].transform.parent.name;
                }

                saveFile.PositionX = componentByHash[hash].transform.position.x;
                saveFile.PositionY = componentByHash[hash].transform.position.y;
                saveFile.PositionZ = componentByHash[hash].transform.position.z;

                saveFile.RotationX = componentByHash[hash].transform.eulerAngles.x;
                saveFile.RotationY = componentByHash[hash].transform.eulerAngles.y;
                saveFile.RotationZ = componentByHash[hash].transform.eulerAngles.z;

                saveProject.Components.Add(saveFile);
            }

            var json = JsonConvert.SerializeObject(saveProject, new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});

            var writer = new StreamWriter(path);
            writer.Write(Convert.ToBase64String(Encoding.UTF8.GetBytes(json)));
            writer.Close();

            CurrentProjectPath = path;
            HasUnsavedChanges = false;
            CurrentProjectName = Path.GetFileNameWithoutExtension(path);
            UpdateWindowTitle(false);
        }

        public IEnumerator LoadProject(string path)
        {
            NewProject();
            var reader = new StreamReader(path);
            var json = reader.ReadToEnd();
            reader.Close();

            if (IsBase64String(json))
                json = Encoding.UTF8.GetString(Convert.FromBase64String(json));

            var saveFile = JsonConvert.DeserializeObject<InoProjectSaveFile>(json,
                new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented});

            var components = new List<Tuple<InoComponent, SaveFile>>();
            var componentByHash = new Dictionary<string, InoComponent>();
            foreach (var componentSaveFile in saveFile.Components)
            {
                if (prefabByName.ContainsKey(componentSaveFile.PrefabName))
                {
                    var newComponent = Instantiate(prefabByName[componentSaveFile.PrefabName]).GetComponent<InoComponent>();
                    if (!string.IsNullOrEmpty(componentSaveFile.Hash))
                    {
                        newComponent.Hash = componentSaveFile.Hash;
                        componentByHash.Add(newComponent.Hash, newComponent);
                    }

                    if (!string.IsNullOrEmpty(componentSaveFile.Name))
                        newComponent.Name = componentSaveFile.Name;

                    components.Add(Tuple.Create(newComponent, componentSaveFile));
                }
            }

            yield return new WaitForEndOfFrame();

            foreach (var inoComponent in components)
            {
                inoComponent.Item1.Load(inoComponent.Item2);
                inoComponent.Item1.transform.position = new Vector3(inoComponent.Item2.PositionX, inoComponent.Item2.PositionY, inoComponent.Item2.PositionZ);
                inoComponent.Item1.transform.eulerAngles = new Vector3(inoComponent.Item2.RotationX, inoComponent.Item2.RotationY, inoComponent.Item2.RotationZ);
            }

            yield return new WaitForEndOfFrame();

            foreach (var inoComponent in components)
                inoComponent.Item1.UpdatePinsConnection();

            yield return new WaitForEndOfFrame();

            foreach (var componentSaveFile in saveFile.Components)
            {
                if (!prefabByName.ContainsKey(componentSaveFile.PrefabName) &&
                    componentSaveFile is JumperSaveFile jumperSaveFile)
                {
                    InoPort port1 = null;
                    var port1Position = new Vector3(jumperSaveFile.Port1PositionX,
                        jumperSaveFile.Port1PositionY + DefaultIndicatorSize.y,
                        jumperSaveFile.Port1PositionZ);
                    var port1Ray = new Ray(port1Position, Vector3.down);
                    if (Physics.Raycast(port1Ray, out var port1Hit))
                        port1 = port1Hit.transform.GetComponent<InoPort>();

                    InoPort port2 = null;
                    var port2Position = new Vector3(jumperSaveFile.Port2PositionX,
                        jumperSaveFile.Port2PositionY + DefaultIndicatorSize.y,
                        jumperSaveFile.Port2PositionZ);
                    var port2Ray = new Ray(port2Position, Vector3.down);
                    if (Physics.Raycast(port2Ray, out var port2Hit))
                        port2 = port2Hit.transform.GetComponent<InoPort>();

                    if (port1 != null && port2 != null)
                        StartCoroutine(CreateJumper(port1, port2, jumperSaveFile.CurrentColor, jumperSaveFile.IsRigid));
                }
            }

            yield return new WaitForEndOfFrame();

            foreach (var tuple in components)
            {
                var component = tuple.Item1;
                var save = tuple.Item2;
                if (!string.IsNullOrEmpty(save.ParentHash) && componentByHash.ContainsKey(save.ParentHash))
                {
                    var parent = componentByHash[save.ParentHash].transform.Find(save.ParentName);
                    if (parent != null)
                        component.transform.parent = parent;
                }
            }

            DeselectComponent();
            CurrentProjectPath = path;
            HasUnsavedChanges = false;
            CurrentProjectName = Path.GetFileNameWithoutExtension(path);
            UpdateWindowTitle(false);
            UpdateSceneComponents();
            CameraController.Instance.Reset();
        }

        public void InstantiateComponent(string componentName)
        {
            if (prefabByName.ContainsKey(componentName))
            {
                isAddingComponent = true;
                var newComponent = Instantiate(prefabByName[componentName]).GetComponent<InoComponent>();

                var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, float.MaxValue, floorLayerMask))
                {
                    addingComponentStartPosition =
                        new Vector3(hit.point.x, hit.point.x + newComponent.DefaultHeight, hit.point.z);
                    newComponent.transform.position = addingComponentStartPosition;
                }

                SelectComponent(newComponent);
                if (!HasUnsavedChanges)
                    UpdateWindowTitle(true);
                HasUnsavedChanges = true;
                UpdateSceneComponents();
            }
        }

        public InoComponent GetSelectedComponent()
        {
            return selectedComponent;
        }

        public Dictionary<string, Dictionary<string, List<string>>> GetComponentsCategories()
        {
            return componentsCategories;
        }

        public Texture GetIcon(string iconName)
        {
            return iconByName[iconName];
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
                StartCoroutine(CreateJumper(selectedPorts[0], selectedPorts[1], 0, false));
        }

        public void OnPortUnselected(InoPort port)
        {
            selectedPorts.Remove(port);
        }

        public void SelectComponent(InoComponent component)
        {
            isDragging = false;

            if (selectedComponent != null)
            {
                selectedComponent.DisableHighlight();
                selectedComponent.gameObject.layer = inoComponentLayerId;

                foreach (var child in selectedComponent.GetComponentsInChildren<Transform>(true))
                    if (child.gameObject.layer == floorLayerMaskId)
                        child.gameObject.GetComponent<BoxCollider>().enabled = true;
            }

            selectedComponent = component;
            canDrag = selectedComponent.CanDrag;
            selectedComponent.EnableHighlight();
            UIManager.Instance.SetSelectedComponentName(selectedComponent.Name);

            selectedComponent.gameObject.layer = 0;
            foreach (var child in selectedComponent.GetComponentsInChildren<Transform>(true))
                if (child.gameObject.layer == floorLayerMaskId)
                    child.gameObject.GetComponent<BoxCollider>().enabled = false;

            if (selectedComponent.IsAttachable())
                selectedComponentLayer = floorLayerMask | inoLayerMask;
            else
                selectedComponentLayer = floorLayerMask;
        }

        public void DeselectComponent()
        {
            canDrag = false;
            isDragging = false;

            if (selectedComponent != null)
            {
                foreach (var child in selectedComponent.GetComponentsInChildren<Transform>(true))
                    if (child.gameObject.layer == floorLayerMaskId)
                        child.gameObject.GetComponent<BoxCollider>().enabled = true;

                selectedComponent.DisableHighlight();
                selectedComponent.gameObject.layer = inoComponentLayerId;
            }

            selectedComponent = null;
        }
        
        public void DeselectPorts()
        {
            foreach (var selectedPort in selectedPorts)
                selectedPort.Deselect();
            selectedPorts.Clear();
        }

        #endregion

        #region Private Methods

        private bool IsBase64String(string s)
        {
            s = s.Trim();
            return s.Length % 4 == 0 && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
        }

        private InoComponent GetParentComponent(Transform baseTransform)
        {
            var tempParent = baseTransform.parent;
            while (tempParent != null)
            {
                var comp = tempParent.GetComponent<InoComponent>();
                if (comp != null)
                    return comp;
                tempParent = tempParent.parent;
            }

            return null;
        }

        private bool IsParent(Transform baseTransform, Transform possibleParent)
        {
            var tempParent = baseTransform.parent;
            while (tempParent != null)
            {
                if (tempParent == possibleParent)
                    return true;
                tempParent = tempParent.parent;
            }

            return false;
        }

        private IEnumerator CreateJumper(InoPort port1, InoPort port2, int color, bool isRigid)
        {
            if (!HasUnsavedChanges)
                UpdateWindowTitle(true);
            HasUnsavedChanges = true;
            isAddingJumper = true;
            var jumperGameObject = Instantiate(jumperPrefab);
            var jumper = jumperGameObject.GetComponent<Jumper>();
            jumper.Generate(port1, port2, color, isRigid);

            selectedPorts.Clear();
            SelectComponent(jumper);

            yield return new WaitForEndOfFrame();
            isAddingJumper = false;
            UpdateSceneComponents();
        }

        private void UpdateWindowTitle(bool addStar)
        {
            if (currentWindow == IntPtr.Zero)
                currentWindow = FindWindow(null, "INO3D");
            SetWindowText(currentWindow, "INO3D - " + CurrentProjectName + (addStar ? "*" : ""));
        }

        #endregion
    }
}