using System.Numerics;
using Raylib_cs;
using SimpleFEM.Extensions;

namespace SimpleFEM.Types.Settings;

public struct DrawSettings
{
    public Color elementColor;
    public Color nodeColor;
    public Color selectedElementColor;
    public Color selectedNodeColor;
    public Color hoveredElementColor;
    public Color hoveredNodeColor;
    public Color selectionBoxColor;
    public float elementThickness;
    public float nodeRadius;
    
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
        this.elementColor = RaylibExtensions.Vector4ToColor(elementColor);
        this.nodeColor = RaylibExtensions.Vector4ToColor(nodeColor);
        this.selectedElementColor = RaylibExtensions.Vector4ToColor(selectedElementColor);
        this.selectedNodeColor = RaylibExtensions.Vector4ToColor(selectedNodeColor);
        this.hoveredElementColor = RaylibExtensions.Vector4ToColor(hoveredElementColor);
        this.hoveredNodeColor = RaylibExtensions.Vector4ToColor(hoveredNodeColor);
        this.selectionBoxColor = RaylibExtensions.Vector4ToColor(selectionBoxColor);
        this.elementThickness = elementThickness;
        this.nodeRadius = nodeRadius;
    }
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
                hoveredElementColor = Color.Blue,
                hoveredNodeColor = Color.Blue,
                selectionBoxColor = new Color(199,199,199,40),
                elementThickness = 3f,
                nodeRadius = 3f,
            };
        }
    }
}