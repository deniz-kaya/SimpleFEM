using System.Numerics;

namespace SimpleFEM.Types.StructureTypes;

public struct Node
{
    public Node(Vector2 position)
    {
        Pos = position;
    }
    public Vector2 Pos;

}