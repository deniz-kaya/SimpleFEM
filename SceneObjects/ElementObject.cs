using System.Numerics;
using Raylib_cs;

namespace SimpleFEM.SceneObjects;

public struct ElementObject : ISceneObject
{
    private Vector2 position1;
    private Vector2 position2;
    private Color color;
    private float thickness;
    public ElementObject(Vector2 position1, Vector2 position2, Color color, float thickness)
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