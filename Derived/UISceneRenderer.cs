using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using SimpleFEM.Base;
using SimpleFEM.Types.Settings;

namespace SimpleFEM.Derived;
public class UISceneRenderer : SceneRenderer
{
    public const int ScenePixelGridSpacing = 50;
    public const int SceneGridSlices = 200;
    public UISceneRenderer(SceneRendererSettings settings) : base(settings)
    {
        
    }
    public bool SceneWindowHovered { get; private set; }
    public Vector2 TextureStartPosition;
    public Vector2 GetScenePos(Vector2 screenPos)
    {
        return Raylib.GetScreenToWorld2D((screenPos - TextureStartPosition), Camera) * new Vector2(1f,-1f);
    }
    
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