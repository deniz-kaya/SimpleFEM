using System;
using System.Numerics;

namespace SimpleFEM.Extensions;

public static class Vector2Extensions
{
    public static float DistanceToLineSegment(this Vector2 pos, Vector2 linePos1, Vector2 linePos2)
    {
        //using dot product formula to figure out "amount of position vector" in the direction of the line
        float t = Vector2.Dot(pos- linePos1, linePos2 - linePos1) / Vector2.DistanceSquared(linePos1, linePos2);
        
        //if t > 1, that means the amount of position vector in direction of line is more than line itself
        // so closest is the point we didn't use to get the position
        //if t < 0 that means the amount of position vector in direction is negative
        // so closest is the point we did use to get the position
        //otherwise, closest point lies somewhere within the line and t gives us the % of where it is with relation to length of line
        if (t > 0 && t < 1)
        {
            Vector2 closesPointOnSegment = linePos1 + t * (linePos2 - linePos1);
            return Vector2.Distance(closesPointOnSegment, pos);
        }
        return float.MaxValue;
        
    }
    public static bool CheckSegmentIntersections(Vector2 l1, Vector2 l2, Vector2 r1, Vector2 r2)
    {
        //-1 ccw, 0 colinear, 1 cw
        int Orientation(Vector2 a, Vector2 b, Vector2 c)
        {
            float val = (b - c).Cross(a - b);
            if (val == 0) return 0;
            return val > 0 ? 1 : -1; 
        }
        
        int o1 = Orientation(l1, r1, l2);
        int o2 = Orientation(l1, r1, r2);
        int o3 = Orientation(l2, r2, l1);
        int o4 = Orientation(l2, r2, r1);

        //case where one node is shared by two elements
        if (o1 == 0 && (r1 == l1 || r1 == l2)) return false;
        if (o2 == 0 && (l1 == r1 || l1 == r2)) return false;
        if (o3 == 0 && (r2 == l1 || r2 == l2)) return false;
        if (o4 == 0 && (l2 == r1 || l2 == r2)) return false;

        //this is point on the line
        if (o1 == 0 && r1.OnSegment(l1, l2)) return true;
        if (o2 == 0 && l1.OnSegment(r1, r2)) return true;
        if (o3 == 0 && r2.OnSegment(l1, l2)) return true;
        if (o4 == 0 && l2.OnSegment(r1, r2)) return true;


        if ((o2 == 0 || o4 == 0) && l2.OnSegment(r1, r2)) return true;
        
        //point not on the line and no point of intersection therefore segments don't intersect 
        return false;
    }
    public static float Cross(this Vector2 left, Vector2 right)
    {
        //cross product
        return left.X * right.Y + left.Y * right.X;
    }
    //todo remove or fix onsegment
    public static bool OnSegment(this Vector2 a, Vector2 b, Vector2 c)
    {
        //if cross product of these vectors is 0, the point a is on the segment bc
        if (Cross(b-a, c-a) == 0)
        {
            return true;
        }

        return false;
    }
    public static bool LiesWithinRect(this Vector2 vector, Vector2 pos1, Vector2 pos2)
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
    public static Vector2 Round(this Vector2 vector)
    {
        return new(
            MathF.Round(vector.X),
            MathF.Round(vector.Y)
        );
    }

    public static Vector2 RoundToNearest(this Vector2 vector, float value)
    {
        // this is doing: 
        // Round(vector / value) * value
        return Vector2.Multiply(Round(Vector2.Divide(vector, value)), new Vector2(value));
    }
    public static Vector2 Floor(this Vector2 vector)
    {
        return new Vector2(
            MathF.Floor(vector.X),
            MathF.Floor(vector.Y)
            );
    }

    public static (int, int) ToInteger(this Vector2 vector)
    {
        Vector2 roundedVec = Round(vector);
        return new((int)roundedVec.X, (int)roundedVec.Y);
    }
}