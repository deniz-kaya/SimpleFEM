using System;
using System.Numerics;
using Raylib_cs;

namespace SimpleFEM.Extensions;

public static class RectangleExtensions
{
    public static Rectangle GetRectangleFromPoints(Vector2 point1, Vector2 point2)
    {
        //convert opposite corner points into top-left coordiante and size of rectangle
        float posX = MathF.Min(point1.X, point2.X);
        float posY = MathF.Min(point1.Y, point2.Y);
        float width = MathF.Abs(point1.X - point2.X);
        float height = MathF.Abs(point1.Y - point2.Y);
        return new Rectangle(posX, posY, width, height);
    }
}
