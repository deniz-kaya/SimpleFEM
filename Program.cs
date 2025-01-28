using ImGuiNET;
using rlImGui_cs;
using Raylib_cs;

namespace SimpleFEM;

class Program
{
    static void Main(string[] args)
    {
        Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.VSyncHint | ConfigFlags.ResizableWindow);
        Raylib.InitWindow(1024, 1024, "Test Window");
        rlImGui.Setup(true, true);
        Raylib.SetTargetFPS(60);
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            rlImGui.Begin();
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                     if (ImGui.MenuItem("hellow")) {}
                     
                     ImGui.EndMenu();   
                }

                if (ImGui.BeginMenu("Settings"))
                {
                    
                }
                ImGui.EndMainMenuBar();
            }
            ImGui.ShowDemoWindow();

            rlImGui.End();
            Raylib.EndDrawing();
        }
            
        rlImGui.Shutdown();
        Raylib.CloseWindow();
    }
}