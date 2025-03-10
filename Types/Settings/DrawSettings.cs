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
    
    public DrawSettings(
        Vector4 elementColor,
        Vector4 nodeColor,
        Vector4 selectedElementColor,
        Vector4 selectedNodeColor,
        Vector4 hoveredElementColor,
        Vector4 hoveredNodeColor,
        Vector4 selectionBoxColor,
        float elementThickness,
        float nodeRadius
        )
    {
        ElementColor = elementColor.Vector4ToColor();
        NodeColor = nodeColor.Vector4ToColor();
        SelectedElementColor = selectedElementColor.Vector4ToColor();
        SelectedNodeColor = selectedNodeColor.Vector4ToColor();
        HoveredElementColor = hoveredElementColor.Vector4ToColor();
        HoveredNodeColor = hoveredNodeColor.Vector4ToColor();
        SelectionBoxColor = selectionBoxColor.Vector4ToColor();
        ElementThickness = elementThickness;
        NodeRadius = nodeRadius;
    }
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