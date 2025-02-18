using System.Numerics;
using Raylib_cs;
using SimpleFEM.Interfaces;

namespace SimpleFEM.SceneObjects;

public struct LineObject : ISceneObject
{
    private Vector2 position1;
    private Vector2 position2;
    private Color color;
    private float thickness;
    public LineObject(Vector2 position1, Vector2 position2, Color color, float thickness)
    {
        this.position1 = position1;
        this.position2 = position2;
        this.color = color;
        this.thickness = thickness;
    }

    public void Render()
    {
        Raylib.DrawLineEx(position1, position1, thickness, color);
    }
    
}