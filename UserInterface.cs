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
    private DrawSettings drawSettings;
    private HotkeySettings settings;
    
    public UserInterface(IStructure structure, HotkeySettings settings)
    {
        structureSolver = new UIStructureSolver(structure);
        structureEditor = new UIStructureEditor(structure, StructureEditorSettings.Default);
        drawSettings = DrawSettings.Default;
        sceneRenderer = new UISceneRenderer();
        this.settings = settings;
    }

    public void DrawSceneWindow()
    {
        sceneRenderer.SetRenderQueue(structureEditor.GetSceneObjects(drawSettings));
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
            string mousePosition = sceneRenderer.GetScenePos(ImGui.GetMousePos()).ToString();
            ImGui.SameLine(width - ImGui.CalcTextSize(mousePosition).X);
            ImGui.Text(mousePosition);
        }

        ImGui.End();
        
        ImGui.PopStyleVar(10);
    }

    public void DrawHoveredNodePropertyViewer()
    {
        ImGui.Begin("Node Property Viewer");
        (Vector2? pos, BoundaryCondition bc, Load l) = structureEditor.GetHoveredNodeProperties();
        if (pos == null)
        {
            ImGui.Text("No node hovered!");
            return;
        }

        ImGui.Text($"Pos: {pos.ToString()}");
        ImGui.NewLine();
        ImGui.SeparatorText("Boundary Condition");
        if (!bc.IsDefault)
        {
            if (ImGui.BeginTable("Boundary Conditions", 2, ImGuiTableFlags.Borders))
            {
                ImGui.TableNextColumn();
                ImGui.Text("Fixed X");
                ImGui.TableNextColumn();
                ImGui.Text(bc.FixedX.ToString());
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text("Fixed Y");
                ImGui.TableNextColumn();
                ImGui.Text(bc.FixedY.ToString());
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text("Fixed Rotation");
                ImGui.TableNextColumn();
                ImGui.Text(bc.FixedRotation.ToString());
                
                ImGui.EndTable();
                
            }
        }
        else
        {
            ImGui.Text("Node has no boundary conditions!");
        }

        ImGui.NewLine();
        ImGui.SeparatorText("Load");
        if (!l.IsDefault)
        {
            if (ImGui.BeginTable("Load", 2, ImGuiTableFlags.Borders))
            {
                //TOdo naming
                ImGui.TableNextColumn();
                ImGui.Text("X Load");
                ImGui.TableNextColumn();
                ImGui.Text(l.ForceX.ToString());
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text("Y Load");
                ImGui.TableNextColumn();
                ImGui.Text(l.ForceY.ToString());
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text("Moment");
                ImGui.TableNextColumn();
                ImGui.Text(l.Moment.ToString());
                
                ImGui.EndTable();
                
            }
        }
        else 
        {
            ImGui.Text("Node has no loads!");    
        }
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
    public void DrawSolveSystemWindow()
    {
        // TODO rename
        ImGui.Begin("Solve system window");
        
        if (ImGui.Button("Solve current system"))
        {
            structureSolver.Solve();
            
        }

        if (ImGui.Button("test element intersection"))
        {
            if (structureSolver.CheckElementIntersections())
            {
                Console.WriteLine("there are no intersection");
            }
            else
            {
                Console.WriteLine("there is intersections");
            }
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
                    
                }

                if (ImGui.MenuItem("Add new section"))
                {
                    //todo material handling   
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Options"))
            {
                if (ImGui.MenuItem("Edit keybinds"))
                {
                    //todo keybind editor 
                }

                if (ImGui.MenuItem("Edit colours"))
                {
                    // todo draw-colour editor 
                }
                if (ImGui.MenuItem("Edit selection settings"))
                {
                    //todo selection settings editor
                }
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

        structureEditor.MaterialSelectComboBox();
        structureEditor.SectionSelectComboBox();
    }
    //Input Handling
    public void HandleInputs()
    {
        Vector2 scenePos = sceneRenderer.GetScenePos(ImGui.GetMousePos());
        structureEditor.SetLivePos(scenePos);
        if (sceneRenderer.SceneWindowHovered)
        {
            structureEditor.UpdateHoveredItems();
            HandleToolSwitchInputs();
            if (ImGui.IsKeyPressed(ImGuiKey.MouseRight))
            {
                HandlePopupDisplay();
            }
            
            
            if (structureEditor.CurrentTool == Tool.Move && structureEditor.Dragging)
            {
                //todo make this not make me want to kill myself
                sceneRenderer.MoveCameraTarget(structureEditor.GetMousePositionChange());
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
    public void HandleToolSwitchInputs()
    {
        if (ImGui.IsKeyPressed(settings.MoveToolKey))
        {
            structureEditor.SwitchTool(Tool.Move);
        }
        else if (ImGui.IsKeyPressed(settings.AddNodeToolKey))
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

    public void HandlePopupDisplay()
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
    //Load Editor
    private float loadX, loadY, moment;
    //Boundary Condition Editor
    private bool fixedX, fixedY, fixedMoment;
    //---------
    //Flags
    private bool openBCEditor, openLoadEditor;
    public void DefineAllPopups()
    {
        DefineLoadEditorModal();
        DefineBoundaryConditionEditorModal();
        DefineSelectedNodePopup();
        if (openBCEditor)
        {
            ImGui.OpenPopup("Boundary Condition Editor");
            openBCEditor = false;
        }

        if (openLoadEditor)
        {
            ImGui.OpenPopup("Load Editor");
            openLoadEditor = false;
        }
    }

    public void DefineSelectedNodePopup()
    {
        if (ImGui.BeginPopup("SelectedNodePopup"))
        {
            if (ImGui.Selectable("Delete node(s)"))
            {
                structureEditor.DeleteSelectedNodes();
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.Selectable("Edit node load(s)"))
            {
                ImGui.CloseCurrentPopup();
                openLoadEditor = true;
            }

            if (ImGui.Selectable("Edit node boundary condition(s)"))
            {
                ImGui.CloseCurrentPopup();
                openBCEditor = true;
            }
            ImGui.EndPopup();
        }
    }


    private Vector4 sceneElementColor = new();
    private Vector4 sceneNodeColor = new();
    private Vector4 sceneSelectedElementColor = new();
    private Vector4 sceneSelectedNodeColor = new();
    private Vector4 sceneHoveredElementColor = new();
    private Vector4 sceneHoveredNodeColor = new();
    private Vector4 sceneSelectionBoxColor = new();
    private float sceneElementThickness;
    private float sceneNodeRadius;
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
        ImGui.GetIO();


    }

    private void DefineEditorSettings()
    {
        
    }
    private bool DisplaySettingsEditorWindow = true;

    public void DefineSettingsEditorWindow()
    {
        if (!DisplaySettingsEditorWindow) return;

        ImGui.Begin("Settings Editor");
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

        ImGui.End();
    }
    public void DefineLoadEditorModal()
    {
        if (ImGui.BeginPopupModal("Load Editor", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text($"Editing load for {structureEditor.SelectedNodeCount} node(s)");
            ImGui.InputFloat("Load in X", ref loadX);
            ImGui.InputFloat("Load in Y", ref loadY);
            ImGui.InputFloat("Moment", ref moment);
            if (ImGui.Button("Reset to default"))
            {
                loadX = 0;
                loadY = 0;
                moment = 0;
            }
            ImGui.Separator();
            if (ImGui.Button("Add Load(s)"))
            {
                structureEditor.AddLoadToSelectedNodes(new Load(loadX, loadY, moment));
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
        
    }

    public void DefineBoundaryConditionEditorModal()
    {
        if (ImGui.BeginPopupModal("Boundary Condition Editor"))
        {
            ImGui.Text($"Editing boundary condition for {structureEditor.SelectedNodeCount} node(s)");
            ImGui.Checkbox("Fixed X", ref fixedX);
            ImGui.Checkbox("Fixed Y", ref fixedY);
            ImGui.Checkbox("Fixed Moment", ref fixedMoment);

            if (ImGui.Button("Reset to default"))
            {
                fixedX = false;
                fixedY = false;
                fixedMoment = false;
            }
            ImGui.Separator();
            if (ImGui.Button("Add BC(s)"))
            {
                structureEditor.AddBoundaryConditionToSelectedNodes(new BoundaryCondition(fixedX, fixedY, fixedMoment));
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
    }
}