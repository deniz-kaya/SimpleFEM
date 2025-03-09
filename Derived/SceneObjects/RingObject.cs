using System.Numerics;
using Raylib_cs;
using SimpleFEM.Interfaces;

namespace SimpleFEM.SceneObjects;

public class RingObject : ISceneObject
{
    public RingObject(Vector2 center, float innerRadius, float outerRadius, float startAngle, float endAngle, int segments, Color color)
    {
       this.center = center;
       this.innerRadius = innerRadius;
       this.outerRadius = outerRadius;
       this.startAngle = startAngle;
       this.endAngle = endAngle;
       this.segments = segments;
       this.color = color;
    }
    private Vector2 center;
    private float innerRadius;
    private float outerRadius;
    private float startAngle;
    private float endAngle;
    private int segments;
    private Color color;
    public void Render()
    {
        Raylib.DrawRing(center, innerRadius, outerRadius, startAngle, endAngle, segments, color);
    }
}