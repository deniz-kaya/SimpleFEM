using System.Numerics;

namespace SimpleFEM;
//structure toolbox
//takes structure as input, builds on top of structure
//selected nodes, elements etc are all insidf eof htis 
//it is stored inside of scene
//inputs are processed inside of scene, relevant toolbox methods are called to extract information from structure 
public class StructureToolbox
{
    public enum Tool
    {
        None = 0,
        AddNode = 1,
        AddElement = 2,
        SelectNode = 3,
        SelectElement = 4,
        DeleteSeltected = 5,
        MoveNode = 6,
    }

    public int selectionFeather = 10;
    private Structure structure;
    public Tool CurrentTool = Tool.None;
    public StructureToolbox(Structure structure)
    {
        this.structure = structure;
    }
    List<int> selectedNodes = new List<int>();
    private List<int> selectedElements = new List<int>();
    private Vector2 selectPos1 = Vector2.Zero;
    private Vector2 selectPos2 = Vector2.Zero;
    
    public void ResetState(Tool switchToTool = Tool.None)
    {
        CurrentTool = switchToTool;
        selectPos1 = Vector2.Zero;
        selectPos2 = Vector2.Zero;
        selectedElements.Clear();
        selectedNodes.Clear();
    }

    public void AddElement()
    {
        int node1 = structure.CheckForNodeCollisions(selectPos1);
        int node2 = structure.CheckForNodeCollisions(selectPos2);
        if (node1 == -1)
        {
            structure.AddNode(selectPos1);
            node1 = structure.Nodes.LastAddedIndex;
        }
        if (node2 == -1)
        {
            structure.AddNode(selectPos2);
            node2 = structure.Nodes.LastAddedIndex;
        }
        structure.AddElement(node1, node2, new Material());
    }
    public void DeleteSelectedNodes()
    {
        foreach (int i in selectedNodes)
        {
            structure.RemoveNode(i);
        }
    }

    public void DeleteSelectedElements()
    {
        foreach (int i in selectedElements)
        {
            structure.RemoveElement(i);
        }
    }
    public List<int> GetNodesWithinArea()
    {
        List<int> nodes = new List<int>();
        foreach (int i in structure.Nodes.GetIndexes())
        {
            Vector2 nodePos = structure.Nodes[i].pos;
            if (nodePos.LiesWithinRect(selectPos1, selectPos2))
            {
                nodes.Add(i);
            }
        }

        return nodes;
    }

    public void SelectElementsWithinArea()
    {
        selectedElements = GetElementsWithinArea();
    }
    public void SelectNodesWithinArea()
    {
        selectedNodes = GetNodesWithinArea();
    }

    public List<Vector2> GetNodePositionListFromIndexes(List<int> indexes)
    {
        List<Vector2> positions = new List<Vector2>();
        foreach (int i in indexes)
        {
            positions.Add(structure.Nodes[i].pos);
        }
        return positions;
    }

    public List<int> GetElementsWithinArea()
    {
        List<int> elements = new List<int>();
        List<int> nodesWithinArea = GetNodesWithinArea();
        foreach (int i in structure.Elements.GetIndexes())
        {
            if (nodesWithinArea.Contains(structure.Elements[i].Node1Id) && nodesWithinArea.Contains(structure.Elements[i].Node2Id))
            {
                elements.Add(i);
            }
        }

        return elements;
    }
}