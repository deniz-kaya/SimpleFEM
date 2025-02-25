using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using SimpleFEM.Base;

namespace SimpleFEM.Derived;
public class UISceneRenderer(Vector2 initialSize) : SceneRenderer(initialSize)
{

    public bool SceneWindowHovered { get; private set; }
    public Vector2 TextureStartPosition;
    
    public Vector2 GetScenePos(Vector2 screenPos)
    {
        return Raylib.GetScreenToWorld2D((screenPos - TextureStartPosition), camera) * new Vector2(1f,-1f);
    }

    public UISceneRenderer() : this(new Vector2(100f,100f)) {}

    public void ShowSceneWindow()
    {
        ImGui.Begin("Scene Window");
           
        SceneWindowHovered = ImGui.IsWindowHovered();
        
        TextureStartPosition = ImGui.GetCursorScreenPos();
        
        ProcessTextureSizeChanges(ImGui.GetContentRegionAvail());
        
        rlImGui.ImageRenderTexture(GetSceneTexture(ImGui.GetContentRegionAvail()));
        
        ImGui.End();
    }
}