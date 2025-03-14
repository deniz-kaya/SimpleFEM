using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using SimpleFEM.Base;
using SimpleFEM.Types.Settings;

namespace SimpleFEM.Derived;
public class UISceneRenderer : SceneRenderer
{
    //constant values for the grid spacing
    public const int ScenePixelGridSpacing = 50;
    public const int SceneGridSlices = 200;
    public UISceneRenderer(SceneRendererSettings settings) : base(settings)
    {
        
    }
    public bool SceneWindowHovered { get; private set; }
    public Vector2 TextureStartPosition;
    public Vector2 GetScenePos(Vector2 screenPos)
    {
        //use relative position of the window to get the scene position of the mouse pointer
        //Y coordinate flipped as we flip the scene along the X axis when drawing to convert to the traditional coordinate system
        return Raylib.GetScreenToWorld2D((screenPos - TextureStartPosition), Camera) * new Vector2(1f,-1f);
    }
    
    public void ShowSceneWindow()
    {
        ImGui.Begin("Scene Window");
           
        //set flags and window information for later
        SceneWindowHovered = ImGui.IsWindowHovered();
        
        TextureStartPosition = ImGui.GetCursorScreenPos();
        
        //change the texture size according to the current available area in the window
        ProcessTextureSizeChanges(ImGui.GetContentRegionAvail());
        
        //render the texture in the imgui window
        rlImGui.ImageRenderTexture(GetSceneTexture(ImGui.GetContentRegionAvail()));
        
        ImGui.End();
    }
}