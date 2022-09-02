using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Assets.Scripts.Camera;
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
        private bool displayPortOverlay;
        private bool showConsole;

        private string overlayPortName;
        private PortType overlayPortType;
        private PinType overlayPinType;

        private string selectedCategory;

        private bool consoleAutoScroll;
        private byte[] consoleInputBuffer;

        private string currentLog;
        private int currentLineEnding = 1;

        private Action currentPopupAction = () => { };

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

            currentLog = string.Empty;
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

        #region Public Methods

        public bool IsMouserOverUI()
        {
            return !displayPortOverlay && isMouseOverUI;
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

        public void ShowConsole()
        {
            showConsole = !showConsole;
        }

        public void AddLog(string log, int lineEnding = 1)
        {
            log = Regex.Unescape(log.Split('\0').First());
            currentLog += log + (lineEnding == 1 ? "\n" : "");
        }

        public void BeginPropertyBar()
        {
            ImGui.Columns(2);
        }

        public void GenerateStringPropertyField(string label, byte[] stringBuffer)
        {
            var cursorPosition = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new Vector2(cursorPosition.x, cursorPosition.y + ImGui.GetFontSize() / 2));
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGui.InputText(label, stringBuffer, (uint)stringBuffer.Length);
            ImGui.NextColumn();
        }

        public void GenerateIntPropertyField(string label, ref int value)
        {
            var cursorPosition = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new Vector2(cursorPosition.x, cursorPosition.y + ImGui.GetFontSize() / 2));
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGui.InputInt(label, ref value, 10, 100);
            ImGui.NextColumn();
        }

        public void GenerateComboBoxPropertyField(string label, ref int selectedIndex, string[] items)
        {
            var cursorPosition = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new Vector2(cursorPosition.x, cursorPosition.y + ImGui.GetFontSize() / 2));
            ImGui.Text(label);
            ImGui.NextColumn();
            ImGui.SetNextItemWidth(-1);
            ImGui.Combo(label, ref selectedIndex, items, items.Length);
            ImGui.NextColumn();
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

        public void EndPropertyBar()
        {
            ImGui.Columns(1);
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

            ShowButtonBar();
            ShowComponentsWindow();
            ShowPropertiesWindow();

            if (showConsole)
                ShowConsoleWindow();

            if (displayPortOverlay)
                ShowPortOverlay();
        }

        private void ShowComponentsWindow()
        {
            if (ImGui.Begin(LocalizationManager.Instance.Localize("Menu.Components") + "###components", ImGuiWindowFlags.NoCollapse))
            {
                var categories = ComponentsManager.Instance.GetComponentsCategories();
                // Left
                ImGui.BeginChild("left pane", new Vector2(40, 0), false, ImGuiWindowFlags.NoScrollbar);
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
                    selectedComponent.DrawPropertiesWindow();
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

            InLineSpacing();

            DrawInLineButton("Console", true, LocalizationManager.Instance.Localize("OpenConsole"),
                () => ShowConsole());

            InLineSpacing();

            var menuBarSize = ImGui.GetWindowSize();
            var pausePosition = menuBarSize.x / 2 - buttonBarButtonSize.x / 2;
            var playPosition = pausePosition - buttonBarButtonSize.x - 3 * padding.x;
            var stopPosition = pausePosition + buttonBarButtonSize.x + 3 * padding.x;
            var timePosition = stopPosition + buttonBarButtonSize.x + 3 * padding.x;

            ImGui.SetCursorPos(new Vector2(playPosition, padding.y));

            DrawButton("Play", !SimulationManager.Instance.IsSimulating(),
                LocalizationManager.Instance.Localize("StartSimulation"),
                () => SimulationManager.Instance.StartSimulation());

            ImGui.SetCursorPos(new Vector2(pausePosition, padding.y));
            DrawButton("Pause", SimulationManager.Instance.IsSimulating(),
                LocalizationManager.Instance.Localize("PauseSimulation"), null);
            ImGui.SetCursorPos(new Vector2(stopPosition, padding.y));
            DrawButton("Stop", SimulationManager.Instance.IsSimulating(),
                LocalizationManager.Instance.Localize("StopSimulation"),
                () => SimulationManager.Instance.StopSimulation());

            if (SimulationManager.Instance.IsSimulating())
            {
                var us = SimulationManager.Instance.GetTime();
                var ms = us * 1000;
                var s = ms / 1000 % 60;
                var m = ms / 1000 / 60;
                var h = m / 60;

                string timeString;
                if (h > 1)
                    timeString = h.ToString("0", CultureInfo.InvariantCulture) + " h " +
                                 m.ToString("0", CultureInfo.InvariantCulture) + " m " +
                                 s.ToString("0.00", CultureInfo.InvariantCulture) + " s";
                else if (m > 1)
                    timeString = m.ToString("0", CultureInfo.InvariantCulture) + " m " +
                                 s.ToString("0.00", CultureInfo.InvariantCulture) + " s";
                else if (s > 1)
                    timeString = s.ToString("0.00", CultureInfo.InvariantCulture) + " s";
                else
                    timeString = ms.ToString("0", CultureInfo.InvariantCulture) + " ms";

                ImGui.SetCursorPos(new Vector2(timePosition, menuBarSize.y / 2 - ImGui.GetFontSize() / 2));
                ImGui.Text(LocalizationManager.Instance.Localize("SimulationTime") + ": " + timeString);
            }

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

        private void ShowConsoleWindow()
        {
            ImGui.SetNextWindowSize(new Vector2(520, 300), ImGuiCond.FirstUseEver);
            ImGui.Begin(LocalizationManager.Instance.Localize("Console"), ref showConsole,
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
                AddLog(Encoding.UTF8.GetString(consoleInputBuffer));
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

        private void ClearLog()
        {
            currentLog = string.Empty;
        }

        #endregion
    }
}