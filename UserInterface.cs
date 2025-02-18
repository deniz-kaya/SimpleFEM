using System.Numerics;
using ImGuiNET;
using SimpleFEM.Base;
using SimpleFEM.Derived;
using SimpleFEM.Interfaces;
using SimpleFEM.Types;

namespace SimpleFEM;

public class UserInterface
{
    private UIStructureManager structureManager;
    private UISceneRenderer sceneRenderer;
    private UserInterfaceSettings settings;
    
    public UserInterface(IStructure structure, UserInterfaceSettings settings)
    {
        structureManager = new UIStructureManager(structure);
        sceneRenderer = new UISceneRenderer();
        this.settings = settings;
    }

    public void DrawSceneWindow()
    {
        sceneRenderer.SetRenderQueue(structureManager.GetSceneObjects(DrawSettings.Default));
        sceneRenderer.ShowSceneWindow();
    }
    public void DrawFooter()
    {
        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoTitleBar 
                                       | ImGuiWindowFlags.NoCollapse
                                       | ImGuiWindowFlags.NoDocking
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
        ImGui.Text($"Left of the footer");
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