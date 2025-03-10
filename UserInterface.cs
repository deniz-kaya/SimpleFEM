using System.Numerics;
using System.Xml;
using ImGuiNET;
using Raylib_cs;
using SimpleFEM.Derived;
using SimpleFEM.Extensions;
using SimpleFEM.Interfaces;
using SimpleFEM.LinearAlgebra;
using SimpleFEM.Types;
using SimpleFEM.Types.Settings;
using SimpleFEM.Types.StructureTypes;
using Material = SimpleFEM.Types.StructureTypes.Material;

namespace SimpleFEM;

public class UserInterface
{
    private UIStructureEditor structureEditor;
    private UISceneRenderer sceneRenderer;
    private UIStructureSolver structureSolver;
    private StructureEditorSettings structureSettings;
    private DrawSettings drawSettings;
    private HotkeySettings settings;
    private OperationMode CurrentOperation;
    public UserInterface(IStructure structure, HotkeySettings settings)
    {
        CurrentOperation = OperationMode.Editor;
        structureSolver = new UIStructureSolver(structure);
        structureEditor = new UIStructureEditor(structure, StructureEditorSettings.Default);
        drawSettings = DrawSettings.Default;
        sceneRenderer = new UISceneRenderer(SceneRendererSettings.Default);
        this.settings = settings;
    }

    public void DrawSceneWindow()
    {
        if (CurrentOperation == OperationMode.Editor)
        {
            sceneRenderer.SetRenderQueue(structureEditor.GetSceneObjects(drawSettings));
        }
        else if (CurrentOperation == OperationMode.Solver)
        {
            sceneRenderer.SetRenderQueue(structureSolver.GetSceneObjects(drawSettings));
        }
        sceneRenderer.ShowSceneWindow();
    }
    public void DrawFooter()
    {
        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoTitleBar 
                                       | ImGuiWindowFlags.NoCollapse
                                       | ImGuiWindowFlags.NoDocking
                                       | ImGuiWindowFlags.NoResize
                                       | ImGuiWindowFlags.NoScrollbar
                                       | ImGuiWindowFlags.NoNav;
        
        // TODO variables
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(5,3));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        
        Vector2 size = ImGui.GetMainViewport().Size;
        ImGui.SetNextWindowSize(new Vector2(size.X, settings.FooterHeight));
        ImGui.SetNextWindowPos(new Vector2(0, size.Y-settings.FooterHeight));
        
        ImGui.Begin("Footer", windowFlags);

        float width = ImGui.GetContentRegionAvail().X;
        
        //Left of the footer
        ImGui.Text($"Current tool: {structureEditor.CurrentTool.ToString().Replace('_', ' ')}");
        //Right of the footer

        if (sceneRenderer.SceneWindowHovered)
        {
            string mousePosition = structureEditor.GetRealCoordinates(sceneRenderer.GetScenePos(ImGui.GetMousePos())).ToString();
            ImGui.SameLine(width - ImGui.CalcTextSize(mousePosition).X);
            ImGui.Text(mousePosition);
        }

        ImGui.End();
        
        ImGui.PopStyleVar(10);
    }

    public void DrawHoveredPropertyViewer()
    {
        ImGui.Begin("Property Viewer");
        if (CurrentOperation == OperationMode.Editor)
        {
            structureEditor.DrawHoveredPropertiesViewer();
        }
        else if (CurrentOperation == OperationMode.Solver)
        {
            ImGui.Text("Switch back to editor to see properties!");
        }
        ImGui.End();

    }
    public void DrawMainDockSpace()
    {
        float mainMenuBarHeight = ImGui.GetFrameHeight();
        float footerHeight = settings.FooterHeight;
        Vector2 viewportSize = ImGui.GetMainViewport().Size;
        
        ImGui.SetNextWindowPos(new Vector2(0, mainMenuBarHeight));
        ImGui.SetNextWindowSize(new Vector2(viewportSize.X, (viewportSize.Y - mainMenuBarHeight) - footerHeight));
        
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar 
                                 | ImGuiWindowFlags.NoCollapse 
                                 | ImGuiWindowFlags.NoResize 
                                 | ImGuiWindowFlags.NoMove
                                 | ImGuiWindowFlags.NoScrollbar
                                 | ImGuiWindowFlags.NoNav
                                 | ImGuiWindowFlags.NoBackground
                                 | ImGuiWindowFlags.NoInputs
                                 | ImGuiWindowFlags.NoMouseInputs
                                 | ImGuiWindowFlags.NoScrollWithMouse
                                 | ImGuiWindowFlags.NoDecoration
                                 | ImGuiWindowFlags.NoBackground 
                                 | ImGuiWindowFlags.NoDocking;
        
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        
        ImGui.Begin("Dockspace", flags);
        
        ImGui.DockSpace(ImGui.GetID("MainDockSpace"), Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);
        ImGui.End();
        ImGui.PopStyleVar(2);

    }

    public void Test()
    {
        ImGui.Text("This is a test window!");
        if (ImGui.Button("Test button"))
        {
            Console.WriteLine("pressed button!");
        }
    }
    public void DrawStructureOperationWindow()
    {
        // TODO rename
        ImGui.Begin("Structure Operations");

        if (ImGui.BeginTabBar("Structure Operations Tab Bar"))
        {
            if (ImGui.BeginTabItem("Editor"))
            {
                CurrentOperation = OperationMode.Editor;
                structureEditor.DrawOperationWindow();
                
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Solver"))
            {
                CurrentOperation = OperationMode.Solver;
                structureSolver.DrawOperationWindow();
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }
        // if (ImGui.Button("Solve current system"))
        // {
        //     structureSolver.Solve();
        //     
        // }
        // //todo fix?
        // if (ImGui.Button("test element intersection"))
        // {
        //     if (structureSolver.CheckElementIntersections())
        //     {
        //         Console.WriteLine("there are no intersection");
        //     }
        //     else
        //     {
        //         Console.WriteLine("there is intersections");
        //     }
        // }
        
        ImGui.End();
    }
    public void DrawMainMenuBar()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("New Project"))
                {
                    //todo new project popup modal
                }

                if (ImGui.MenuItem("Open Project"))
                {
                    //todo open project popup modal
                }

                if (ImGui.MenuItem("Save Project"))
                {
                    //todo save structure popup/thing
                }

                if (ImGui.MenuItem("Save Project As"))
                {
                    //todo save structure as popup
                }
                
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit"))
            {
                if (ImGui.MenuItem("Add new material"))
                {
                    structureEditor.OpenAddMaterialModal = true;
                }

                if (ImGui.MenuItem("Add new section"))
                {
                    structureEditor.OpenAddSectionModal = true;
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Options"))
            {
                openSettingsEditorModal = true;
                ImGui.EndMenu();
            }
            ImGui.EndMainMenuBar();
        }
    }

    public void DrawToolbar()
    {
        ImGui.Begin("Toolbar");
        foreach (Tool t in Enum.GetValues(typeof(Tool))) 
        {
            ImGui.SameLine();
            if (ImGui.Button(t.ToString().Replace('_', ' ')))
            {
                structureEditor.SwitchTool(t);
            }
        }
    }
    //Input Handling
    public void HandleCameraMovement()
    {
        if (ImGui.IsKeyDown(ImGuiKey.RightArrow))
        {
            sceneRenderer.OperateCamera(CameraOperation.Right);
        }
        if (ImGui.IsKeyDown(ImGuiKey.LeftArrow))
        {
            sceneRenderer.OperateCamera(CameraOperation.Left);
        }
        if (ImGui.IsKeyDown(ImGuiKey.DownArrow))
        {
            sceneRenderer.OperateCamera(CameraOperation.Down);
        }
        if (ImGui.IsKeyDown(ImGuiKey.UpArrow))
        {
            sceneRenderer.OperateCamera(CameraOperation.Up);
        }
        if (ImGui.IsKeyPressed(ImGuiKey.PageUp))
        {
            sceneRenderer.OperateCamera(CameraOperation.ZoomIn);
        }
        if (ImGui.IsKeyPressed(ImGuiKey.PageDown))
        {
            sceneRenderer.OperateCamera(CameraOperation.ZoomOut);
        }
    }
    public void HandleInputs()
    {
        Vector2 scenePos = sceneRenderer.GetScenePos(ImGui.GetMousePos());
        structureEditor.SetLivePos(scenePos);
        HandleCameraMovement();
        if (sceneRenderer.SceneWindowHovered && CurrentOperation == OperationMode.Editor)
        {
            
            structureEditor.UpdateHoveredItems();
            HandleToolSwitchInputs();
            if (ImGui.IsKeyPressed(ImGuiKey.MouseRight))
            {
                HandleRightClickPopupDisplay();
            }
            
            if (ImGui.IsKeyDown(ImGuiKey.MouseLeft))
            {
                structureEditor.HandleMouseKeyDownEvent();
            }
            else if (ImGui.IsKeyReleased(ImGuiKey.MouseLeft))
            {
                structureEditor.HandleMouseKeyUpEvent();
            }
            if (ImGui.IsKeyPressed(ImGuiKey.MouseLeft))
            {
                structureEditor.HandleMouseKeyPressedEvent();
            }

            if (ImGui.IsKeyPressed(ImGuiKey.Delete))
            {
                //todo delete confirmation?
                structureEditor.DeleteSelectedElements();
                structureEditor.DeleteSelectedNodes();
            }

            
        }
    }

    public void HandlePopups()
    {
        structureEditor.HandlePopups();
        structureSolver.HandlePopups();
        DefineSettingsEditorModal();

        if (openSettingsEditorModal)
        {
            ImGui.OpenPopup("SettingsEditorModal");
            openSettingsEditorModal = false;
        }
        
    }
    public void HandleToolSwitchInputs()
    {
        if (ImGui.IsKeyPressed(settings.AddNodeToolKey))
        {
            structureEditor.SwitchTool(Tool.Add_Node);
        }
        else if (ImGui.IsKeyPressed(settings.AddElementToolKey))
        {
            structureEditor.SwitchTool(Tool.Add_Element);
        }
        else if (ImGui.IsKeyPressed(settings.SelectNodesToolKey))
        {
            structureEditor.SwitchTool(Tool.Select_Nodes);
        }
        else if (ImGui.IsKeyPressed(settings.SelectElementsToolKey))
        {
            structureEditor.SwitchTool(Tool.Select_Elements);
        }
        else if (ImGui.IsKeyPressed(settings.MouseSelectToolKey))
        {
            structureEditor.SwitchTool(Tool.Mouse_Select);
        }
    }

    public void HandleRightClickPopupDisplay()
    {
        if (structureEditor.SelectedNodeCount != 0 && structureEditor.SelectedElementCount == 0)
        {
            ImGui.OpenPopup("SelectedNodePopup");
        }
    }
    
    //Popups
    // TODO rename properties to be better
    // Popup Properties
    //---------
    private Vector4 sceneElementColor = new();
    private Vector4 sceneNodeColor = new();
    private Vector4 sceneSelectedElementColor = new();
    private Vector4 sceneSelectedNodeColor = new();
    private Vector4 sceneHoveredElementColor = new();
    private Vector4 sceneHoveredNodeColor = new();
    private Vector4 sceneSelectionBoxColor = new();
    private float sceneElementThickness;
    private float sceneNodeRadius;

    //todo finish scenedrawsettings, add note saying please put something for all
    private void DefineSceneDrawSettings()
    {
        ImGui.ColorEdit4("Element Color", ref sceneElementColor);
        ImGui.ColorEdit4("Node Color", ref sceneNodeColor);
        ImGui.ColorEdit4("Selected Element Color", ref sceneSelectedElementColor);
        ImGui.ColorEdit4("Selected Node Color", ref sceneSelectedNodeColor);
        ImGui.ColorEdit4("Hovered Element Color", ref sceneHoveredElementColor);
        ImGui.ColorEdit4("Hovered Node Color", ref sceneHoveredNodeColor);
        ImGui.ColorEdit4("Selection Box Color", ref sceneSelectionBoxColor);
        ImGui.DragFloat("Element Thickness", ref sceneElementThickness, 0.1f, 1f, 5f);
        ImGui.DragFloat("Node Radius", ref sceneNodeRadius, 0.1f, 1f, 5f);
        //todo saving and loading userdata on program launch
        if (ImGui.Button("Save Settings"))
        {
            drawSettings = new DrawSettings(
                sceneElementColor,
                sceneNodeColor,
                sceneSelectedElementColor,
                sceneSelectedNodeColor,
                sceneHoveredElementColor,
                sceneHoveredNodeColor,
                sceneSelectionBoxColor,
                sceneElementThickness,
                sceneNodeRadius
            );
        }

        if (ImGui.Button("Reset to defaults"))
        {
            drawSettings = DrawSettings.Default;
        }
    }
    
    private void DefineToolHotkeysSettings()
    {

    }

    private void DefineEditorSettings()
    {
        
    }

    private bool openSettingsEditorModal;
    public void DefineSettingsEditorModal()
    {

        if (ImGui.BeginPopupModal("SettingsEditorModal"))
        {
            if (ImGui.BeginTabBar("SettingsTabBar"))
            {
                if (ImGui.BeginTabItem("Scene Draw settings"))
                {
                    DefineSceneDrawSettings();
                }
                if (ImGui.BeginTabItem("Tool Hotkeys"))
                {
                    DefineToolHotkeysSettings();
                }

                if (ImGui.BeginTabItem("Editor Settings"))
                {
                    DefineEditorSettings();
                }
                ImGui.EndTabBar();
            }
            ImGui.Separator();
            if (ImGui.Button("Close"))
            {
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
    }
}