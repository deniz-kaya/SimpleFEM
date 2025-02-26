using Raylib_cs;

namespace SimpleFEM.Types.Settings;

public struct DrawSettings
{
    public Color elementColor;
    public Color nodeColor;
    public Color selectedElementColor;
    public Color selectedNodeColor;
    public Color selectionBoxColor;
    public float elementThickness;
    public float nodeRadius;
    
    public static DrawSettings Default
    {
        get
        {
            return new DrawSettings()
            {
                elementColor = Color.Green,
                nodeColor = Color.Red,
                selectedElementColor = Color.Orange,
                selectedNodeColor = Color.Orange,
                selectionBoxColor = new Color(199,199,199,40),
                elementThickness = 3f,
                nodeRadius = 3f,
            };
        }
    }
}