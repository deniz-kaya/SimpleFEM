using System.Numerics;
using System.Threading.Tasks.Dataflow;
using Raylib_cs;
using ImGuiNET;
using rlImGui_cs;

namespace SimpleFEM;

public class Scene
{
    public Camera2D camera;
    private Vector2 SceneWindowPosition;
    private Vector2 SceneWindowSize;
    private Structure structure;
    public bool SceneWindowHovered;
    
    //settings
    private int gridSpacing = 50;
    public int cameraSpeed = 20;
    
    public Vector2? worldPos
    {
        get => SceneWindowHovered ? GetScreenToScenePos(ImGui.GetMousePos()) : null;
    }
    private bool FirstRender;
    public float CameraZoom
    {
        get => camera.Zoom;
        set => camera.Zoom = value;
    }
    private RenderTexture2D viewTexture;
    public Scene(Structure structure)
    {
        this.structure = structure;
        FirstRender = true;
    }

    public Vector2 GetScreenToScenePos(Vector2 screenPos)
    {
        return Raylib.GetScreenToWorld2D((screenPos - SceneWindowPosition), camera);
        
    }

    public void ShowSceneWindow()
    {
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar;
        
        //optional
        // ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0f,0f));        
        // ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);
        // ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
        ImGui.Begin("Scene Window", flags);
        
        SceneWindowHovered = ImGui.IsWindowHovered();
        
        if (FirstRender)
        {
            SceneWindowPosition = ImGui.GetCursorScreenPos();
            SceneWindowSize = ImGui.GetContentRegionAvail();
            
            camera = new Camera2D(Vector2.Divide(SceneWindowSize,2), Vector2.Zero, 0f, 1f);

            viewTexture = RaylibExtensions.LoadRenderTextureV(SceneWindowSize);

            FirstRender = false;
        }
        else
        {
            SceneWindowPosition = ImGui.GetCursorScreenPos();
            ProcessWindowSizeChanges(ImGui.GetContentRegionAvail());
        }
        RenderSceneToTexture();
        rlImGui.ImageRenderTexture(viewTexture);
        ImGui.PopStyleVar(10);

        ImGui.End();
    }

    private void ProcessWindowSizeChanges(Vector2 newWindowSize)
    {
        if (SceneWindowSize != newWindowSize)
        {
            Raylib.UnloadRenderTexture(viewTexture);
            viewTexture = RaylibExtensions.LoadRenderTextureV(newWindowSize);
            camera.Offset = Vector2.Divide(newWindowSize, 2);
            SceneWindowSize = newWindowSize;
        }
    }

    private void RenderSceneToTexture()
    {
        Raylib.BeginTextureMode(viewTexture);
        Raylib.BeginMode2D(camera);
        
        Vector2 pos = GetScreenToScenePos(ImGui.GetMousePos());

        Raylib.ClearBackground(Color.RayWhite);
            
        DrawGrid(); 
    
        DrawElements();

        DrawNodes();
        
        Raylib.DrawCircleV(pos, 2, Color.Black);
        Raylib.DrawCircleV(pos.RoundToNearest(gridSpacing), 6, Color.Green);
        Raylib.DrawPixelV(pos.RoundToNearest(gridSpacing), Color.Red);
        
        Raylib.EndMode2D();
        Raylib.EndTextureMode();

    }

    
    private void DrawElements()
    {
        foreach (Element e in structure.Elements)
        {
            Vector2 pos1 = structure.Nodes[e.Node1Id].pos;
            Vector2 pos2 = structure.Nodes[e.Node2Id].pos;

            Raylib.DrawLineEx(pos1, pos2, 2, Color.Green);
        }
    }

    public void ProcessInputs()
    {
        if (ImGui.IsKeyDown(ImGuiKey.RightArrow))
            camera.Target += new Vector2(cameraSpeed / camera.Zoom, 0);
        if (ImGui.IsKeyDown(ImGuiKey.LeftArrow))
            camera.Target -= new Vector2(cameraSpeed / camera.Zoom, 0);
        if (ImGui.IsKeyDown(ImGuiKey.DownArrow))
            camera.Target += new Vector2(0, cameraSpeed / camera.Zoom);
        if (ImGui.IsKeyDown(ImGuiKey.UpArrow))
            camera.Target -= new Vector2(0, cameraSpeed / camera.Zoom);
        if (ImGui.IsKeyPressed(ImGuiKey.PageUp))
            camera.Zoom += camera.Zoom + 0.25f > 6 ? 0f : 0.25f;
        if (ImGui.IsKeyPressed(ImGuiKey.PageDown))
            camera.Zoom -= camera.Zoom - 0.25f < 0.1f ? 0f : 0.25f;
    }
    private void DrawNodes()
    {
        foreach (Node n in structure.Nodes)
        {
            Raylib.DrawCircleV(n.pos, 3, Color.Red);
        }
    }
    private void DrawGrid()
    {
        Rlgl.PushMatrix();
        Rlgl.Rotatef(90, 1, 0, 0);
        Raylib.DrawGrid(200,gridSpacing);
        Rlgl.Rotatef(-180,1,0,0);            
        Raylib.DrawGrid(200,gridSpacing);
        Rlgl.PopMatrix();
    }
    public void UpdateCameraTarget(Vector2 target)
    {
        camera.Target = target;
    }
}