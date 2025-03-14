using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Intrinsics;
using Raylib_cs;
using SimpleFEM.Interfaces;

namespace SimpleFEM.SceneObjects;

public class LinesObject : ISceneObject
{
    private Queue<(Vector2, Vector2)> _lines;
    private readonly Color _color;
    private readonly float _thickness;
    public LinesObject(Color color, float thickness)
    {
        _lines = new();
        _color = color;
        _thickness = thickness;
    }

    public void AddLine(Vector2 pos1, Vector2 pos2)
    {
        _lines.Enqueue((pos1, pos2));
    }

    public void Render()
    {
        //dequeue all lines and draw them in their positions
        while (_lines.Count > 0)
        {
            (Vector2 position1, Vector2 position2) line = _lines.Dequeue();
            Raylib.DrawLineEx(line.position1, line.position2, _thickness, _color);
        }
    }
}