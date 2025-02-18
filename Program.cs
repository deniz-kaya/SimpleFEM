using System.Runtime.CompilerServices;
using ImGuiNET;
using rlImGui_cs;
using Raylib_cs;
using SimpleFEM.Base;
using SimpleFEM.Interfaces;
using SimpleFEM.Types;

namespace SimpleFEM;

class Program
{
    static void Main(string[] args)
    {
        
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        
        Raylib.InitWindow(1600, 900, "SimpleFEM");
        Raylib.SetExitKey(KeyboardKey.Null);
        rlImGui.Setup(true, true);
        Raylib.SetTargetFPS(60);

        IStructure structure = new InMemoryStructure("test structure");
        UserInterface ui = new UserInterface(structure, new UserInterfaceSettings() {FooterHeight = 30f});
        
        
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.RayWhite);
            rlImGui.Begin();
            
            //DRAW EVERYTHING BELOW ME
            //ImGui.DockSpaceOverViewport();
            ui.DrawMainDockspace();
            
            ui.DrawMainMenuBar();
            ui.DrawFooter();
            ui.DrawSceneWindow();
            
            ImGui.Begin("Test");
            ImGui.Text("It compiled!");
            ImGui.End();
            
            
            
            //DRAW EVERYTHING ABOVE ME
            
            rlImGui.End();
            Raylib.EndDrawing();
        }
            
        rlImGui.Shutdown();
        Raylib.CloseWindow();
    }
}