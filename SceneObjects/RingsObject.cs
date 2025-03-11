using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using SimpleFEM.Interfaces;

namespace SimpleFEM.SceneObjects;

public class RingsObject : ISceneObject
{
    private List<Vector2> _positions;
    private readonly float _innerRadius;
    private readonly float _outerRadius;
    private readonly float _startAngle;
    private readonly float _endAngle;
    private readonly int _segments;
    private readonly Color _color;
    public RingsObject(float innerRadius, float outerRadius, float startAngle, float endAngle, int segments, Color color)
    {
        _positions = new List<Vector2>();
        _innerRadius = innerRadius;
        _outerRadius = outerRadius;
        _startAngle = startAngle;
        _endAngle = endAngle;
        _segments = segments;
        _color = color;
    }

    public void AddRing(Vector2 pos)
    {
        _positions.Add(pos);
    }

    public void Render()
    {
        foreach (Vector2 v in _positions)
        {
            Raylib.DrawRing(v, _innerRadius, _outerRadius, _startAngle, _endAngle, _segments, _color);

        }
    }
}