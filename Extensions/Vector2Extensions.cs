using System.Numerics;

namespace SimpleFEM.Extensions;

public static class Vector2Extensions
{
    public static float DistanceToLineSegment(this System.Numerics.Vector2 pos, System.Numerics.Vector2 linePos1, System.Numerics.Vector2 linePos2)
    {
        //using dot product formula to figure out "amount of position vector" in the direction of the line
        float t = System.Numerics.Vector2.Dot(pos- linePos1, linePos2 - linePos1) / System.Numerics.Vector2.DistanceSquared(linePos1, linePos2);
        
        //if t > 1, that means the amount of position vector in direction of line is more than line itself
        // so closest is the point we didn't use to get the position
        //if t < 0 that means the amount of position vector in direction is negative
        // so closest is the point we did use to get the position
        //otherwise, closest point lies somewhere within the line and t gives us the % of where it is with relation to length of line
        if (t > 0 && t < 1)
        {
            System.Numerics.Vector2 closesPointOnSegment = linePos1 + t * (linePos2 - linePos1);
            return System.Numerics.Vector2.Distance(closesPointOnSegment, pos);
        }
        return float.MaxValue;
        
    }
    public static bool LiesWithinRect(this System.Numerics.Vector2 vector, System.Numerics.Vector2 pos1, System.Numerics.Vector2 pos2)
    {
        if (pos1 == pos2)
        {
            return false;
        }
        float maxX = Math.Max(pos1.X, pos2.X);
        float minX = Math.Min(pos1.X, pos2.X);
        float maxY = Math.Max(pos1.Y, pos2.Y);
        float minY = Math.Min(pos1.Y, pos2.Y);
        
        return vector.X >= minX && vector.X <= maxX && vector.Y >= minY && vector.Y <= maxY;
    }
    public static System.Numerics.Vector2 Round(this System.Numerics.Vector2 vector)
    {
        return new(
            MathF.Round(vector.X),
            MathF.Round(vector.Y)
        );
    }

    public static System.Numerics.Vector2 RoundToNearest(this System.Numerics.Vector2 vector, float value)
    {
        // this is doing: 
        // Round(vector / value) * value
        return System.Numerics.Vector2.Multiply(Round(System.Numerics.Vector2.Divide(vector, value)), new System.Numerics.Vector2(value));
    }
    public static System.Numerics.Vector2 Floor(this System.Numerics.Vector2 vector)
    {
        return new System.Numerics.Vector2(
            MathF.Floor(vector.X),
            MathF.Floor(vector.Y)
            );
    }

    public static (int, int) ToInteger(this System.Numerics.Vector2 vector)
    {
        System.Numerics.Vector2 roundedVec = Round(vector);
        return new((int)roundedVec.X, (int)roundedVec.Y);
    }
}