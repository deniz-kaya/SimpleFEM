using System.Numerics;
using Raylib_cs;
using SimpleFEM.Extensions;

namespace SimpleFEM.Types.Settings;

public struct DrawSettings
{
    public Color ElementColor;
    public Color NodeColor;
    public Color SelectedElementColor;
    public Color SelectedNodeColor;
    public Color HoveredElementColor;
    public Color HoveredNodeColor;
    public Color SelectionBoxColor;
    public float ElementThickness;
    public float NodeRadius;
    
    
    public static DrawSettings Default =>
        new DrawSettings()
        {
            ElementColor = Color.Green,
            NodeColor = Color.Red,
            SelectedElementColor = Color.Orange,
            SelectedNodeColor = Color.Orange,
            HoveredElementColor = Color.Blue,
            HoveredNodeColor = Color.Blue,
            SelectionBoxColor = new Color(199,199,199,40),
            ElementThickness = 3f,
            NodeRadius = 3f,
        };
}