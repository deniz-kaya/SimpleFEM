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
        
        Data.Structure structure = new("Test Structure");
        UserInterface userInterface = new(structure);
        
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            rlImGui.Begin();
            //DRAW EVERYTHING BELOW ME
            userInterface.ShowSimpleEditGUI();
            
            
            ImGui.ShowDemoWindow();
            //DRAW EVERYTHING ABOVE ME
            rlImGui.End();
            Raylib.EndDrawing();
        }
            
        rlImGui.Shutdown();
        Raylib.CloseWindow();
    }
}