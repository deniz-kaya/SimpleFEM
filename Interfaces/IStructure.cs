using System.Numerics;
using SimpleFEM.Types.Settings;
using SimpleFEM.Types.StructureTypes;

namespace SimpleFEM.Interfaces;
public interface IStructure
{
    public string GetName();
    public bool AddElement(Element element);
    public bool AddElement(Element element, out int index);
    public bool AddNode(Vector2 pos);
    public bool AddNode(Vector2 pos, out int index);
    public void RemoveElement(int elementID);
    public void RemoveNode(int nodeID);
    public Element GetElement(int elementID);
    public Node GetNode(int nodeID);
    public void SetBoundaryCondition(int nodeID, BoundaryCondition boundaryCondition);
    public void SetLoad(int nodeID, Load load);
    public BoundaryCondition GetBoundaryCondition(int nodeID);
    public Load GetLoad(int nodeID);
    public List<int> GetNodeIndexesSorted();
    public List<int> GetElementIndexesSorted();
    public StructureSettings GetStructureSettings();
    public int GetNodeCount();
    public int GetElementCount();
    public int GetLoadCount();
    public int GetBoundaryConditionCount();
    public bool ValidNodeID(int nodeID);
    //todo material with this description already exists!
    public void AddMaterial(Material mat);
    public void AddSection(Section sect);
    public List<int> GetMaterialIndexesSorted();
    public List<int> GetSectionIndexesSorted();
    public Section GetSection(int sectionID);
    public Material GetMaterial(int materialID);
    public bool ValidElementID(int elementID);

}