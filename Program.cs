using System.Numerics;
using System.Runtime.CompilerServices;
using ImGuiNET;
using rlImGui_cs;
using Raylib_cs;
using SimpleFEM.Base;
using SimpleFEM.Extensions;
using SimpleFEM.Interfaces;
using SimpleFEM.Types;
using SimpleFEM.Types.StructureTypes;
using Material = Raylib_cs.Material;

namespace SimpleFEM;

class Program
{
    static void Main(string[] args)
    {
        Matrix6x6 m = new Matrix6x6();
        m[0,0] = 1.0f;
        m[1, 1] = 1f;
        m[2, 2] = 1f;
        m[3, 3] = 1f;
        m[4, 4] = 1f;
        m[5, 5] = 1f;
        Matrix6x6.DebugPrint(8 * Matrix6x6.Identity);
        Matrix6x6 test = new Matrix6x6();
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 6; col++)
            {
                test[row, col] = new Random().NextSingle();
            }
        }
        Matrix6x6.DebugPrint(test);
        
        Matrix6x6.DebugPrint(test * m);
        
    }
    static void aMain(string[] args)
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
            ui.DrawMainDockSpace();
            
            ui.DrawMainMenuBar();
            ui.DrawFooter();
            
            ui.DefineAllPopups();

            ui.DrawSceneWindow();
            
            ui.HandleInputs();
            
            //DRAW EVERYTHING ABOVE ME
            
            rlImGui.End();
            Raylib.EndDrawing();
        }
            
        rlImGui.Shutdown();
        Raylib.CloseWindow();
    }
}