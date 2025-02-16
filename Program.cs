using System.Numerics;
using ImGuiNET;
using rlImGui_cs;
using Raylib_cs;

namespace SimpleFEM;

class Program
{
    static void Main(string[] args)
    {
        
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        
        Raylib.InitWindow(1600, 900, "SimpleFEM");
        rlImGui.Setup(true, true);
        Raylib.SetTargetFPS(60);
        
        Structure structure = new("Test Structure");
        UserInterface UI = new(structure);
        Scene scene = new Scene(structure);
        
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.RayWhite);
            rlImGui.Begin();
            
            //DRAW EVERYTHING BELOW ME
            
            UI.ShowFooter();
            UI.ShowMainMenuBar();
            
            UI.DrawMainDockSpace();
            
            UI.scene.ShowSceneWindow();
            
            UI.scene.ProcessInputs();

            UI.ShowSimpleEditGUI();
            
            ImGui.ShowDemoWindow();
            //DRAW EVERYTHING ABOVE ME
            
            rlImGui.End();
            Raylib.EndDrawing();
        }
            
        rlImGui.Shutdown();
        Raylib.CloseWindow();
    }
}