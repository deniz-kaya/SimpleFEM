using System.Numerics;
using System.Runtime.CompilerServices;
using ImGuiNET;
using rlImGui_cs;
using Raylib_cs;
using SimpleFEM.Base;
using SimpleFEM.Extensions;
using SimpleFEM.Interfaces;
using SimpleFEM.LinearAlgebra;
using SimpleFEM.Types.Settings;
using SimpleFEM.Types.StructureTypes;
using Vector = SimpleFEM.LinearAlgebra.Vector;


namespace SimpleFEM;

class Program
{
    static void Main(string[] args)
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
        
        Vector z = LinearAlgebra.LinearAlgebra.Solve(v, m);
        z.DebugPrint();
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
        SimpleFEM.Types.StructureTypes.Material mat = SimpleFEM.Types.StructureTypes.Material.Steel;
        SimpleFEM.Types.StructureTypes.Section sect = SimpleFEM.Types.StructureTypes.Section.UB;

        structure.AddNode(new Vector2(0f,0f));
        structure.AddNode(new Vector2(0f,50f));
        structure.AddNode(new Vector2(100f,0f));
        structure.AddElement(new Element(0, 1, mat, sect));
        structure.AddElement(new Element(0, 2, mat, sect));
        structure.AddElement(new Element(1, 2, mat, sect));
        //
        StructureSolver solver = new StructureSolver(structure); 
        solver.GetGlobalStiffnessMatrix().DebugPrint();
        
        
        UserInterface ui = new UserInterface(structure, UserSettings.Default);

        Vector2 pos = Vector2.Zero;
        
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            rlImGui.Begin();
            Raylib.ClearBackground(Color.Black);
            //DRAW EVERYTHING BELOW ME
            ui.DrawMainDockSpace();
            
            ui.DrawMainMenuBar();
            
            ui.DrawFooter();

            ui.DefineAllPopups();

            ui.DrawSolveSystemWindow();
            
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