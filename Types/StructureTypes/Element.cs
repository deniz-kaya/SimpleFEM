namespace SimpleFEM.Types.StructureTypes;

public struct Element
{
    public int Node1ID;
    public int Node2ID;
    public int MaterialID;
    public int SectionID;

    public Element(int node1Id, int node2Id, int materialID, int sectionID)
    {
        Node1ID = node1Id;
        Node2ID = node2Id;
        MaterialID = materialID;
        SectionID = sectionID;
    }
}