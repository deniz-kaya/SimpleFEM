using System.Numerics;
using Raylib_cs;
using SimpleFEM.Interfaces;

namespace SimpleFEM.SceneObjects;

public struct LineObject : ISceneObject
{
    private readonly Vector2 _position1;
    private readonly Vector2 _position2;
    private readonly Color _color;
    private readonly float _thickness;
    public LineObject(Vector2 position1, Vector2 position2, Color color, float thickness)
    {
        _position1 = position1;
        _position2 = position2;
        _color = color;
        _thickness = thickness;
    }

    public void Render()
    {
        Raylib.DrawLineEx(_position1, _position2, _thickness, _color);
    }
    
}