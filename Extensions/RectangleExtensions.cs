using System.Numerics;
using Raylib_cs;

namespace SimpleFEM.Extensions;

public static class RectangleExtensions
{
    public static Raylib_cs.Rectangle GetRectangleFromPoints(System.Numerics.Vector2 point1, System.Numerics.Vector2 point2)
    {
        float posX = MathF.Min(point1.X, point2.X);
        float posY = MathF.Min(point1.Y, point2.Y);
        float width = MathF.Abs(point1.X - point2.X);
        float height = MathF.Abs(point1.Y - point2.Y);
        return new Raylib_cs.Rectangle(posX, posY, width, height);
    }
}
