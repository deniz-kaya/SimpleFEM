using System.Numerics;
using SimpleFEM.Types;
using SimpleFEM.Types.Settings;

namespace SimpleFEM.Interfaces;
using SimpleFEM.Types.StructureTypes;
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
    public BoundaryCondition GetBoundaryCondition(int nodeID);
    public Load GetLoad(int nodeID);
    
    public List<int> GetNodeIndexesSorted();
    public List<int> GetElementIndexesSorted();
    public StructureSettings GetStructureSettings();
    public int GetNodeCount();
    public int GetElementCount();

}