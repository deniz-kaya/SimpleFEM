using System.Collections;
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
        Raylib.SetExitKey(KeyboardKey.Null);
        rlImGui.Setup(true, true);
        Raylib.SetTargetFPS(60);
        
        Structure structure = new("Test Structure");
        UserInterface UI = new(structure);
        SceneRenderer sceneRenderer = new SceneRenderer(structure);
        
        //TEST STRUCTURE SETUP
        Material mat = new Material();
        structure.AddNode(new Node(new Vector2(0f,0f)));
        structure.AddNode(new Node(new Vector2(0f,50f)));
        structure.AddNode(new Node(new Vector2(100f,0f)));
        structure.AddElement(0, 1, mat);
        structure.AddElement(0, 2, mat);
        structure.AddElement(1, 2, mat);
        //
        
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.RayWhite);
            rlImGui.Begin();
            
            //DRAW EVERYTHING BELOW ME
            ImGui.Begin("Test");
            if (ImGui.BeginPopupModal("ModifyBoundaryCondition", ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("Yippeeee!!");
                
            }
            ImGui.End();
            
            
            UI.ShowFooter();
            UI.ShowMainMenuBar();
            
            UI.DrawMainDockSpace();
            
            UI.SceneRenderer.ShowSceneWindow();
            
            //UI.scene.ProcessInputs();
            //UI.ShowToolBox();

            UI.ShowSimpleEditGUI();
            
            //DRAW EVERYTHING ABOVE ME
            
            rlImGui.End();
            Raylib.EndDrawing();
        }
            
        rlImGui.Shutdown();
        Raylib.CloseWindow();
    }
}