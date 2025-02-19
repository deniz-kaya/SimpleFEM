using System.Numerics;
using Raylib_cs;
using SimpleFEM.Interfaces;

namespace SimpleFEM.SceneObjects;
public class SphereObject : ISceneObject
{
    private Vector2 pos;
    private float radius;
    private Color color;
    public SphereObject(Vector2 pos, Color color, float radius)
    {
        this.pos = pos;
        this.color = color;
        this.radius = radius;
    }

    public void Render()
    {
        Raylib.DrawCircleV(pos, radius, color);
    }
}