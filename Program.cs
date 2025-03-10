using System.Numerics;
using System.Runtime.CompilerServices;
using ImGuiNET;
using rlImGui_cs;
using Raylib_cs;
using Material = SimpleFEM.Types.StructureTypes.Material;
using SimpleFEM.Base;
using SimpleFEM.Extensions;
using SimpleFEM.Interfaces;
using SimpleFEM.LinearAlgebra;
using SimpleFEM.Types.Settings;
using SimpleFEM.Types.StructureTypes;
using Vector = SimpleFEM.LinearAlgebra.Vector;
using Microsoft.Data.Sqlite;

namespace SimpleFEM;

class Program
{
    static void aMain(string[] args)
    {
        Matrix m = new Matrix(5, 5);
        m[0, 0] = 1;
        m[0, 1] = 2;
        m[0, 2] = 3;
        m[0, 3] = 9;
        m[0, 4] = 4;
        m[1, 0] = 5;
        m[1, 1] = 6;
        m[1, 2] = 7;
        m[1, 3] = 8;
        m[1, 4] = 1;
        m[2, 0] = 3;
        m[2, 1] = 2;
        m[2, 2] = 3;
        m[2, 3] = 2;
        m[2, 4] = 8;
        m[3, 0] = 3;
        m[3, 1] = 1;
        m[3, 2] = 7;
        m[3, 3] = 8;
        m[3, 4] = 2;
        m[4, 0] = 4;
        m[4, 1] = 1;
        m[4, 2] = 5;
        m[4, 3] = 6;
        m[4, 4] = 7;

        Vector v = new Vector(5);
        v[0] = 3;
        v[1] = 4;
        v[2] = 5;
        v[3] = 6;
        v[4] = 195;
        
        Vector z = LinAlgMethods.Solve(m, v);
        z.DebugPrint();
    }
    static void Main(string[] args)
    {
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        
        Raylib.InitWindow(1600, 900, "SimpleFEM");
        Raylib.SetExitKey(KeyboardKey.Null);
        rlImGui.Setup(true, true);
        Raylib.SetTargetFPS(60);
        bool databaseStructure = true;
        IStructure structure;
        if (databaseStructure)
        {
            structure = new DatabaseStructure(
                @"C:\Users\blind\RiderProjects\SimpleFEM\SimpleFEM\DBs",
                "testStructure",
                new StructureSettings() { gridSpacing = 0.50f });
        }
        else
        {
            structure = new InMemoryStructure("test structure", new StructureSettings() {gridSpacing =  0.50f});

            //STRUCTURE SETUP
            // Material mat = Material.Steel;
            // Section sect = Section.UB;
            //
            structure.AddNode(new Vector2(0f, 0f));
            structure.AddNode(new Vector2(0f, 50f));
            structure.AddNode(new Vector2(100f, 0f));
            structure.AddElement(new Element(1, 2, 0, 0));
            structure.AddElement(new Element(1, 3, 0, 0));
            structure.AddElement(new Element(2, 3, 0, 0));
            // //
        }

        StructureSolver solver = new StructureSolver(structure); 
        
        UserInterface ui = new UserInterface(structure, HotkeySettings.Default);
        
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            rlImGui.Begin();
            Raylib.ClearBackground(Color.Black);
            //DRAW EVERYTHING BELOW ME
            ui.DrawMainDockSpace();
            
            ui.DrawMainMenuBar();
            
            ui.DrawFooter();
            ui.DrawToolbar();
            ui.HandlePopups();

            ui.DrawStructureOperationWindow();
            ui.DrawSceneWindow();
            //ui.DefineSettingsEditorWindow();

            ui.HandleInputs();
            ui.DrawHoveredPropertyViewer();
            //DRAW EVERYTHING ABOVE ME
            
            rlImGui.End();
            Raylib.EndDrawing();
        }
            
        rlImGui.Shutdown();
        Raylib.CloseWindow();
    }
}