using System.Numerics;
using System.Xml;
using ImGuiNET;
using Raylib_cs;
using SimpleFEM.Base;
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
    private UIStructureSolver structureSolver;
    private UISceneRenderer sceneRenderer;
    public bool StructureLoaded { get; private set; }
    
    private StructureEditorSettings structureEditorSettings;
    
    private DrawSettings drawSettings;
    private HotkeySettings settings;

    private bool ShouldShowNewProjectModal;
    private bool ShouldShowOpenProjectModal;

    
    private OperationMode CurrentOperation;
    public UserInterface()
    {
        StructureLoaded = false;
        structureEditorSettings = StructureEditorSettings.Default;
        CurrentOperation = OperationMode.Editor;
        drawSettings = DrawSettings.Default;
        settings = HotkeySettings.Default;
        sceneRenderer = new UISceneRenderer(SceneRendererSettings.Default);
    }

    public void SwitchStructure(IStructure structure)
    {
        CurrentOperation = OperationMode.Editor;
        StructureLoaded = true;
        structureEditor = new UIStructureEditor(structure, structureEditorSettings);
        structureSolver = new UIStructureSolver(structure);
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
                    ShouldShowNewProjectModal = true;
                }

                if (ImGui.MenuItem("Open Project"))
                {
                    ShouldShowOpenProjectModal = true;
                }

                if (StructureLoaded && ImGui.MenuItem("Save Project As"))
                {
                    //todo save structure as popup
                }
                
                ImGui.EndMenu();
            }

            if (StructureLoaded && ImGui.BeginMenu("Edit"))
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
                if (ImGui.MenuItem("Preferences"))
                {
                    openSettingsEditorModal = true;
                }

                ImGui.EndMenu();
            }
            ImGui.EndMainMenuBar();
        }
    }

    private string NewProjectModalErrorMessage = String.Empty;
    private string NewProjectModalFilepath = String.Empty;
    private string NewProjectModalProjectName = String.Empty;
    private float NewProjectModalGridSpacingSize = 0f;
    const int MaxStringInputLength = 1024;
    public void DefineNewProjectModal()
    {
        if (ImGui.BeginPopupModal("New Project", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.SeparatorText("Filepath");
            ImGui.TextWrapped("The filepath of the project");
            ImGui.InputText("Path", ref NewProjectModalFilepath, MaxStringInputLength);
            ImGui.SeparatorText("Project name");
            ImGui.TextWrapped("The name of the project. The project will be saved as a .structure file with this name on the given path.");
            ImGui.InputText("Name", ref NewProjectModalProjectName, MaxStringInputLength);
            ImGui.SeparatorText("Grid spacing distance");
            ImGui.TextWrapped("What distance one grid step will be equal to, in metres");
            ImGui.InputFloat("Grid Spacing", ref NewProjectModalGridSpacingSize);

            if (ImGui.Button("Create"))
            {
                if (!Path.Exists(NewProjectModalFilepath))
                {
                    NewProjectModalErrorMessage = "The filepath is inaccessible! Check for invalid characters.";
                }
                else if (NewProjectModalProjectName == String.Empty)
                {
                    NewProjectModalErrorMessage = "The project name cannot be blank!";
                }
                else if (Path.GetInvalidFileNameChars().Any(NewProjectModalProjectName.Contains))
                {
                    NewProjectModalErrorMessage = "The project name has invalid characters! Use characters that are valid for a filename.";
                }
                else if (File.Exists(NewProjectModalFilepath))
                {
                    NewProjectModalErrorMessage = "Filepath points to a file! Check and correct filepath to remove extension.";
                }
                else if (Path.Exists(Path.Combine(NewProjectModalFilepath, NewProjectModalProjectName + ".structure")))
                {
                    NewProjectModalErrorMessage = "This file already exists! Try opening the project instead.";
                }
                else if (NewProjectModalGridSpacingSize == 0f)
                {
                    NewProjectModalErrorMessage = "The grid spacing size cannot be zero!";
                }
                else
                {
                    IStructure newStructure = new DatabaseStructure(
                        NewProjectModalFilepath, 
                        NewProjectModalProjectName, 
                        new StructureSettings(NewProjectModalGridSpacingSize));
                    NewProjectModalErrorMessage = String.Empty;
                    NewProjectModalProjectName = String.Empty;
                    SwitchStructure(newStructure);
                    ImGui.CloseCurrentPopup();
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                NewProjectModalErrorMessage = String.Empty;
                NewProjectModalProjectName = String.Empty;
                ImGui.CloseCurrentPopup();
            }
            ImGui.TextWrapped(NewProjectModalErrorMessage);
            ImGui.EndPopup();
        }
    }

    private string OpenProjectModalFilePath = String.Empty;
    private string OpenProjectModalErrorMessage = String.Empty;
    public void DefineOpenProjectModal()
    {
        if (ImGui.BeginPopupModal("Open Project", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.SeparatorText("Filepath");
            ImGui.TextWrapped("The full filepath of the project");
            ImGui.InputText("Path", ref OpenProjectModalFilePath, MaxStringInputLength);
            if (ImGui.Button("Open"))
            {
                if (!Path.Exists(OpenProjectModalFilePath))
                {
                    OpenProjectModalErrorMessage = "The filepath is inaccessible! Check for invalid characters.";
                }
                else if (!File.Exists(OpenProjectModalFilePath))
                {
                    OpenProjectModalErrorMessage = "File doesn't exist! Check path to ensure it points to a valid file.";
                }
                else if (!DatabaseStructure.FileIsSqliteDatabase(OpenProjectModalFilePath))
                {
                    OpenProjectModalErrorMessage = "The file is invalid! Might be corrupted.";
                }
                else
                {
                    IStructure newStructure = new DatabaseStructure(OpenProjectModalFilePath);
                    OpenProjectModalFilePath = String.Empty;
                    OpenProjectModalErrorMessage = String.Empty;
                    SwitchStructure(newStructure);
                    ImGui.CloseCurrentPopup();
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                OpenProjectModalFilePath = String.Empty;
                OpenProjectModalErrorMessage = String.Empty;
                ImGui.CloseCurrentPopup();
            }
            ImGui.TextWrapped(OpenProjectModalErrorMessage);
            ImGui.EndPopup();
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
        if (sceneRenderer.SceneWindowHovered)
        {
            HandleCameraMovement();

            if (CurrentOperation != OperationMode.Editor) return;
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
        if (StructureLoaded)
        {
            structureEditor.HandlePopups();
            structureSolver.HandlePopups();
        }

        DefineSettingsEditorModal();
        DefineNewProjectModal();
        DefineOpenProjectModal();
        
        //todo popup flag names
        if (openSettingsEditorModal)
        {
            ImGui.OpenPopup("SettingsEditorModal");
            openSettingsEditorModal = false;
        }

        if (ShouldShowNewProjectModal)
        {
            ImGui.OpenPopup("New Project");
            ShouldShowNewProjectModal = false;
        }

        if (ShouldShowOpenProjectModal)
        {
            ImGui.OpenPopup("Open Project");
            ShouldShowOpenProjectModal = false;
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