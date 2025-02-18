using System.Numerics;
using Raylib_cs;
using SimpleFEM.Interfaces;

namespace SimpleFEM.SceneObjects;

//most likely never going to use this, but its here
public class CustomLinesObject : ISceneObject
{

    private Queue<(bool custom, Vector2 position1, Vector2 position2)> lines;
    private Queue<(Color color, float thickness)> lineProperties;
    private Color defaultColor;
    private float defaultThickness;
    
    public CustomLinesObject(Color defaultColor, float defaultThickness)
    {
        lines = new();
        lineProperties = new();
        this.defaultColor = defaultColor;
        this.defaultThickness = defaultThickness;
    }

    public void AddElement(Vector2 position1, Vector2 position2, Color? color = null, float? thickness = null)
    {
        if (color is not null || thickness is not null)
        {
            lines.Enqueue((true, position1, position2));
            Color validColor = color ?? defaultColor;
            float validThickness = thickness ?? defaultThickness;
            lineProperties.Enqueue((validColor, validThickness));
        }
        else
        {
            lines.Enqueue((false, position1, position2));
        }
    }
    public void Render()
    {
        while (lines.Count > 0){
            (bool custom, Vector2 position1, Vector2 position2) line = lines.Dequeue();
            if (line.custom)
            {
                (Color color, float thickness) properties = lineProperties.Dequeue();
                Raylib.DrawLineEx(line.position1, line.position2, properties.thickness, properties.color);
            }
            else
            {
                Raylib.DrawLineEx(line.position1, line.position2, defaultThickness, defaultColor);
            }
        }
    }
}