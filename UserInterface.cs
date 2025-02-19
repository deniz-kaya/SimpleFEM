using System.Numerics;
using System.Xml;
using ImGuiNET;
using SimpleFEM.Derived;
using SimpleFEM.Interfaces;
using SimpleFEM.Types;
using SimpleFEM.Types.StructureTypes;

namespace SimpleFEM;

public class UserInterface
{
    private UIStructureEditor structureEditor;
    private UISceneRenderer sceneRenderer;
    private UserSettings settings;
    
    public UserInterface(IStructure structure, UserSettings settings)
    {
        structureEditor = new UIStructureEditor(structure, StructureEditorSettings.Default);
        sceneRenderer = new UISceneRenderer();
        this.settings = settings;
    }

    public void DrawSceneWindow()
    {
        sceneRenderer.SetRenderQueue(structureEditor.GetSceneObjects(DrawSettings.Default));
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
        ImGui.Text($"Current tool: {structureEditor.CurrentTool.ToString()}");
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
    public void DrawMainMenuBar()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                ImGui.MenuItem("File");
                ImGui.SeparatorText("Test");
                ImGui.Text(DateTime.Now.ToString());
                ImGui.EndMenu();
            }
            ImGui.EndMainMenuBar();
        }
    }
    
    //Input Handling
    public void HandleInputs()
    {
        
        if (sceneRenderer.SceneWindowHovered)
        {
            HandleToolSwitchInputs();
            if (ImGui.IsKeyPressed(ImGuiKey.MouseRight))
            {
                HandlePopupDisplay();
            }
            
            Vector2 scenePos = sceneRenderer.GetScenePos(ImGui.GetMousePos());
            structureEditor.SetLivePos(scenePos);
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
                Console.WriteLine("Mouse key pressed");
                structureEditor.HandleMouseKeyPressedEvent();
            }

            if (ImGui.IsKeyPressed(ImGuiKey.Delete))
            {
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
            structureEditor.SwitchTool(Tool.AddNode);
        }
        else if (ImGui.IsKeyPressed(settings.AddElementToolKey))
        {
            structureEditor.SwitchTool(Tool.AddElement);
        }
        else if (ImGui.IsKeyPressed(settings.SelectNodesToolKey))
        {
            structureEditor.SwitchTool(Tool.SelectNodes);
        }
        else if (ImGui.IsKeyPressed(settings.SelectElementsToolKey))
        {
            structureEditor.SwitchTool(Tool.SelectElements);
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