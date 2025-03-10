using System.Numerics;
using Raylib_cs;
using SimpleFEM.Interfaces;

namespace SimpleFEM.SceneObjects;
public class SphereObject : ISceneObject
{
    private readonly Vector2 _pos;
    private readonly float _radius;
    private readonly Color _color;
    public SphereObject(Vector2 pos, Color color, float radius)
    {
        _pos = pos;
        _color = color;
        _radius = radius;
    }

    public void Render()
    {
        Raylib.DrawCircleV(_pos, _radius, _color);
    }
}