using System.Numerics;
using Raylib_cs;
using SimpleFEM.Interfaces;
using SimpleFEM.Extensions;

namespace SimpleFEM.SceneObjects;

public class SelectionBoxObject : ISceneObject
{
    private readonly Vector2 _position1;
    private readonly Vector2 _position2;
    private readonly Color _color;
    public SelectionBoxObject(Vector2 position1, Vector2 position2, Color color)
    {
        _position1 = position1;
        _position2 = position2;
        _color = color;
    }
    public void Render()
    {
        Raylib.DrawRectangleRec(RectangleExtensions.GetRectangleFromPoints(_position1, _position2), _color);
    }
}