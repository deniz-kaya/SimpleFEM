using System.Numerics;
using System.Runtime.CompilerServices;
using ImGuiNET;
using rlImGui_cs;
using Raylib_cs;
using SimpleFEM.Base;
using SimpleFEM.Interfaces;
using SimpleFEM.Types;
using SimpleFEM.Types.StructureTypes;
using Material = Raylib_cs.Material;

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

        IStructure structure = new InMemoryStructure("test structure", new StructureSettings() {gridSpacing =  50f});
        
        //STRUCTURE SETUP
        structure.AddNode(new Vector2(0f,0f));
        structure.AddNode(new Vector2(0f,50f));
        structure.AddNode(new Vector2(100f,0f));
        structure.AddElement(new Element(0, 1));
        structure.AddElement(new Element(0, 2));
        structure.AddElement(new Element(1, 2));
        //
        UserInterface ui = new UserInterface(structure, UserSettings.Default);

        Vector2 pos = Vector2.Zero;
        
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
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
           
            //hello
            ui.HandleInputs();
            
            //DRAW EVERYTHING ABOVE ME
            
            rlImGui.End();
            Raylib.EndDrawing();
        }
            
        rlImGui.Shutdown();
        Raylib.CloseWindow();
    }
}