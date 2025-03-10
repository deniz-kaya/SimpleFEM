using System.Numerics;
using Raylib_cs;
using SimpleFEM.Interfaces;

namespace SimpleFEM.SceneObjects;

public class RingsObject : ISceneObject
{
    private List<Vector2> positions;
    private float innerRadius;
    private float outerRadius;
    private float startAngle;
    private float endAngle;
    private int segments;
    private Color color;
    public RingsObject(float innerRadius, float outerRadius, float startAngle, float endAngle, int segments, Color color)
    {
        positions = new List<Vector2>();
        this.innerRadius = innerRadius;
        this.outerRadius = outerRadius;
        this.startAngle = startAngle;
        this.endAngle = endAngle;
        this.segments = segments;
        this.color = color;
    }

    public void AddRing(Vector2 pos)
    {
        positions.Add(pos);
    }

    public void Render()
    {
        foreach (Vector2 v in positions)
        {
            Raylib.DrawRing(v, innerRadius, outerRadius, startAngle, endAngle, segments, color);

        }
    }
}