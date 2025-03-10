using System.Numerics;
using System.Text.RegularExpressions;
using ImGuiNET;
using SimpleFEM.Base;
using SimpleFEM.Derived;
using SimpleFEM.Interfaces;
using SimpleFEM.Types;
using SimpleFEM.Types.Settings;

namespace SimpleFEM;

public class UserInterface
{
    private UIStructureEditor _structureEditor;
    private UIStructureSolver _structureSolver;
    private UISceneRenderer _sceneRenderer;
    
    public bool StructureLoaded { get; private set; }
    private bool _volatileStructure;
    
    private const string SaveFileExtension = ".structure";
    private string _currentStructureFilepath;
    
    private StructureEditorSettings _structureEditorSettings;
    private DrawSettings _drawSettings;
    private UserInterfaceSettings _settings;

    private bool _shouldShowNewProjectModal;
    private bool _shouldShowOpenProjectModal;
    private bool _shouldShowSaveProjectAsModal;

    
    private OperationMode _currentOperation;
    public UserInterface()
    {
        StructureLoaded = false;
        _currentOperation = OperationMode.Editor;
        _structureEditorSettings = StructureEditorSettings.Default;
        _drawSettings = DrawSettings.Default;
        _settings = UserInterfaceSettings.Default;
        _sceneRenderer = new UISceneRenderer(SceneRendererSettings.Default);
    }

    private void SwitchStructure(IStructure structure)
    {
        _currentOperation = OperationMode.Editor;
        StructureLoaded = true;
        _structureEditor = new UIStructureEditor(structure, _structureEditorSettings);
        _structureSolver = new UIStructureSolver(structure);
    }
    
    //imgui windows
    public void DrawSceneWindow()
    {
        if (_currentOperation == OperationMode.Editor)
        {
            _sceneRenderer.SetRenderQueue(_structureEditor.GetSceneObjects(_drawSettings));
        }
        else if (_currentOperation == OperationMode.Solver)
        {
            _sceneRenderer.SetRenderQueue(_structureSolver.GetSceneObjects(_drawSettings));
        }
        _sceneRenderer.ShowSceneWindow();
    }
    public void DrawFooter()
    {
        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoTitleBar 
                                       | ImGuiWindowFlags.NoCollapse
                                       | ImGuiWindowFlags.NoDocking
                                       | ImGuiWindowFlags.NoResize
                                       | ImGuiWindowFlags.NoScrollbar
                                       | ImGuiWindowFlags.NoNav;
        
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(5,3));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        
        Vector2 size = ImGui.GetMainViewport().Size;
        ImGui.SetNextWindowSize(new Vector2(size.X, _settings.FooterHeight));
        ImGui.SetNextWindowPos(new Vector2(0, size.Y-_settings.FooterHeight));
        
        ImGui.Begin("Footer", windowFlags);

        float width = ImGui.GetContentRegionAvail().X;
        
        //Left of the footer
        ImGui.Text($"Current tool: {Regex.Replace(_structureEditor.CurrentTool.ToString(), "(?<!^)([A-Z])"," $1")}");
        //Right of the footer

        if (_sceneRenderer.SceneWindowHovered)
        {
            string mousePosition = _structureEditor.GetRealCoordinates(_sceneRenderer.GetScenePos(ImGui.GetMousePos())).ToString();
            ImGui.SameLine(width - ImGui.CalcTextSize(mousePosition).X);
            ImGui.Text(mousePosition);
        }

        ImGui.End();
        
        ImGui.PopStyleVar(10);
    }
    public void DrawHoveredPropertyViewer()
    {
        ImGui.Begin("Property Viewer");
        if (_currentOperation == OperationMode.Editor)
        {
            _structureEditor.DrawHoveredPropertiesViewer();
        }
        else if (_currentOperation == OperationMode.Solver)
        {
            ImGui.Text("Switch back to editor to see properties!");
        }
        ImGui.End();

    }
    public void DrawMainDockSpace()
    {
        float mainMenuBarHeight = ImGui.GetFrameHeight();
        float footerHeight = _settings.FooterHeight;
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
    public void DrawStructureOperationWindow()
    {
        ImGui.Begin("Structure Operations");

        if (ImGui.BeginTabBar("Structure Operations Tab Bar"))
        {
            if (ImGui.BeginTabItem("Editor"))
            {
                _currentOperation = OperationMode.Editor;
                _structureEditor.DrawOperationWindow();
                
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Solver"))
            {
                _currentOperation = OperationMode.Solver;
                _structureSolver.DrawOperationWindow();
                
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
                    _shouldShowNewProjectModal = true;
                }

                if (ImGui.MenuItem("Open Project"))
                {
                    _shouldShowOpenProjectModal = true;
                }

                if (StructureLoaded && !_volatileStructure && ImGui.MenuItem("Save Project As"))
                {
                    _shouldShowSaveProjectAsModal = true;
                }
                
                ImGui.EndMenu();
            }

            if (StructureLoaded && ImGui.BeginMenu("Edit"))
            {
                if (ImGui.MenuItem("Add new material"))
                {
                    _structureEditor.OpenAddMaterialModal = true;
                }

                if (ImGui.MenuItem("Add new section"))
                {
                    _structureEditor.OpenAddSectionModal = true;
                }
                ImGui.EndMenu();
            }

            // if (ImGui.BeginMenu("Options"))
            // {
            //     if (ImGui.MenuItem("Preferences"))
            //     {
            //         ShouldShowPreferencesModal = true;
            //     }
            //
            //     ImGui.EndMenu();
            // }
            ImGui.EndMainMenuBar();
        }
    }
    public void DrawToolbar()
    {
        ImGui.Begin("Toolbar");
        foreach (Tool t in Enum.GetValues(typeof(Tool))) 
        {
            ImGui.SameLine();
            if (ImGui.Button(Regex.Replace(t.ToString(), "(?<!^)([A-Z])"," $1")))
            {
                _structureEditor.SwitchTool(t);
            }
        }
    }
    
    
    //Input Handling
    private void HandleCameraMovement()
    {
        if (ImGui.IsKeyDown(ImGuiKey.RightArrow))
        {
            _sceneRenderer.OperateCamera(CameraOperation.Right);
        }
        if (ImGui.IsKeyDown(ImGuiKey.LeftArrow))
        {
            _sceneRenderer.OperateCamera(CameraOperation.Left);
        }
        if (ImGui.IsKeyDown(ImGuiKey.DownArrow))
        {
            _sceneRenderer.OperateCamera(CameraOperation.Down);
        }
        if (ImGui.IsKeyDown(ImGuiKey.UpArrow))
        {
            _sceneRenderer.OperateCamera(CameraOperation.Up);
        }
        if (ImGui.IsKeyPressed(ImGuiKey.PageUp))
        {
            _sceneRenderer.OperateCamera(CameraOperation.ZoomIn);
        }
        if (ImGui.IsKeyPressed(ImGuiKey.PageDown))
        {
            _sceneRenderer.OperateCamera(CameraOperation.ZoomOut);
        }
    }
    public void HandleInputs()
    {
        Vector2 scenePos = _sceneRenderer.GetScenePos(ImGui.GetMousePos());
        _structureEditor.SetLivePos(scenePos);
        if (_sceneRenderer.SceneWindowHovered)
        {
            HandleCameraMovement();

            if (_currentOperation != OperationMode.Editor) return;
            _structureEditor.UpdateHoveredItems();
            HandleToolSwitchInputs();
            if (ImGui.IsKeyPressed(ImGuiKey.MouseRight))
            {
                HandleRightClickPopupDisplay();
            }
            
            if (ImGui.IsKeyDown(ImGuiKey.MouseLeft))
            {
                _structureEditor.HandleMouseKeyDownEvent();
            }
            else if (ImGui.IsKeyReleased(ImGuiKey.MouseLeft))
            {
                _structureEditor.HandleMouseKeyUpEvent();
            }
            if (ImGui.IsKeyPressed(ImGuiKey.MouseLeft))
            {
                _structureEditor.HandleMouseKeyPressedEvent();
            }

            if (ImGui.IsKeyPressed(ImGuiKey.Delete))
            {
                //todo delete confirmation?
                _structureEditor.DeleteSelectedElements();
                _structureEditor.DeleteSelectedNodes();
            }

            
        }

    }
    public void HandlePopups()
    {
        if (StructureLoaded)
        {
            _structureEditor.HandlePopups();
            _structureSolver.HandlePopups();
        }

        DefineNewProjectModal();
        DefineOpenProjectModal();
        DefineSaveProjectAsModal();
        //DefinePreferencesModal();

        //todo popup flag names
        // if (ShouldShowPreferencesModal)
        // {
        //     ImGui.OpenPopup("Preferences");
        //     ShouldShowPreferencesModal = false;
        // }

        if (_shouldShowNewProjectModal)
        {
            ImGui.OpenPopup("New Project");
            _shouldShowNewProjectModal = false;
        }

        if (_shouldShowOpenProjectModal)
        {
            ImGui.OpenPopup("Open Project");
            _shouldShowOpenProjectModal = false;
        }

        if (_shouldShowSaveProjectAsModal)
        {
            ImGui.OpenPopup("Save Project As");
            _shouldShowSaveProjectAsModal = false;
        }
    }
    private void HandleToolSwitchInputs()
    {
        if (ImGui.IsKeyPressed(_settings.AddNodeToolKey))
        {
            _structureEditor.SwitchTool(Tool.AddNode);
        }
        else if (ImGui.IsKeyPressed(_settings.AddElementToolKey))
        {
            _structureEditor.SwitchTool(Tool.AddElement);
        }
        else if (ImGui.IsKeyPressed(_settings.SelectNodesToolKey))
        {
            _structureEditor.SwitchTool(Tool.SelectNodes);
        }
        else if (ImGui.IsKeyPressed(_settings.SelectElementsToolKey))
        {
            _structureEditor.SwitchTool(Tool.SelectElements);
        }
        else if (ImGui.IsKeyPressed(_settings.MouseSelectToolKey))
        {
            _structureEditor.SwitchTool(Tool.MouseSelect);
        }
    }
    private void HandleRightClickPopupDisplay()
    {
        if (_structureEditor.SelectedNodeCount != 0 && _structureEditor.SelectedElementCount == 0)
        {
            ImGui.OpenPopup("SelectedNodePopup");
        }
    }

    //Popups
    const int MaxStringInputLength = 1024;
    
    private string _newProjectModalErrorMessage = string.Empty;
    private string _newProjectModalFilepath = string.Empty;
    private string _newProjectModalProjectName = string.Empty;
    private float _newProjectModalGridSpacingSize;

    private string _saveProjectAsModalFilepath = string.Empty;
    private string _saveProjectAsModalFilename = string.Empty; 
    private string _saveProjectAsModalErrorMessage = string.Empty;
    
    private string _openProjectModalFilePath = string.Empty;
    private string _openProjectModalErrorMessage = string.Empty;
    private void DefineNewProjectModal()
    {
        if (ImGui.BeginPopupModal("New Project", ImGuiWindowFlags.AlwaysAutoResize))
        {
            if (ImGui.BeginTabBar("Structure Type Tab Bar"))
            {
                if (ImGui.BeginTabItem("In-Memory"))
                {
                    ImGui.TextWrapped("Perfect for quick mockups and testing of structures.");
                    ImGui.Separator();
                    ImGui.TextColored(new Vector4(1f, 0f, 0f, 1f), "Warning! Structure cannot be saved!");
                    ImGui.SeparatorText("Grid spacing distance");
                    ImGui.TextWrapped("What distance one grid step will be equal to, in metres");
                    ImGui.InputFloat("Grid Spacing", ref _newProjectModalGridSpacingSize);
                    if (ImGui.Button("Create"))
                    {
                        if (_newProjectModalGridSpacingSize == 0f)
                        {
                            _newProjectModalErrorMessage = "The grid spacing size cannot be zero!";
                        }
                        else
                        {
                            IStructure newStructure = new InMemoryStructure(
                                "In Memory Structure",
                                new StructureSettings(_newProjectModalGridSpacingSize));
                            _volatileStructure = true;
                            _currentStructureFilepath = string.Empty;
                            _newProjectModalErrorMessage = String.Empty;
                            _newProjectModalProjectName = String.Empty;
                            SwitchStructure(newStructure);
                            ImGui.CloseCurrentPopup();
                        }
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Cancel"))
                    {
                        _newProjectModalErrorMessage = String.Empty;
                        _newProjectModalProjectName = String.Empty;
                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.TextWrapped(_newProjectModalErrorMessage);
                    ImGui.EndTabItem();
                }
                if (ImGui.BeginTabItem("Database"))
                {
                    ImGui.SeparatorText("Filepath");
                    ImGui.TextWrapped("The filepath of the project");
                    ImGui.InputText("Path", ref _newProjectModalFilepath, MaxStringInputLength);
                    ImGui.SeparatorText("Project name");
                    ImGui.TextWrapped(
                        "The name of the project. The project will be saved as a .structure file with this name on the given path.");
                    ImGui.InputText("Name", ref _newProjectModalProjectName, MaxStringInputLength);
                    ImGui.SeparatorText("Grid spacing distance");
                    ImGui.TextWrapped("What distance one grid step will be equal to, in metres");
                    ImGui.InputFloat("Grid Spacing", ref _newProjectModalGridSpacingSize);

                    if (ImGui.Button("Create"))
                    {
                        if (!Path.Exists(_newProjectModalFilepath))
                        {
                            _newProjectModalErrorMessage = "The filepath is inaccessible! Check for invalid characters.";
                        }
                        else if (_newProjectModalProjectName == String.Empty)
                        {
                            _newProjectModalErrorMessage = "The project name cannot be blank!";
                        }
                        else if (Path.GetInvalidFileNameChars().Any(_newProjectModalProjectName.Contains))
                        {
                            _newProjectModalErrorMessage =
                                "The project name has invalid characters! Use characters that are valid for a filename.";
                        }
                        else if (File.Exists(_newProjectModalFilepath))
                        {
                            _newProjectModalErrorMessage =
                                "Filepath points to a file! Check and correct filepath to remove extension.";
                        }
                        else if (Path.Exists(Path.Combine(_newProjectModalFilepath,
                                     _newProjectModalProjectName + ".structure")))
                        {
                            _newProjectModalErrorMessage = "This file already exists! Try opening the project instead.";
                        }
                        else if (_newProjectModalGridSpacingSize == 0f)
                        {
                            _newProjectModalErrorMessage = "The grid spacing size cannot be zero!";
                        }
                        else
                        {
                            _currentStructureFilepath = Path.Combine(_newProjectModalFilepath, _newProjectModalProjectName + SaveFileExtension);
                            IStructure newStructure = new DatabaseStructure(
                                _currentStructureFilepath,
                                new StructureSettings(_newProjectModalGridSpacingSize));
                            _volatileStructure = false;
                            _newProjectModalErrorMessage = String.Empty;
                            _newProjectModalProjectName = String.Empty;
                            SwitchStructure(newStructure);
                            ImGui.CloseCurrentPopup();
                        }
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Cancel"))
                    {
                        _newProjectModalErrorMessage = String.Empty;
                        _newProjectModalProjectName = String.Empty;
                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.TextWrapped(_newProjectModalErrorMessage);
                   
                    ImGui.EndTabItem();
                }
                ImGui.EndTabBar();
            }
                
            ImGui.EndPopup();
        }
    }
    private void DefineOpenProjectModal()
    {
        if (ImGui.BeginPopupModal("Open Project", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.SeparatorText("Filepath");
            ImGui.TextWrapped("The full filepath of the project");
            ImGui.InputText("Path", ref _openProjectModalFilePath, MaxStringInputLength);
            if (ImGui.Button("Open"))
            {
                if (!Path.Exists(_openProjectModalFilePath))
                {
                    _openProjectModalErrorMessage = "The filepath is inaccessible! Check for invalid characters.";
                }
                else if (!File.Exists(_openProjectModalFilePath))
                {
                    _openProjectModalErrorMessage = "File doesn't exist! Check path to ensure it points to a valid file.";
                }
                else if (!DatabaseStructure.FileIsSqliteDatabase(_openProjectModalFilePath))
                {
                    _openProjectModalErrorMessage = "The file is invalid! Might be corrupted.";
                }
                else
                {
                    IStructure newStructure = new DatabaseStructure(_openProjectModalFilePath);
                    _currentStructureFilepath = _openProjectModalFilePath;
                    _openProjectModalFilePath = String.Empty;
                    _openProjectModalErrorMessage = String.Empty;
                    SwitchStructure(newStructure);
                    ImGui.CloseCurrentPopup();
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                _openProjectModalFilePath = String.Empty;
                _openProjectModalErrorMessage = String.Empty;
                ImGui.CloseCurrentPopup();
            }
            ImGui.TextWrapped(_openProjectModalErrorMessage);
            ImGui.EndPopup();
        }
    }
    private void DefineSaveProjectAsModal()
    {
        if (ImGui.BeginPopupModal("Save Project As", ImGuiWindowFlags.AlwaysAutoResize))
        {
            
            ImGui.SeparatorText("Filepath");
            ImGui.TextWrapped("Save location");
            ImGui.InputText("Path", ref _saveProjectAsModalFilepath, MaxStringInputLength);
            ImGui.SeparatorText("Filename");
            ImGui.TextWrapped(
                "The new name of the file.");
            ImGui.InputText("Name", ref _saveProjectAsModalFilename, MaxStringInputLength);
            if (ImGui.Button("Save"))
            {
                if (!Path.Exists(_saveProjectAsModalFilepath))
                {
                    _saveProjectAsModalErrorMessage = "The filepath is inaccessible! Check for invalid characters.";
                }
                else if (_saveProjectAsModalFilename == String.Empty)
                {
                    _saveProjectAsModalErrorMessage = "The project name cannot be blank!";
                }
                else if (Path.GetInvalidFileNameChars().Any(_saveProjectAsModalFilename.Contains))
                {
                    _saveProjectAsModalFilepath =
                        "The project name has invalid characters! Use characters that are valid for a filename.";
                }
                else if (File.Exists(_saveProjectAsModalFilepath))
                {
                    _saveProjectAsModalErrorMessage =
                        "Filepath points to a file! Remove the file from the file path.";
                }
                else if (File.Exists(Path.Combine(_saveProjectAsModalFilepath, _saveProjectAsModalFilename + SaveFileExtension)))
                {
                    _saveProjectAsModalErrorMessage = "There is a file with the same name in the given path!";
                }
                else
                {
                    File.Copy(_currentStructureFilepath, Path.Combine(_saveProjectAsModalFilepath, _saveProjectAsModalFilename + SaveFileExtension));
                    _saveProjectAsModalFilepath = String.Empty;
                    _saveProjectAsModalErrorMessage = String.Empty;
                    ImGui.CloseCurrentPopup();
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                _saveProjectAsModalFilepath = String.Empty;
                _saveProjectAsModalErrorMessage = String.Empty;
                ImGui.CloseCurrentPopup();
            }
            ImGui.TextWrapped(_saveProjectAsModalErrorMessage);
            ImGui.EndPopup();
        }
    }
    
    // TO.DO rename properties to be better
    // Popup Properties
    //---------
    // private Vector4 sceneElementColor = new();
    // private Vector4 sceneNodeColor = new();
    // private Vector4 sceneSelectedElementColor = new();
    // private Vector4 sceneSelectedNodeColor = new();
    // private Vector4 sceneHoveredElementColor = new();
    // private Vector4 sceneHoveredNodeColor = new();
    // private Vector4 sceneSelectionBoxColor = new();
    // private float sceneElementThickness;
    // private float sceneNodeRadius;
    //
    // //to.do finish scenedrawsettings, add note saying please put something for all
    // private void DefineSceneDrawSettings()
    // {
    //     ImGui.ColorEdit4("Element Color", ref sceneElementColor);
    //     ImGui.ColorEdit4("Node Color", ref sceneNodeColor);
    //     ImGui.ColorEdit4("Selected Element Color", ref sceneSelectedElementColor);
    //     ImGui.ColorEdit4("Selected Node Color", ref sceneSelectedNodeColor);
    //     ImGui.ColorEdit4("Hovered Element Color", ref sceneHoveredElementColor);
    //     ImGui.ColorEdit4("Hovered Node Color", ref sceneHoveredNodeColor);
    //     ImGui.ColorEdit4("Selection Box Color", ref sceneSelectionBoxColor);
    //     ImGui.DragFloat("Element Thickness", ref sceneElementThickness, 0.1f, 1f, 5f);
    //     ImGui.DragFloat("Node Radius", ref sceneNodeRadius, 0.1f, 1f, 5f);
    //     //todo saving and loading userdata on program launch
    //     if (ImGui.Button("Save Settings"))
    //     {
    //         drawSettings = new DrawSettings(
    //             sceneElementColor,
    //             sceneNodeColor,
    //             sceneSelectedElementColor,
    //             sceneSelectedNodeColor,
    //             sceneHoveredElementColor,
    //             sceneHoveredNodeColor,
    //             sceneSelectionBoxColor,
    //             sceneElementThickness,
    //             sceneNodeRadius
    //         );
    //     }
    //
    //     if (ImGui.Button("Reset to defaults"))
    //     {
    //         drawSettings = DrawSettings.Default;
    //     }
    // }
    //
    // private void DefineToolHotkeysSettings()
    // {
    //
    // }
    //
    // private void DefineEditorSettings()
    // {
    //     
    // }
    //
    // private bool ShouldShowPreferencesModal;
    // public void DefinePreferencesModal()
    // {
    //
    //     if (ImGui.BeginPopupModal("Preferences"))
    //     {
    //         if (ImGui.BeginTabBar("PreferencesTabBar"))
    //         {
    //             if (ImGui.BeginTabItem("Scene Draw settings"))
    //             {
    //                 DefineSceneDrawSettings();
    //             }
    //             if (ImGui.BeginTabItem("Tool Hotkeys"))
    //             {
    //                 DefineToolHotkeysSettings();
    //             }
    //
    //             if (ImGui.BeginTabItem("Editor Settings"))
    //             {
    //                 DefineEditorSettings();
    //             }
    //             ImGui.EndTabBar();
    //         }
    //         ImGui.Separator();
    //         if (ImGui.Button("Close"))
    //         {
    //             ImGui.CloseCurrentPopup();
    //         }
    //         ImGui.EndPopup();
    //     }
    // }
}