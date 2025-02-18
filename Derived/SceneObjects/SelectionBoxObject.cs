using System.Numerics;
using Raylib_cs;
using SimpleFEM.Interfaces;
using SimpleFEM.Extensions;

namespace SimpleFEM.SceneObjects;

public class SelectionBoxObject : ISceneObject
{
    private Vector2 position1;
    private Vector2 position2;
    private Color color;
    public SelectionBoxObject(Vector2 position1, Vector2 position2, Color color)
    {
        this.position1 = position1;
        this.position2 = position2;
        this.color = color;
    }
    public void Render()
    {
        Raylib.DrawRectangleRec(RectangleExtensions.GetRectangleFromPoints(position1, position2), color);
    }
}