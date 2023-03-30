using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Camera;
using Assets.Scripts.Components.Base;
using Assets.Scripts.NodeEditor.Scripts;
using Assets.Scripts.Utils;
using Assets.Scripts.Windows;
using RuntimeNodeEditor;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Assets.Scripts.Components.Base.InoComponent;

namespace Assets.Scripts.Managers
{
    public class UIManager : MonoBehaviour
    {
        #region Properties

        public static UIManager Instance { get; private set; }

        public GameObject WarningPrefab;
        public GameObject GenericButtonPrefab;

        public TMP_Text ComponentsText;
        public TMP_Text PropertiesText;

        public TMP_Text SelectedCategoryText;
        public RectTransform CategoryScrollContent;
        public RectTransform ComponentsScrollContent;
        public RectTransform PropertiesScrollContent;

        public ScrollRect ComponentsScroll;

        public GameObject RightPanelGameObject;
        public GameObject PortOverlayGameObject;
        public TMP_Text PortOverlayText;

        public RectTransform Canvas;

        public Button FileButton;
        public Button OpenButton;
        public Button SaveButton;
        public Button Camera2DButton;
        public Button Camera3DButton;
        public Button ConsoleButton;
        public Button SettingsButton;
        public Button PlayButton;
        public Button StopButton;

        public GameObject InputButtonPrefab;
        public GameObject InputCheckBoxPrefab;
        public GameObject InputComboBoxPrefab;
        public GameObject InputNumberPrefab;
        public GameObject InputTextPrefab;
        public GameObject ConsoleWindowPrefab;
        public GameObject SettingsWindowPrefab;
        public GameObject CodeEditorWindowPrefab;
        public GameObject NodeEditorWindowPrefab;

        #endregion

        #region Fields

        private Dictionary<GameObject, GameObject> warningByObject;
        private ExtensionFilter[] extensions;
        private ConsoleWindow console;
        private SettingsWindow settings;
        private CodeEditorWindow codeEditor;
        private NodeEditorWindow nodeEditor;
        private Vector3 lastConsolePosition;
        private Vector3 lastSettingsPosition;
        private Vector3 lastCodeEditorPosition;
        private Vector3 lastNodeEditorPosition;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            else
                Instance = this;

            extensions = new[]
            {
                new ExtensionFilter(LocalizationManager.Instance.Localize("Ino3DProjectFiles"), "i3d"),
            };

            warningByObject = new Dictionary<GameObject, GameObject>();
        }

        private void Start()
        {
            SetupTopBar();
            SetupRightBar();
            UpdateButtonsStates();
        }

        private void Update()
        {
            if (console != null)
                lastConsolePosition = console.transform.localPosition;
            if (settings != null)
                lastSettingsPosition = settings.transform.localPosition;
            if (codeEditor != null)
                lastCodeEditorPosition = codeEditor.transform.localPosition;
            if (nodeEditor != null)
                lastNodeEditorPosition = nodeEditor.transform.localPosition;
        }

        #endregion

        #region Private Methods

        private void SetupTopBar()
        {
            FileButton.onClick.AddListener(() =>
            {
                var currentPopupAction = new Action(() => { ComponentsManager.Instance.NewProject(); });
                if (ComponentsManager.Instance.HasUnsavedChanges)
                    ShowUnsavedPopup(currentPopupAction);
                else
                    currentPopupAction();
            });
            OpenButton.onClick.AddListener(() =>
            {
                var currentPopupAction = new Action(() =>
                {
                    StandaloneFileBrowser.OpenFilePanelAsync(LocalizationManager.Instance.Localize("OpenProject"),
                        "",
                        extensions, false, paths =>
                        {
                            if (paths.Length > 0 && File.Exists(paths[0]))
                                StartCoroutine(ComponentsManager.Instance.LoadProject(paths[0]));
                        });
                });

                if (ComponentsManager.Instance.HasUnsavedChanges)
                    ShowUnsavedPopup(currentPopupAction);
                else
                    currentPopupAction();
            });
            SaveButton.onClick.AddListener(() =>
            {
                if (string.IsNullOrEmpty(ComponentsManager.Instance.CurrentProjectPath))
                {
                    StandaloneFileBrowser.SaveFilePanelAsync(LocalizationManager.Instance.Localize("SaveProject"),
                        "",
                        ComponentsManager.Instance.CurrentProjectName, extensions, path =>
                        {
                            if (!string.IsNullOrEmpty(path))
                                ComponentsManager.Instance.SaveProject(path);
                        });
                }
                else
                {
                    ComponentsManager.Instance.SaveProject(ComponentsManager.Instance.CurrentProjectPath);
                }
            });

            Camera2DButton.onClick.AddListener(() =>
            {
                CameraController.Instance.SetCameraAsOrthographic();
            });
            Camera3DButton.onClick.AddListener(() =>
            {
                CameraController.Instance.SetCameraAsPerspective();
            });

            ConsoleButton.onClick.AddListener(() =>
            {
                if (console == null)
                {
                    console = Instantiate(ConsoleWindowPrefab).GetComponent<ConsoleWindow>();
                    console.transform.parent = Canvas;
                    console.transform.localScale = Vector3.one;
                    console.transform.localPosition = lastConsolePosition;
                }
            });

            SettingsButton.onClick.AddListener(() =>
            {
                if (settings == null)
                {
                    settings = Instantiate(SettingsWindowPrefab).GetComponent<SettingsWindow>();
                    settings.transform.parent = Canvas;
                    settings.transform.localScale = Vector3.one;
                    settings.transform.localPosition = lastSettingsPosition;
                }
            });

            PlayButton.onClick.AddListener(() =>
            {
                SimulationManager.Instance.StartSimulation();
                UpdateButtonsStates();
            });
            StopButton.onClick.AddListener(() =>
            {
                SimulationManager.Instance.StopSimulation();

                foreach (var warning in warningByObject.Values)
                    Destroy(warning);
                warningByObject.Clear();
                UpdateButtonsStates();
            });
        }

        private void SetupRightBar()
        {
            ComponentsText.text = LocalizationManager.Instance.Localize("Menu.Components");
            PropertiesText.text = LocalizationManager.Instance.Localize("Menu.Properties");

            var first = false;
            var categories = ComponentsManager.Instance.GetComponentsCategories();
            foreach (var category in categories)
            {
                var button = Instantiate(GenericButtonPrefab);
                button.GetComponent<Button>().onClick.AddListener(() =>
                {
                    GenerateCategoryButtons(category.Key, category.Value);
                });
                button.transform.parent = CategoryScrollContent;
                button.transform.localScale = Vector3.one;
                button.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);

                var texture = ComponentsManager.Instance.GetIcon(category.Key);
                button.transform.GetChild(0).GetComponent<Image>().sprite = Sprite.Create((Texture2D)texture,
                    new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                if (!first)
                {
                    GenerateCategoryButtons(category.Key, category.Value);
                    first = true;
                }
            }
        }

        private void UpdateButtonsStates()
        {
            FileButton.interactable = !SimulationManager.Instance.IsSimulating();
            OpenButton.interactable = !SimulationManager.Instance.IsSimulating();
            SaveButton.interactable = !SimulationManager.Instance.IsSimulating();
            SettingsButton.interactable = !SimulationManager.Instance.IsSimulating();
            
            PlayButton.interactable = !SimulationManager.Instance.IsSimulating();
            StopButton.interactable = SimulationManager.Instance.IsSimulating();

            RightPanelGameObject.SetActive(!SimulationManager.Instance.IsSimulating());
            PortOverlayGameObject.SetActive(false);

            if (settings != null)
                settings.gameObject.SetActive(!SimulationManager.Instance.IsSimulating());
        }

        private void ShowUnsavedPopup(Action currentPopupAction)
        {
            var content = LocalizationManager.Instance.Localize("UnsavedPopupMessage") + $" \"{ComponentsManager.Instance.CurrentProjectName}\".";
            PopUpManager.Instance.Show(LocalizationManager.Instance.Localize("UnsavedPopupTitle"), content, () =>
            {
                if (string.IsNullOrEmpty(ComponentsManager.Instance.CurrentProjectPath))
                {
                    StandaloneFileBrowser.SaveFilePanelAsync(LocalizationManager.Instance.Localize("SaveProject"), "", "",
                        extensions, path =>
                        {
                            if (!string.IsNullOrEmpty(path))
                            {
                                ComponentsManager.Instance.SaveProject(path);
                                currentPopupAction();
                            }
                        });
                }
                else
                {
                    ComponentsManager.Instance.SaveProject(ComponentsManager.Instance.CurrentProjectPath);
                    currentPopupAction();
                }
            }, currentPopupAction, () =>
            {
            });
        }

        private void GenerateCategoryButtons(string category, Dictionary<string, List<string>> components)
        {
            for (var i = 0; i < ComponentsScrollContent.childCount; i++)
                Destroy(ComponentsScrollContent.GetChild(i).gameObject);

            SelectedCategoryText.text = LocalizationManager.Instance.Localize(category);
            foreach (var component in components.Values.SelectMany(c => c))
            {
                var button = Instantiate(GenericButtonPrefab);
                button.GetComponent<CustomButton>().OnMouseUp += () =>
                {
                    ComponentsScroll.enabled = true;
                };
                button.GetComponent<CustomButton>().OnMouseDown += () =>
                {
                    ComponentsScroll.enabled = false;
                    ComponentsManager.Instance.InstantiateComponent(component);
                };

                var texture = ComponentsManager.Instance.GetIcon(component);
                button.transform.GetChild(0).GetComponent<Image>().sprite = Sprite.Create((Texture2D) texture,
                    new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                button.transform.parent = ComponentsScrollContent;
                button.transform.localScale = Vector3.one;
                button.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 50);
            }
        }

        #endregion

        #region Public Methods

        public ConsoleWindow GetConsole()
        {
            return console;
        }

        public bool IsMouserOverUI()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }

        #region Windows

        public void ShowWarning(GameObject obj, Vector3 position)
        {
            var show = LocalizationManager.Instance.GetShowWarnings();
            if (!show || !SimulationManager.Instance.IsSimulating() || warningByObject.ContainsKey(obj))
            {
                if (!show && warningByObject.ContainsKey(obj))
                    HideWarning(obj);
                return;
            }

            var warning = Instantiate(WarningPrefab);
            warning.transform.parent = obj.transform;
            warning.transform.localPosition = position;

            warningByObject.Add(obj, warning);
        }

        public void HideWarning(GameObject obj)
        {
            if (!warningByObject.ContainsKey(obj))
                return;

            Destroy(warningByObject[obj]);
            warningByObject.Remove(obj);
        }

        public void DisplayPortOverlay(string portName, PortType portType, PinType pinType)
        {
            if(SimulationManager.Instance.IsSimulating() || ComponentsManager.Instance.IsAddingComponent())
                return;

            PortOverlayText.text = LocalizationManager.Instance.Localize("Overlay.Port") + ": " + portName;
            if (portType != PortType.None)
            {
                var type = string.Empty;
                switch (portType)
                {
                    case PortType.Analog:
                        type = LocalizationManager.Instance.Localize("Overlay.Type.Analog");
                        break;
                    case PortType.Digital:
                        type = LocalizationManager.Instance.Localize("Overlay.Type.Digital");
                        break;
                    case PortType.DigitalPwm:
                        type = LocalizationManager.Instance.Localize("Overlay.Type.DigitalPwm");
                        break;
                    case PortType.Power:
                        type = LocalizationManager.Instance.Localize("Overlay.Type.Power");
                        break;
                }

                PortOverlayText.text += "\n" + LocalizationManager.Instance.Localize("Overlay.Type") + ": " + type;
            }

            PortOverlayGameObject.GetComponent<RectTransform>().position = Input.mousePosition;
            PortOverlayGameObject.SetActive(true);
        }

        public void HidePortOverlay()
        {
            PortOverlayGameObject.SetActive(false);
        }

        public void ShowError(string message)
        {
            Debug.LogError(message);
            //UpdateButtonsStates();
        }

        public void ShowEditCode(string code, Action<string> onSave, InoComponent component)
        {
            if (codeEditor == null)
            {
                codeEditor = Instantiate(CodeEditorWindowPrefab).GetComponent<CodeEditorWindow>();
                codeEditor.OnSave = onSave;
                codeEditor.InitialCode = code;
                codeEditor.Component = component;
                codeEditor.transform.parent = Canvas;
                codeEditor.transform.localScale = Vector3.one;
                codeEditor.transform.localPosition = lastCodeEditorPosition;
            }
        }

        public void ShowNodeEditor(string graph, Action<string, string, string> onSave, InoComponent component)
        {
            if (nodeEditor == null)
            {
                nodeEditor = Instantiate(NodeEditorWindowPrefab).GetComponent<NodeEditorWindow>();
                nodeEditor.OnSave = onSave;
                nodeEditor.InitialCode = graph;
                nodeEditor.Component = component;
                nodeEditor.transform.parent = Canvas;
                nodeEditor.transform.localScale = Vector3.one;
                nodeEditor.transform.localPosition = lastNodeEditorPosition;
            }
        }

        #endregion

        #region Properties

        public void SelectComponent(InoComponent inoComponent)
        {
            for (var i = 0; i < PropertiesScrollContent.childCount; i++)
                Destroy(PropertiesScrollContent.GetChild(i).gameObject);

            if (inoComponent != null)
            {
                GenerateStringPropertyField(LocalizationManager.Instance.Localize("Name"), inoComponent.Name, value => inoComponent.Name = value);
                inoComponent.DrawPropertiesWindow();
            }
        }

        public void GenerateCheckboxPropertyField(string label, bool value, Action<bool> onChangeValue)
        {
            var prefab = Instantiate(InputCheckBoxPrefab);
            prefab.transform.parent = PropertiesScrollContent;
            prefab.transform.localScale = Vector3.one;

            prefab.transform.GetChild(0).GetComponent<TMP_Text>().text = label;
            var toggle = prefab.transform.GetChild(1).GetComponent<Toggle>();
            toggle.isOn = value;
            toggle.onValueChanged.AddListener(v => onChangeValue(v));
        }

        public void GenerateButtonPropertyField(string label, Action onChangeValue)
        {
            var prefab = Instantiate(InputButtonPrefab);
            prefab.transform.parent = PropertiesScrollContent;
            prefab.transform.localScale = Vector3.one;

            var button = prefab.transform.GetChild(0).GetComponent<Button>();
            button.onClick.AddListener(() => onChangeValue());
            button.GetComponentInChildren<TMP_Text>().text = label;
        }

        public void GenerateComboBoxPropertyField(string label, string[] items, int selected, Action<int> onChangeValue)
        {
            var prefab = Instantiate(InputComboBoxPrefab);
            prefab.transform.parent = PropertiesScrollContent;
            prefab.transform.localScale = Vector3.one;

            prefab.transform.GetChild(0).GetComponent<TMP_Text>().text = label;

            var comboBox = prefab.transform.GetChild(1).GetComponent<TMP_Dropdown>();
            comboBox.AddOptions(items.Select(s => new TMP_Dropdown.OptionData(s)).ToList());
            comboBox.value = selected;
            comboBox.onValueChanged.AddListener(v => onChangeValue(v));
        }

        public void GenerateIntPropertyField(string label, int value, Action<int> onChangeValue)
        {
            var prefab = Instantiate(InputNumberPrefab);
            prefab.transform.parent = PropertiesScrollContent;
            prefab.transform.localScale = Vector3.one;

            prefab.transform.GetChild(0).GetComponent<TMP_Text>().text = label;

            var inputText = prefab.transform.GetChild(1).GetComponent<TMP_InputField>();
            inputText.text = value.ToString();
            inputText.onEndEdit.AddListener(v => onChangeValue(int.Parse(v)));
        }

        public void GenerateStringPropertyField(string label, string value, Action<string> onChangeValue)
        {
            var prefab = Instantiate(InputTextPrefab);
            prefab.transform.parent = PropertiesScrollContent;
            prefab.transform.localScale = Vector3.one;

            prefab.transform.GetChild(0).GetComponent<TMP_Text>().text = label;

            var inputText = prefab.transform.GetChild(1).GetComponent<TMP_InputField>();
            inputText.text = value;
            inputText.onEndEdit.AddListener(v => onChangeValue(v));
        }

        #endregion

        #endregion
    }
}