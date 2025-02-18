using System.Numerics;
using System.Runtime.Intrinsics;
using Raylib_cs;
using SimpleFEM.Interfaces;

namespace SimpleFEM.SceneObjects;

public class LinesObject : ISceneObject
{
    private Queue<(Vector2, Vector2)> lines;
    private Color color;
    private float thickness;
    public LinesObject(Color color, float thickness)
    {
        lines = new();
        this.color = color;
        this.thickness = thickness;
    }

    public void AddLine(Vector2 pos1, Vector2 pos2)
    {
        lines.Enqueue((pos1, pos2));
    }

    public void Render()
    {
        while (lines.Count > 0)
        {
            (Vector2 position1, Vector2 position2) line = lines.Dequeue();
            Raylib.DrawLineEx(line.position1, line.position2, thickness, color);
        }
    }
}