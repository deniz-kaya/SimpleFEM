using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.Metadata;

namespace SimpleFEM;

public struct Node
{
    public Node(float x, float y)
    {
        X = x; Y = y;
    }
    public float X;
    public float Y;

    public Vector2 Pos
    {
        get
        {
            return new Vector2(X, Y);
        }
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }
    public float GetDistance(Vector2 pos)
    {
        return Vector2.Distance(this.Pos, pos);
    }
}

public struct BoundaryCondition
{
    public BoundaryCondition(int nodeID, bool fixedY, bool fixedX, bool fixedMoment)
    {
        this.NodeID = nodeID;
        this.FixedY = fixedY;
        this.FixedX = fixedX;
        this.FixedMoment = fixedMoment;
    }
    public int NodeID;
    public bool FixedY;
    public bool FixedX;
    public bool FixedMoment;
}

public struct Material
{
    public double E;
    public double Poisson;
    public double Density;
}

public struct Section
{
    public double I;
    public double A;
}

public class Element
{
    public int Node1Id;
    public int Node2Id;
    public Material ElementMaterial;

    public Element(int node1Id, int node2Id, Material elementMaterial)
    {
        Node1Id = node1Id;
        Node2Id = node2Id;
        ElementMaterial = elementMaterial;
    }
}

public class Load
{
    public Load(int nodeID, double forceX, double forceY, double moment)
    {
        this.NodeID = nodeID;
        this.ForceX = forceX;
        this.ForceY = forceY;
        this.Moment = moment;
    }
    public int NodeID;
    public double ForceX;
    public double ForceY;
    public double Moment;
}

