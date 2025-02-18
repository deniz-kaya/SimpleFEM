using System.Numerics;
using ImGuiNET;
using SimpleFEM.Derived;
using SimpleFEM.Interfaces;
using SimpleFEM.Types;

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

    // TODO REMOVE
    private Vector2 pos = Vector2.Zero;
    public void TestThing()
    {
        ImGui.Begin("AddNode");
        ImGui.SliderFloat2("x", ref pos, -500f, 500f);
        if (ImGui.Button("AddNode"))
        {
            structureEditor.AddeNode(pos);
        }
        ImGui.End();
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
    public void HandleInputs()
    {
        HandleToolSwitchInputs();
        
        if (sceneRenderer.SceneWindowHovered)
        {
            
            Vector2 scenePos = sceneRenderer.GetScenePos(ImGui.GetMousePos());
            structureEditor.IdleSelection(scenePos);
            if (ImGui.IsKeyDown(ImGuiKey.MouseLeft))
            {
                structureEditor.HandleMouseKeyDownEvent(scenePos);
            }
            else if (ImGui.IsKeyReleased(ImGuiKey.MouseLeft))
            {
                structureEditor.HandleMouseKeyUpEvent(scenePos);
            }
            else if (ImGui.IsKeyPressed(ImGuiKey.MouseLeft))
            {
                structureEditor.HandleMouseKeyPressedEvent(scenePos);
            }

            if (ImGui.IsKeyPressed(ImGuiKey.Delete))
            {
                structureEditor.DeleteSelectedElements();
            }
        }
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
    public void DrawMainDockspace()
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
    
}