using System.Numerics;
using Raylib_cs;
using SimpleFEM.Interfaces;

namespace SimpleFEM.SceneObjects;

//most likely never going to use this, but its here
public class CustomLinesObject : ISceneObject
{

    private Queue<(bool custom, Vector2 position1, Vector2 position2)> _lines;
    private Queue<(Color color, float thickness)> _lineProperties;
    private readonly Color _defaultColor;
    private readonly float _defaultThickness;
    
    public CustomLinesObject(Color defaultColor, float defaultThickness)
    {
        _lines = new();
        _lineProperties = new();
        _defaultColor = defaultColor;
        _defaultThickness = defaultThickness;
    }

    public void AddElement(Vector2 position1, Vector2 position2, Color? color = null, float? thickness = null)
    {
        if (color is not null || thickness is not null)
        {
            _lines.Enqueue((true, position1, position2));
            Color validColor = color ?? _defaultColor;
            float validThickness = thickness ?? _defaultThickness;
            _lineProperties.Enqueue((validColor, validThickness));
        }
        else
        {
            _lines.Enqueue((false, position1, position2));
        }
    }
    public void Render()
    {
        while (_lines.Count > 0){
            (bool custom, Vector2 position1, Vector2 position2) line = _lines.Dequeue();
            if (line.custom)
            {
                (Color color, float thickness) properties = _lineProperties.Dequeue();
                Raylib.DrawLineEx(line.position1, line.position2, properties.thickness, properties.color);
            }
            else
            {
                Raylib.DrawLineEx(line.position1, line.position2, _defaultThickness, _defaultColor);
            }
        }
    }
}