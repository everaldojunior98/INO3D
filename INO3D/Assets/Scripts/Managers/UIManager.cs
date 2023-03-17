using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Assets.Scripts.Camera;
using Assets.Scripts.Components;
using ImGuiNET;
using SFB;
using UnityEngine;
using static Assets.Scripts.Components.Base.InoComponent;

namespace Assets.Scripts.Managers
{
    public class UIManager : MonoBehaviour
    {
        #region Properties

        public static UIManager Instance { get; private set; }

        public GameObject WarningPrefab;

        #endregion

        #region Fields

        #region Consts

        private const ImGuiDockNodeFlags DockspaceFlags =
            ImGuiDockNodeFlags.PassthruCentralNode | ImGuiDockNodeFlags.NoDockingInCentralNode;

        private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoDocking |
                                                     ImGuiWindowFlags.NoBackground |
                                                     ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse |
                                                     ImGuiWindowFlags.NoResize |
                                                     ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBringToFrontOnFocus |
                                                     ImGuiWindowFlags.NoNavFocus;

        private const ImGuiWindowFlags ButtonBarFlags = ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoDecoration |
                                           ImGuiWindowFlags.NoResize;

        private const ImGuiWindowFlags PortHoverWindowFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize |
                                             ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing |
                                             ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoMove;

        private const float PortHoverPaddingX = 10;
        private const float PortHoverPaddingY = 5;

        private const float ButtonBarHeight = 40;
        private readonly Vector2 buttonBarButtonSize = new Vector2(20, 20);
        private readonly Vector2 defaultButtonSize = new Vector2(50, 50);

        #endregion

        private ImGuiStylePtr style;
        private bool setupDearImGui = true;

        private bool isMouseOverUI;
        private bool isMouseUp;

        private bool displayPortOverlay;
        private bool showLockCamera;
        private bool showConsole;
        private bool showSettings;
        private bool showEditCode;

        private string overlayPortName;
        private PortType overlayPortType;
        private PinType overlayPinType;

        private float componentVoltage;
        private float componentCurrent;

        private string selectedCategory;

        private bool consoleAutoScroll;
        private byte[] searchInputBuffer;
        private byte[] consoleInputBuffer;
        private byte[] selectedComponentNameBuffer;

        private string currentLog;
        private int currentLineEnding = 1;
        private int selectedLanguage = -1;
        private float cameraSensitivity = 1;
        private bool showWarnings = true;

        private Action currentPopupAction = () => { };
        private Action<string> onCodeSave;
        private string currentCode = string.Empty;

        private Dictionary<GameObject, GameObject> warningByObject;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            else
                Instance = this;

            consoleAutoScroll = true;
            consoleInputBuffer = new byte[1024];
            selectedComponentNameBuffer = new byte[1024];

            currentLog = string.Empty;
            warningByObject = new Dictionary<GameObject, GameObject>();
            LoadSettings();
        }

        private void OnEnable()
        {
            ImGuiUn.Layout += OnLayout;
        }

        private void OnDisable()
        {
            ImGuiUn.Layout -= OnLayout;
        }

        #endregion

        #region Private Methods

        private void LoadSettings()
        {
            selectedLanguage = LocalizationManager.Instance.GetCurrentLanguage();
            cameraSensitivity = LocalizationManager.Instance.GetCameraSensitivity();
            showWarnings = LocalizationManager.Instance.GetShowWarnings();
        }

        #endregion

        #region Public Methods

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

        public bool IsMouserOverUI()
        {
            return !displayPortOverlay && isMouseOverUI;
        }

        public bool ImGuiIsMouseUp()
        {
            return isMouseUp;
        }

        public void DisplayPortOverlay(string portName, PortType portType, PinType pinType)
        {
            displayPortOverlay = true;

            overlayPortName = portName;
            overlayPortType = portType;
            overlayPinType = pinType;
        }

        public void HidePortOverlay()
        {
            displayPortOverlay = false;
        }

        public void ShowLockCamera()
        {
            searchInputBuffer = new byte[1024];
            showLockCamera = !showLockCamera;
        }
        
        public void ShowConsole()
        {
            showConsole = !showConsole;
        }

        public void ShowSettings()
        {
            showSettings = !showSettings;
            LoadSettings();
        }

        public void ShowEditCode(string code, Action<string> onSave)
        {
            currentCode = code;
            onCodeSave = onSave;
            showEditCode = !showEditCode;
        } 
        
        public void AddLog(string log, int lineEnding = 1)
        {
            if (log.Length > 0)
            {
                log = Regex.Unescape(log.Split('\0').First());
                currentLog += log + (lineEnding == 1 ? "\n" : "");
            }
        }

        public void GenerateStringPropertyField(string label, byte[] stringBuffer)
        {
            ImGui.Columns(2);
            var cursorPosition = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new Vector2(cursorPosition.x, cursorPosition.y + ImGui.GetFontSize() / 2));
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGui.InputText(label, stringBuffer, (uint)stringBuffer.Length);
            ImGui.NextColumn();
            ImGui.Columns(1);
        }

        public void GenerateCheckboxPropertyField(string label, ref bool value)
        {
            ImGui.Columns(2);
            var cursorPosition = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new Vector2(cursorPosition.x, cursorPosition.y + ImGui.GetFontSize() / 2));
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGui.Checkbox("###" + label, ref value);
            ImGui.NextColumn();
            ImGui.Columns(1);
        }

        public void GenerateIntPropertyField(string label, ref int value)
        {
            ImGui.Columns(2);
            var cursorPosition = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new Vector2(cursorPosition.x, cursorPosition.y + ImGui.GetFontSize() / 2));
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGui.InputInt(label, ref value, 10, 100);
            ImGui.NextColumn();
            ImGui.Columns(1);
        }

        public void GenerateComboBoxPropertyField(string label, ref int selectedIndex, string[] items)
        {
            ImGui.Columns(2);
            var cursorPosition = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new Vector2(cursorPosition.x, cursorPosition.y + ImGui.GetFontSize() / 2));
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGui.Combo(label, ref selectedIndex, items, items.Length);
            ImGui.NextColumn();
            ImGui.Columns(1);
        }

        public void GenerateButtonPropertyField(string label, Action onClick)
        {
            ImGui.SetNextItemWidth(-1);
            var buttonSize = new Vector2(-1, 30);
            if (ImGui.Button(label, buttonSize))
                onClick();
        }

        public void GenerateSeparator()
        {
            ImGui.Separator();
        }

        public void SetSelectedComponentName(string componentName)
        {
            selectedComponentNameBuffer = new byte[1024];
            var nameBuffer = Encoding.UTF8.GetBytes(componentName);
            nameBuffer.CopyTo(selectedComponentNameBuffer, 0);
        }

        #endregion

        #region Layouts

        private void ApplyTheme()
        {
            style = ImGui.GetStyle();
            var colors = style.Colors;

            colors[(int) ImGuiCol.Text] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
            colors[(int) ImGuiCol.TextDisabled] = new Vector4(0.40f, 0.40f, 0.40f, 1.00f);
            colors[(int) ImGuiCol.ChildBg] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
            colors[(int) ImGuiCol.WindowBg] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
            colors[(int) ImGuiCol.PopupBg] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
            colors[(int) ImGuiCol.Border] = new Vector4(0.12f, 0.12f, 0.12f, 0.71f);
            colors[(int) ImGuiCol.BorderShadow] = new Vector4(1.00f, 1.00f, 1.00f, 0.06f);
            colors[(int) ImGuiCol.FrameBg] = new Vector4(0.42f, 0.42f, 0.42f, 0.54f);
            colors[(int) ImGuiCol.FrameBgHovered] = new Vector4(0.42f, 0.42f, 0.42f, 0.40f);
            colors[(int) ImGuiCol.FrameBgActive] = new Vector4(0.56f, 0.56f, 0.56f, 0.67f);
            colors[(int) ImGuiCol.TitleBg] = new Vector4(0.19f, 0.19f, 0.19f, 1.00f);
            colors[(int) ImGuiCol.TitleBgActive] = new Vector4(0.19f, 0.19f, 0.19f, 1.00f);
            colors[(int) ImGuiCol.TitleBgCollapsed] = new Vector4(0.17f, 0.17f, 0.17f, 0.90f);
            colors[(int) ImGuiCol.MenuBarBg] = new Vector4(0.335f, 0.335f, 0.335f, 1.000f);
            colors[(int) ImGuiCol.ScrollbarBg] = new Vector4(0.24f, 0.24f, 0.24f, 0.53f);
            colors[(int) ImGuiCol.ScrollbarGrab] = new Vector4(0.41f, 0.41f, 0.41f, 1.00f);
            colors[(int) ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.52f, 0.52f, 0.52f, 1.00f);
            colors[(int) ImGuiCol.ScrollbarGrabActive] = new Vector4(0.76f, 0.76f, 0.76f, 1.00f);
            colors[(int) ImGuiCol.CheckMark] = new Vector4(0.65f, 0.65f, 0.65f, 1.00f);
            colors[(int) ImGuiCol.SliderGrab] = new Vector4(0.52f, 0.52f, 0.52f, 1.00f);
            colors[(int) ImGuiCol.SliderGrabActive] = new Vector4(0.64f, 0.64f, 0.64f, 1.00f);
            colors[(int) ImGuiCol.Button] = new Vector4(0.54f, 0.54f, 0.54f, 0.35f);
            colors[(int) ImGuiCol.ButtonHovered] = new Vector4(0.52f, 0.52f, 0.52f, 0.59f);
            colors[(int) ImGuiCol.ButtonActive] = new Vector4(0.76f, 0.76f, 0.76f, 1.00f);
            colors[(int) ImGuiCol.Header] = new Vector4(0.38f, 0.38f, 0.38f, 1.00f);
            colors[(int) ImGuiCol.HeaderHovered] = new Vector4(0.47f, 0.47f, 0.47f, 1.00f);
            colors[(int) ImGuiCol.HeaderActive] = new Vector4(0.76f, 0.76f, 0.76f, 0.77f);
            colors[(int) ImGuiCol.Separator] = new Vector4(0.000f, 0.000f, 0.000f, 0.137f);
            colors[(int) ImGuiCol.SeparatorHovered] = new Vector4(0.700f, 0.671f, 0.600f, 0.290f);
            colors[(int) ImGuiCol.SeparatorActive] = new Vector4(0.702f, 0.671f, 0.600f, 0.674f);
            colors[(int) ImGuiCol.ResizeGrip] = new Vector4(0.26f, 0.59f, 0.98f, 0.25f);
            colors[(int) ImGuiCol.ResizeGripHovered] = new Vector4(0.26f, 0.59f, 0.98f, 0.67f);
            colors[(int) ImGuiCol.ResizeGripActive] = new Vector4(0.26f, 0.59f, 0.98f, 0.95f);
            colors[(int) ImGuiCol.PlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
            colors[(int) ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
            colors[(int) ImGuiCol.PlotHistogram] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
            colors[(int) ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f);
            colors[(int) ImGuiCol.TextSelectedBg] = new Vector4(0.73f, 0.73f, 0.73f, 0.35f);
            colors[(int) ImGuiCol.ModalWindowDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.35f);
            colors[(int) ImGuiCol.DragDropTarget] = new Vector4(1.00f, 1.00f, 0.00f, 0.90f);
            colors[(int) ImGuiCol.NavHighlight] = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);
            colors[(int) ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
            colors[(int) ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
            colors[(int) ImGuiCol.ResizeGrip] = new Vector4(0.533f, 0.078f, 0.078f, 1f);
            colors[(int) ImGuiCol.ResizeGripActive] = new Vector4(0.894f, 0.133f, 0.149f, 1f);
            colors[(int) ImGuiCol.ResizeGripHovered] = new Vector4(0.894f, 0.133f, 0.149f, 1f);
            colors[(int) ImGuiCol.NavHighlight] = new Vector4(0.894f, 0.133f, 0.149f, 1f);

            style.PopupRounding = 3;

            style.WindowPadding = new Vector2(5, 5);
            style.FramePadding = new Vector2(5, 5);
            style.ItemSpacing = new Vector2(5, 5);

            style.ScrollbarSize = 18;

            style.WindowBorderSize = 1;
            style.ChildBorderSize = 1;
            style.PopupBorderSize = 1;
            style.FrameBorderSize = 0;

            style.WindowRounding = 3;
            style.ChildRounding = 3;
            style.FrameRounding = 3;
            style.ScrollbarRounding = 2;
            style.GrabRounding = 3;

            style.TabRounding = 3;
            colors[(int) ImGuiCol.Tab] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
            colors[(int) ImGuiCol.TabHovered] = new Vector4(0.40f, 0.40f, 0.40f, 1.00f);
            colors[(int) ImGuiCol.TabActive] = new Vector4(0.33f, 0.33f, 0.33f, 1.00f);
            colors[(int) ImGuiCol.TabUnfocused] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
            colors[(int) ImGuiCol.TabUnfocusedActive] = new Vector4(0.33f, 0.33f, 0.33f, 1.00f);
        }

        private void OnLayout()
        {
            if (setupDearImGui)
            {
                ImGui.CaptureMouseFromApp(false);
                ApplyTheme();

                setupDearImGui = false;
            }

            unsafe
            {
                ImGui.GetIO().NativePtr->Framerate = 30;
            }

            var viewport = ImGui.GetMainViewport();
            ImGui.SetNextWindowPos(new Vector2(0, ButtonBarHeight));
            ImGui.SetNextWindowSize(viewport.Size);
            ImGui.SetNextWindowViewport(viewport.ID);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
            ImGui.Begin("MainDockSpace", WindowFlags);
            ImGui.PopStyleVar();
            ImGui.PopStyleVar(2);

            var dockSpaceId = ImGui.GetID("MainDockSpace");
            ImGui.DockSpace(dockSpaceId, new Vector2(0.0f, 0.0f), DockspaceFlags);
            ImGui.End();

            isMouseOverUI = ImGui.GetIO().WantCaptureMouse;
            isMouseUp = !ImGui.IsAnyMouseDown();

            ShowButtonBar();

            if (!SimulationManager.Instance.IsSimulating())
            {
                ShowComponentsWindow();
                ShowPropertiesWindow();
            }

            if (showLockCamera)
                ShowLockCameraWindow();
            
            if (showConsole)
                ShowConsoleWindow();
            
            if (showSettings)
                ShowSettingsWindow();

            if(showEditCode && !SimulationManager.Instance.IsSimulating())
                ShowEditCodeWindow();

            if (displayPortOverlay)
                ShowPortOverlay();
        }

        private void ShowComponentsWindow()
        {
            if (ImGui.Begin(LocalizationManager.Instance.Localize("Menu.Components") + "###components",
                    ImGuiWindowFlags.NoCollapse))
            {
                var categories = ComponentsManager.Instance.GetComponentsCategories();
                // Left
                ImGui.BeginChild("left panel", new Vector2(40, 0), false, ImGuiWindowFlags.NoScrollbar);
                foreach (var category in categories)
                {
                    if (string.IsNullOrEmpty(selectedCategory))
                        selectedCategory = category.Key;

                    var isSelected = selectedCategory == category.Key;

                    if (!isSelected)
                    {
                        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, style.Alpha * 0.5f);
                        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, style.Colors[(int) ImGuiCol.Button]);
                        ImGui.PushStyleColor(ImGuiCol.ButtonActive, style.Colors[(int) ImGuiCol.Button]);
                    }

                    if (ImGui.ImageButton(
                            (IntPtr) ImGuiUn.GetTextureId(
                                ComponentsManager.Instance.GetIcon(category.Key)), new Vector2(30, 30)))
                    {
                        selectedCategory = category.Key;
                    }

                    if (!isSelected)
                    {
                        ImGui.PopStyleVar();
                        ImGui.PopStyleColor();
                        ImGui.PopStyleColor();
                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                        ImGui.TextUnformatted(LocalizationManager.Instance.Localize(category.Key));
                        ImGui.PopTextWrapPos();
                        ImGui.EndTooltip();
                    }
                }

                ImGui.EndChild();

                ImGui.SameLine();

                // Right
                ImGui.BeginGroup();
                ImGui.BeginChild("item view");

                if (categories.ContainsKey(selectedCategory))
                {
                    ImGui.Text(LocalizationManager.Instance.Localize(selectedCategory));
                    ImGui.Separator();
                    foreach (var subCategory in categories[selectedCategory])
                    {
                        if (ImGui.CollapsingHeader(
                                LocalizationManager.Instance.Localize(selectedCategory + "." + subCategory.Key),
                                ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            var visibleSize = ImGui.GetWindowPos().x + ImGui.GetWindowContentRegionMax().x;
                            var componentsCount = subCategory.Value.Count;
                            var currentCount = 0;

                            foreach (var componentName in subCategory.Value)
                            {
                                var disabled = SimulationManager.Instance.IsSimulating();
                                if (disabled)
                                {
                                    ImGui.PushStyleVar(ImGuiStyleVar.Alpha, style.Alpha * 0.5f);
                                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, style.Colors[(int) ImGuiCol.Button]);
                                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, style.Colors[(int) ImGuiCol.Button]);
                                }

                                ImGui.ImageButton(
                                    (IntPtr) ImGuiUn.GetTextureId(ComponentsManager.Instance.GetIcon(componentName)),
                                    defaultButtonSize);
                                if (!disabled && ImGui.IsItemClicked(ImGuiMouseButton.Left))
                                {
                                    ComponentsManager.Instance.InstantiateComponent(componentName);
                                }

                                if (!disabled && ImGui.IsItemHovered())
                                {
                                    ImGui.BeginTooltip();
                                    ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                                    ImGui.TextUnformatted(LocalizationManager.Instance.Localize(componentName));
                                    ImGui.PopTextWrapPos();
                                    ImGui.EndTooltip();
                                }

                                if (disabled)
                                {
                                    ImGui.PopStyleVar();
                                    ImGui.PopStyleColor();
                                    ImGui.PopStyleColor();
                                }

                                var lastButtonSize = ImGui.GetItemRectMax().x;
                                var nextButtonSize = lastButtonSize + style.ItemSpacing.x + defaultButtonSize.x;
                                if (currentCount + 1 < componentsCount && nextButtonSize < visibleSize)
                                    ImGui.SameLine();
                                currentCount++;
                            }
                        }
                    }
                }

                ImGui.EndChild();
                ImGui.EndGroup();
            }

            ImGui.End();
        }

        private void ShowPropertiesWindow()
        {
            if (ImGui.Begin(LocalizationManager.Instance.Localize("Menu.Properties") + "###properties", ImGuiWindowFlags.NoCollapse))
            {
                var selectedComponent = ComponentsManager.Instance.GetSelectedComponent();
                if (selectedComponent != null)
                {
                    GenerateStringPropertyField(LocalizationManager.Instance.Localize("Name"), selectedComponentNameBuffer);
                    selectedComponent.Name = Encoding.UTF8.GetString(selectedComponentNameBuffer).Trim('\0');
                    selectedComponent.DrawPropertiesWindow();
                }
            }

            ImGui.End();
        }

        private void ShowPortOverlay()
        {
            ImGui.SetNextWindowPos(new Vector2(Input.mousePosition.x + PortHoverPaddingX, Screen.height - Input.mousePosition.y - PortHoverPaddingY));
            if (ImGui.Begin("PortOverlay", PortHoverWindowFlags))
            {
                ImGui.Text(LocalizationManager.Instance.Localize("Overlay.Port") + ": " + overlayPortName);

                if (overlayPortType != PortType.None)
                {
                    var type = string.Empty;
                    switch (overlayPortType)
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

                    ImGui.Separator();
                    ImGui.Text(LocalizationManager.Instance.Localize("Overlay.Type") + ": " + type);
                }
            }

            ImGui.End();
        }

        private void ShowButtonBar()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            var padding = new Vector2(5, 5);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, padding);

            var viewport = ImGui.GetMainViewport();
            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.SetNextWindowSize(new Vector2(viewport.Size.x, ButtonBarHeight));
            ImGui.Begin("ButtonBar", ButtonBarFlags);

            var extensions = new[]
            {
                new ExtensionFilter(LocalizationManager.Instance.Localize("Ino3DProjectFiles"), "i3d"),
            };

            DrawInLineButton("File", !SimulationManager.Instance.IsSimulating(),
                LocalizationManager.Instance.Localize("NewProject"), () =>
                {
                    currentPopupAction = () => { ComponentsManager.Instance.NewProject(); };
                    if (ComponentsManager.Instance.HasUnsavedChanges)
                        ImGui.OpenPopup(LocalizationManager.Instance.Localize("UnsavedPopupTitle"));
                    else
                        currentPopupAction();
                });
            DrawInLineButton("Folder", !SimulationManager.Instance.IsSimulating(),
                LocalizationManager.Instance.Localize("OpenProject"), () =>
                {
                    currentPopupAction = () =>
                    {
                        StandaloneFileBrowser.OpenFilePanelAsync(LocalizationManager.Instance.Localize("OpenProject"),
                            "",
                            extensions, false, paths =>
                            {
                                if (paths.Length > 0 && File.Exists(paths[0]))
                                    StartCoroutine(ComponentsManager.Instance.LoadProject(paths[0]));
                            });
                    };

                    if (ComponentsManager.Instance.HasUnsavedChanges)
                        ImGui.OpenPopup(LocalizationManager.Instance.Localize("UnsavedPopupTitle"));
                    else
                        currentPopupAction();
                });
            DrawInLineButton("Save", !SimulationManager.Instance.IsSimulating(),
                LocalizationManager.Instance.Localize("SaveProject"), () =>
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

            InLineSpacing();

            DrawInLineButton("2D", true, LocalizationManager.Instance.Localize("Camera2D"),
                () => CameraController.Instance.SetCameraAsOrthographic());
            DrawInLineButton("3D", true, LocalizationManager.Instance.Localize("Camera3D"),
                () => CameraController.Instance.SetCameraAsPerspective());
            DrawInLineButton("LockCamera", true, LocalizationManager.Instance.Localize("LockCamera"),
                () => ShowLockCamera());

            InLineSpacing();

            DrawInLineButton("Console", true, LocalizationManager.Instance.Localize("OpenConsole"),
                () => ShowConsole());

            InLineSpacing();

            DrawInLineButton("Settings", true, LocalizationManager.Instance.Localize("OpenSettings"),
                () => ShowSettings());

            var menuBarSize = ImGui.GetWindowSize();
            var middleBarPosition = menuBarSize.x / 2;
            var playPosition = middleBarPosition - buttonBarButtonSize.x + padding.x / 2;
            var stopPosition = middleBarPosition + buttonBarButtonSize.x - padding.x / 2;

            ImGui.SetCursorPos(new Vector2(playPosition, padding.y));

            DrawButton("Play", !SimulationManager.Instance.IsSimulating(),
                LocalizationManager.Instance.Localize("StartSimulation"),
                () => SimulationManager.Instance.StartSimulation());
            ImGui.SetCursorPos(new Vector2(stopPosition, padding.y));
            DrawButton("Stop", SimulationManager.Instance.IsSimulating(),
                LocalizationManager.Instance.Localize("StopSimulation"),
                () =>
                {
                    SimulationManager.Instance.StopSimulation();

                    foreach (var warning in warningByObject.Values)
                        Destroy(warning);
                    warningByObject.Clear();
                });

            var center = ImGui.GetMainViewport().Size / 2;
            ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
            var pOpen = true;
            if (ImGui.BeginPopupModal(LocalizationManager.Instance.Localize("UnsavedPopupTitle"), ref pOpen, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize))
            {
                ImGui.Text(LocalizationManager.Instance.Localize("UnsavedPopupMessage") +
                           $" \"{ComponentsManager.Instance.CurrentProjectName}\".");
                ImGui.Separator();

                if (ImGui.Button(LocalizationManager.Instance.Localize("Yes"), new Vector2(65, 0)))
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
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine();
                if (ImGui.Button(LocalizationManager.Instance.Localize("No"), new Vector2(65, 0)))
                {
                    currentPopupAction();
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SetItemDefaultFocus();
                ImGui.SameLine();
                if (ImGui.Button(LocalizationManager.Instance.Localize("Cancel"), new Vector2(65, 0)))
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }

            ImGui.End();
            ImGui.PopStyleVar();
            ImGui.PopStyleVar(2);
        }

        private void DrawInLineButton(string iconName, bool enable, string tooltip, Action onClick)
        {
            if (!enable)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, style.Alpha * 0.5f);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, style.Colors[(int)ImGuiCol.Button]);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, style.Colors[(int)ImGuiCol.Button]);
            }

            ImGui.SameLine();
            if (ImGui.ImageButton(
                    (IntPtr)ImGuiUn.GetTextureId(ComponentsManager.Instance.GetIcon(iconName)),
                    buttonBarButtonSize) && enable)
            {
                onClick?.Invoke();
            }

            if (!string.IsNullOrEmpty(tooltip) && enable && ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted(tooltip);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }

            if (!enable)
            {
                ImGui.PopStyleVar();
                ImGui.PopStyleColor();
                ImGui.PopStyleColor();
            }
        }

        private void DrawButton(string iconName, bool enable, string tooltip, Action onClick)
        {
            if (!enable)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, style.Alpha * 0.5f);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, style.Colors[(int)ImGuiCol.Button]);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, style.Colors[(int)ImGuiCol.Button]);
            }

            if (ImGui.ImageButton(
                    (IntPtr)ImGuiUn.GetTextureId(ComponentsManager.Instance.GetIcon(iconName)),
                    buttonBarButtonSize) && enable)
            {
                onClick?.Invoke();
            }

            if (!string.IsNullOrEmpty(tooltip) && enable && ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted(tooltip);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }

            if (!enable)
            {
                ImGui.PopStyleVar();
                ImGui.PopStyleColor();
                ImGui.PopStyleColor();
            }
        }

        private void InLineSpacing()
        {
            ImGui.SameLine();
            ImGui.Spacing();

            ImGui.SameLine();
            ImGui.Spacing();
        }

        private void ShowLockCameraWindow()
        {
            ImGui.SetNextWindowSize(new Vector2(250, 300), ImGuiCond.FirstUseEver);
            ImGui.Begin(LocalizationManager.Instance.Localize("LockCamera") + "###lockCamera", ref showLockCamera,
                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking);

            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().x - 105);
            ImGui.InputText("", searchInputBuffer, (uint) searchInputBuffer.Length);

            ImGui.SameLine();
            if (ImGui.Button(LocalizationManager.Instance.Localize("ResetCamera"), new Vector2(100, ImGui.GetItemRectSize().y)))
                CameraController.Instance.SetTarget(null);

            ImGui.Separator();

            ImGui.BeginChild("ScrollingRegion", new Vector2(0, -1), false, ImGuiWindowFlags.HorizontalScrollbar);

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 1));

            var searchString = Encoding.UTF8.GetString(searchInputBuffer).Trim('\0').ToLower();
            foreach (var sceneComponent in ComponentsManager.Instance.GetSceneComponents())
            {
                if (string.IsNullOrEmpty(searchString) || sceneComponent.Name.ToLower().Contains(searchString))
                {
                    ImGui.PushID(sceneComponent.Hash);
                    if (ImGui.ImageButton((IntPtr)ImGuiUn.GetTextureId(ComponentsManager.Instance.GetIcon("Eye")), buttonBarButtonSize))
                        CameraController.Instance.SetTarget(sceneComponent.gameObject);
                    ImGui.PopID();

                    ImGui.SameLine();

                    var cursorPosition = ImGui.GetCursorPos();
                    ImGui.SetCursorPos(new Vector2(cursorPosition.x, cursorPosition.y + ImGui.GetFontSize() / 2));
                    ImGui.Text(sceneComponent.Name);
                }
            }

            ImGui.PopStyleVar();
            ImGui.EndChild();

            ImGui.End();
        }

        private void ShowConsoleWindow()
        {
            ImGui.SetNextWindowSize(new Vector2(520, 300), ImGuiCond.FirstUseEver);
            ImGui.Begin(LocalizationManager.Instance.Localize("Console") + "###console", ref showConsole,
                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking);

            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().x - 55);
            if (ImGui.InputText("", consoleInputBuffer, (uint) consoleInputBuffer.Length,
                    ImGuiInputTextFlags.EnterReturnsTrue))
            {
                AddLog(Encoding.UTF8.GetString(consoleInputBuffer));
                consoleInputBuffer = new byte[consoleInputBuffer.Length];
            }

            ImGui.SameLine();
            if (ImGui.Button(LocalizationManager.Instance.Localize("Send"), new Vector2(50, ImGui.GetItemRectSize().y)))
            {
                var command = Regex.Unescape(Encoding.UTF8.GetString(consoleInputBuffer).Split('\0').First());
                AddLog(command);
                if (SimulationManager.Instance.IsSimulating() && command.Length > 0)
                {
                    foreach (var arduinoUno in FindObjectsOfType<ArduinoUno>())
                        arduinoUno.WriteSerial(command);
                }
                consoleInputBuffer = new byte[consoleInputBuffer.Length];
            }

            ImGui.Separator();

            var footerHeightToReserve = ImGui.GetStyle().ItemSpacing.y + ImGui.GetFrameHeightWithSpacing();
            ImGui.BeginChild("ScrollingRegion", new Vector2(0, -footerHeightToReserve), false,
                ImGuiWindowFlags.HorizontalScrollbar);

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 1));

            foreach (var log in currentLog.Split('\n'))
                ImGui.TextUnformatted(log);

            if (consoleAutoScroll && ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
                ImGui.SetScrollHereY(1.0f);

            ImGui.PopStyleVar();
            ImGui.EndChild();

            ImGui.Separator();
            ImGui.Checkbox(LocalizationManager.Instance.Localize("Auto-scroll"), ref consoleAutoScroll);

            ImGui.SameLine(250);
            var lineEndings = new[]
            {
                LocalizationManager.Instance.Localize("NoLineEnding"), LocalizationManager.Instance.Localize("NewLine")
            };
            ImGui.PushID("LineEnding");
            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().x - 55);
            ImGui.Combo("", ref currentLineEnding, lineEndings, lineEndings.Length);
            ImGui.PopID();

            ImGui.SameLine();
            if (ImGui.Button(LocalizationManager.Instance.Localize("Clear"),
                    new Vector2(50, ImGui.GetItemRectSize().y)))
                ClearLog();

            ImGui.End();
        }

        private void ShowSettingsWindow()
        {
            ImGui.SetNextWindowSize(new Vector2(520, 300), ImGuiCond.FirstUseEver);
            ImGui.Begin(LocalizationManager.Instance.Localize("Settings") + "###settings", ref showSettings,
                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking);

            ImGui.Columns(2);
            var languageCursorPosition = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new Vector2(languageCursorPosition.x, languageCursorPosition.y + ImGui.GetFontSize() / 2));
            ImGui.Text(LocalizationManager.Instance.Localize("Language"));

            ImGui.NextColumn();
            
            ImGui.SetNextItemWidth(-1);
            ImGui.PushID("Language");
            var languages = LocalizationManager.Instance.GetLanguages();
            ImGui.Combo("", ref selectedLanguage, languages, languages.Length);
            ImGui.PopID();

            ImGui.NextColumn();

            var cameraSensitivityCursorPosition = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new Vector2(cameraSensitivityCursorPosition.x, cameraSensitivityCursorPosition.y + ImGui.GetFontSize() / 2));
            ImGui.Text(LocalizationManager.Instance.Localize("CameraSensitivity"));

            ImGui.NextColumn();
            
            ImGui.SetNextItemWidth(-1);
            ImGui.PushID("CameraSensitivity");
            ImGui.SliderFloat("", ref cameraSensitivity, 0, 3);
            ImGui.PopID();
            
            ImGui.NextColumn();

            var showWarningsCursorPosition = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new Vector2(showWarningsCursorPosition.x, showWarningsCursorPosition.y + ImGui.GetFontSize() / 2));
            ImGui.Text(LocalizationManager.Instance.Localize("ShowWarnings"));

            ImGui.NextColumn();
            
            ImGui.SetNextItemWidth(-1);
            ImGui.PushID("ShowWarnings");
            ImGui.Checkbox("", ref showWarnings);
            ImGui.PopID();
            ImGui.Columns(1);

            ImGui.Separator();

            if (ImGui.Button(LocalizationManager.Instance.Localize("Save"),
                    new Vector2(ImGui.GetItemRectSize().x - 10, 30)))
            {
                LocalizationManager.Instance.SaveLanguage(languages[selectedLanguage]);
                LocalizationManager.Instance.SaveCameraSensitivity(cameraSensitivity);
                LocalizationManager.Instance.SaveShowWarnings(showWarnings);
            }

            ImGui.End();
        }

        private void ShowEditCodeWindow()
        {
            ImGui.SetNextWindowSize(new Vector2(520, 300), ImGuiCond.FirstUseEver);
            ImGui.Begin(LocalizationManager.Instance.Localize("CodeEditor") + "###editCode", ref showEditCode,
                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking);

            ImGui.InputTextMultiline("", ref currentCode, 10000,
                new Vector2(ImGui.GetItemRectSize().x - 10, ImGui.GetWindowSize().y - 80),
                ImGuiInputTextFlags.AllowTabInput);
            ImGui.Separator();
            if (ImGui.Button(LocalizationManager.Instance.Localize("Save"),
                    new Vector2(ImGui.GetItemRectSize().x - 10, 30)))
            {
                onCodeSave(currentCode);
            }

            ImGui.End();
        }

        private void ClearLog()
        {
            currentLog = string.Empty;
        }

        private string FormatCurrent(float current)
        {
            var symbols = new[] { "GA", "MA", "kA", "A", "mA", "µA", "nA" };
            var units = new[] { 1e-9, 10e-6, 10e-3, 1, 1e3, 1e6, 1e9 };

            if (current == 0)
                return Math.Round(current, 3) + " " + symbols[3];

            for (var i = 0; i < units.Length; i++)
            {
                var value = current * units[i];
                if (Math.Abs(value) > 1 || i == units.Length - 1)
                    return Math.Round(value, 3).ToString(CultureInfo.InvariantCulture) + " " + symbols[i];
            }

            return string.Empty;
        }

        private string FormatVoltage(float voltage)
        {
            var symbols = new[] { "GV", "MV", "kV", "V", "mV", "µV", "nV" };
            var units = new[] { 1e-9, 10e-6, 10e-3, 1, 1e3, 1e6, 1e9 };

            if (voltage == 0)
                return Math.Round(voltage, 3) + " " + symbols[3];

            for (var i = 0; i < units.Length; i++)
            {
                var value = voltage * units[i];
                if (Math.Abs(value) > 1 || i == units.Length - 1)
                    return Math.Round(value, 3).ToString(CultureInfo.InvariantCulture) + " " + symbols[i];
            }

            return string.Empty;
        }

        #endregion
    }
}