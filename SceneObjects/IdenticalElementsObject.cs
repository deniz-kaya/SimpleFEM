using System.Numerics;
using System.Runtime.Intrinsics;
using Raylib_cs;

namespace SimpleFEM.SceneObjects;

public class IdenticalElementsObject : ISceneObject
{
    private List<(Vector2, Vector2)> elements;
    private Color color;
    private float thickness;
    public IdenticalElementsObject(Color color, float thickness)
    {
        elements = new();
        this.color = color;
        this.thickness = thickness;
    }

    public void AddElement(Vector2 pos1, Vector2 pos2)
    {
        elements.Add((pos1, pos2));
    }

    public void Render()
    {
        foreach ((Vector2 pos1, Vector2 pos2) e in elements)
        {
            Raylib.DrawLineEx(e.pos1, e.pos2, thickness, color);
        }
    }
}