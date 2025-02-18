using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.Metadata;

namespace SimpleFEM;

public struct Node
{
    public Node(Vector2 position, BoundaryCondition boundary = default, Load load = default)
    {
        this.Pos = position;
        BoundaryCondition = boundary;
        Load = load;
    }
    public Vector2 Pos;

    public BoundaryCondition BoundaryCondition;
    public Load Load;

    public void SetBoundaryCondition(BoundaryCondition boundaryCondition)
    {
        BoundaryCondition = boundaryCondition;
    }

    public void SetLoad(Load load)
    {
        Load = load;
    }
}

public struct BoundaryCondition
{
    public BoundaryCondition(bool fixedY, bool fixedX, bool fixedMoment)
    {
        this.FixedY = fixedY;
        this.FixedX = fixedX;
        this.FixedMoment = fixedMoment;
    }
    public bool FixedY;
    public bool FixedX;
    public bool FixedMoment;
}

public struct Material
{
    public Material(double e, double poisson, double density)
    {
        E = e;
        Poisson = poisson;
        Density = density;
    }
    public double E;
    public double Poisson;
    public double Density;

    public static Material Dummy => new Material(1, 1, 1);
    
}

public struct Section
{
    public Section(double i, double a)
    {
        I = i;
        A = a;
    }
    public double I;
    public double A;
    
    public static Section Dummy => new Section(1, 1);

}

public struct Element
{
    public int Node1ID;
    public int Node2ID;
    public Material Material;
    public Section Section;

    public Element(int node1Id, int node2Id, Material elementMaterial = default, Section elelentSection = default)
    {
        Node1ID = node1Id;
        Node2ID = node2Id;
        Material = elementMaterial;
        Section = elelentSection;
    }
}

public struct Load
{
    public Load(double forceX, double forceY, double moment)
    {
        this.ForceX = forceX;
        this.ForceY = forceY;
        this.Moment = moment;
    }
    public double ForceX;
    public double ForceY;
    public double Moment;
}


public abstract class SceneElement
{
    public abstract void Render();
}