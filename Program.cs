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
        
        UserInterface ui = new UserInterface();
        
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            rlImGui.Begin();
            Raylib.ClearBackground(Color.Black);
            //DRAW EVERYTHING BELOW ME
            
            ui.DrawMainDockSpace();
            ui.DrawMainMenuBar();

            if (ui.StructureLoaded)
            {
                ui.DrawToolbar();
                ui.DrawStructureOperationWindow();
                ui.DrawSceneWindow();
                ui.HandleInputs();
                ui.DrawHoveredPropertyViewer();
                ui.DrawFooter();

            }
            else
            {
                ui.DrawWelcomeWindow();
            }

            ui.HandlePopups();

            //DRAW EVERYTHING ABOVE ME
            rlImGui.End();
            Raylib.EndDrawing();
        }
            
        rlImGui.Shutdown();
        Raylib.CloseWindow();
    }
}