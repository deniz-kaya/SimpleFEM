using System.Numerics;
using Raylib_cs;
using SimpleFEM.Interfaces;

namespace SimpleFEM.SceneObjects;

public class SpheresObject : ISceneObject
{
    private Queue<Vector2> spheres;
    private float radius;
    private Color color;
    public SpheresObject(Color color, float radius)
    {
        spheres = new();
        this.color = color;
        this.radius = radius;
    }

    public void AddSphere(Vector2 position)
    {
        spheres.Enqueue(position);
    }

    public void Render()
    {
        while (spheres.Count > 0)
        {
            Raylib.DrawCircleV(spheres.Dequeue(), radius, color);
        }
    }
    
}