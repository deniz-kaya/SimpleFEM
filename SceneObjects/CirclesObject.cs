using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using SimpleFEM.Interfaces;

namespace SimpleFEM.SceneObjects;

public class CirclesObject : ISceneObject
{
    private Queue<Vector2> _spheres;
    private readonly float _radius;
    private readonly Color _color;
    public CirclesObject(Color color, float radius)
    {
        _spheres = new();
        _color = color;
        _radius = radius;
    }

    public void AddCircle(Vector2 position)
    {
        _spheres.Enqueue(position);
    }

    public void Render()
    {
        while (_spheres.Count > 0)
        {
            Raylib.DrawCircleV(_spheres.Dequeue(), _radius, _color);
        }
    }
    
}