using ImGuiNET;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region Properties

    public static UIManager Instance { get; private set; }

    #endregion

    #region Fields

    private const ImGuiDockNodeFlags DockspaceFlags =
        ImGuiDockNodeFlags.PassthruCentralNode | ImGuiDockNodeFlags.NoDockingInCentralNode;

    private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking |
                                                 ImGuiWindowFlags.NoBackground |
                                                 ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse |
                                                 ImGuiWindowFlags.NoResize |
                                                 ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBringToFrontOnFocus |
                                                 ImGuiWindowFlags.NoNavFocus;

    private ImGuiStylePtr style;
    private bool setupDearImGui = true;

    private bool isMouseOverUI;

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
        return isMouseOverUI;
    }

    #endregion

    #region Layouts

    private void ApplyTheme()
    {
        style = ImGui.GetStyle();
        var colors = style.Colors;

        colors[(int)ImGuiCol.Text] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
        colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.40f, 0.40f, 0.40f, 1.00f);
        colors[(int)ImGuiCol.ChildBg] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
        colors[(int)ImGuiCol.WindowBg] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
        colors[(int)ImGuiCol.PopupBg] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
        colors[(int)ImGuiCol.Border] = new Vector4(0.12f, 0.12f, 0.12f, 0.71f);
        colors[(int)ImGuiCol.BorderShadow] = new Vector4(1.00f, 1.00f, 1.00f, 0.06f);
        colors[(int)ImGuiCol.FrameBg] = new Vector4(0.42f, 0.42f, 0.42f, 0.54f);
        colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.42f, 0.42f, 0.42f, 0.40f);
        colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.56f, 0.56f, 0.56f, 0.67f);
        colors[(int)ImGuiCol.TitleBg] = new Vector4(0.19f, 0.19f, 0.19f, 1.00f);
        colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.19f, 0.19f, 0.19f, 1.00f);
        colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.17f, 0.17f, 0.17f, 0.90f);
        colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.335f, 0.335f, 0.335f, 1.000f);
        colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.24f, 0.24f, 0.24f, 0.53f);
        colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.41f, 0.41f, 0.41f, 1.00f);
        colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.52f, 0.52f, 0.52f, 1.00f);
        colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.76f, 0.76f, 0.76f, 1.00f);
        colors[(int)ImGuiCol.CheckMark] = new Vector4(0.65f, 0.65f, 0.65f, 1.00f);
        colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.52f, 0.52f, 0.52f, 1.00f);
        colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.64f, 0.64f, 0.64f, 1.00f);
        colors[(int)ImGuiCol.Button] = new Vector4(0.54f, 0.54f, 0.54f, 0.35f);
        colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.52f, 0.52f, 0.52f, 0.59f);
        colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.76f, 0.76f, 0.76f, 1.00f);
        colors[(int)ImGuiCol.Header] = new Vector4(0.38f, 0.38f, 0.38f, 1.00f);
        colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.47f, 0.47f, 0.47f, 1.00f);
        colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.76f, 0.76f, 0.76f, 0.77f);
        colors[(int)ImGuiCol.Separator] = new Vector4(0.000f, 0.000f, 0.000f, 0.137f);
        colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.700f, 0.671f, 0.600f, 0.290f);
        colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.702f, 0.671f, 0.600f, 0.674f);
        colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.26f, 0.59f, 0.98f, 0.25f);
        colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.26f, 0.59f, 0.98f, 0.67f);
        colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.26f, 0.59f, 0.98f, 0.95f);
        colors[(int)ImGuiCol.PlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
        colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
        colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.73f, 0.73f, 0.73f, 0.35f);
        colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.35f);
        colors[(int)ImGuiCol.DragDropTarget] = new Vector4(1.00f, 1.00f, 0.00f, 0.90f);
        colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);
        colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
        colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
        colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.533f, 0.078f, 0.078f, 1f);
        colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.894f, 0.133f, 0.149f, 1f);
        colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.894f, 0.133f, 0.149f, 1f);
        colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.894f, 0.133f, 0.149f, 1f);

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
        colors[(int)ImGuiCol.Tab] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
        colors[(int)ImGuiCol.TabHovered] = new Vector4(0.40f, 0.40f, 0.40f, 1.00f);
        colors[(int)ImGuiCol.TabActive] = new Vector4(0.33f, 0.33f, 0.33f, 1.00f);
        colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
        colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.33f, 0.33f, 0.33f, 1.00f);
    }

    private void OnLayout()
    {
        if (setupDearImGui)
        {
            ImGui.CaptureMouseFromApp(false);
            ApplyTheme();

            setupDearImGui = false;
        }

        var viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.Pos);
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

        ShowAppMainMenuBar();
        ShowComponentsWindow();
    }

    private void ShowAppMainMenuBar()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu(LocalizationManager.Instance.Localize("Menu.File")))
            {
                if (ImGui.MenuItem("Undo", "CTRL+Z"))
                {
                }

                if (ImGui.MenuItem("Redo", "CTRL+Y", false, false))
                {

                }

                ImGui.Separator();
                if (ImGui.MenuItem("Cut", "CTRL+X"))
                {
                }

                if (ImGui.MenuItem("Copy", "CTRL+C"))
                {
                }

                if (ImGui.MenuItem("Paste", "CTRL+V"))
                {
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu(LocalizationManager.Instance.Localize("Menu.Edit")))
            {
                if (ImGui.MenuItem("Undo", "CTRL+Z"))
                {
                }

                if (ImGui.MenuItem("Redo", "CTRL+Y", false, false))
                {

                }

                ImGui.Separator();
                if (ImGui.MenuItem("Cut", "CTRL+X"))
                {
                }

                if (ImGui.MenuItem("Copy", "CTRL+C"))
                {
                }

                if (ImGui.MenuItem("Paste", "CTRL+V"))
                {
                }

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }
    }

    private void ShowComponentsWindow()
    {
        ImGui.Begin("Components", ImGuiWindowFlags.NoCollapse);

        ImGui.End();
    }

    #endregion
}