using System.Numerics;
using Raylib_cs;
using SimpleFEM.Interfaces;

namespace SimpleFEM.SceneObjects;

public class RingObject : ISceneObject
{
    public RingObject(Vector2 center, float innerRadius, float outerRadius, float startAngle, float endAngle, int segments, Color color)
    {
       _center = center;
       _innerRadius = innerRadius;
       _outerRadius = outerRadius;
       _startAngle = startAngle;
       _endAngle = endAngle;
       _segments = segments;
       _color = color;
    }
    private readonly Vector2 _center;
    private readonly float _innerRadius;
    private readonly float _outerRadius;
    private readonly float _startAngle;
    private readonly float _endAngle;
    private readonly int _segments;
    private readonly Color _color;
    public void Render()
    {
        Raylib.DrawRing(_center, _innerRadius, _outerRadius, _startAngle, _endAngle, _segments, _color);
    }
}