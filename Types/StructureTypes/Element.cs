namespace SimpleFEM.Types.StructureTypes;

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