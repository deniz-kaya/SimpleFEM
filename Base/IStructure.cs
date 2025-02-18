using System.Numerics;

namespace SimpleFEM;

public interface IStructure
{
    public string GetName();
    public bool AddElement(Element element);
    public bool AddElement(Element element, out int index);
    public bool AddNode(Vector2 pos);
    public bool AddNode(Vector2 pos, out int index);
    public void RemoveElement(int elementID);
    public void RemoveNode(int nodeID);
    public Element GetElement(int ElementID);
    public Node GetNode(int NodeID);
    public void SetBoundaryCondition(int nodeID, BoundaryCondition boundaryCondition);
    public void SetLoad(int nodeID, Load load);
    public void GetBoundaryCondition(int nodeID, out BoundaryCondition boundaryCondition);
    public void GetLoad(int nodeID, out Load load);
    public List<int> GetNodeIndexes();
    public List<int> GetElementIndexes();

}