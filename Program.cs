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
            //clear the background each frame to avoid ghosting when windows are moved
            Raylib.ClearBackground(Color.Black);
            //DRAW EVERYTHING BELOW ME
            
            //these are always drawn
            ui.DrawMainDockSpace();
            ui.DrawMainMenuBar();

            if (ui.StructureLoaded)
            {
                //draw all windows
                ui.DrawToolbar();
                ui.DrawStructureOperationWindow();
                ui.DrawSceneWindow();
                ui.DrawHoveredPropertyViewer();
                ui.DrawFooter();
                ui.HandleInputs();
                //note: order of handle inputs doesnt exactly matter as the UI is redrawn many times a second
                //this means that even though changes might reflect in the next frame
                //the next frame arrives too fast to make a discernable difference
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