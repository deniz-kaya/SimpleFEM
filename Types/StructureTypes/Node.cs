using System.Numerics;

namespace SimpleFEM.Types.StructureTypes;

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

    public Node WithBoundaryCondition(BoundaryCondition boundaryCondition)
    {
        return new Node(Pos, BoundaryCondition, Load);
    }

    public Node WithLoad(Load load)
    {
        return new Node(Pos, BoundaryCondition, load);
    }
}