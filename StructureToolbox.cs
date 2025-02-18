using System.Numerics;

namespace SimpleFEM;
//structure toolbox
//takes structure as input, builds on top of structure
//selected nodes, elements etc are all insidf eof htis 
//it is stored inside of scene
//inputs are processed inside of scene, relevant toolbox methods are called to extract information from structure
public class StructureToolbox
{
    public int selectionFeather = 10;
    private Structure structure;
    public Tool CurrentTool { get; private set; }
    public bool MultiInputStarted { get; private set; }
    public bool EmptySelection
    {
        get => selectedElements.Count == 0 && selectedNodes.Count == 0;
    }
    public StructureToolbox(Structure structure)
    {
        selectedNodes = new();
        selectedElements = new();
        SwitchState(Tool.None);
        this.structure = structure;
    }
    public List<int> selectedNodes { get; private set;}
    public List<int> selectedElements { get; private set;}
    public Vector2 selectPos1 { get; private set; }
    public Vector2 selectPos2 { get; private set; }

    public bool SelectNearbyElement(Vector2 point)
    {
        selectedElements.Clear();
        selectedNodes.Clear();
        int candidateElement = CheckForElementsCloseToPos(point, 10);
        if (candidateElement != -1)
        {
            selectedElements.Add(candidateElement);
            return true;
        }

        return false;
    }
    public bool SelectNearbyNode(Vector2 point)
    {
        selectedElements.Clear();
        selectedNodes.Clear();
        int candidateNode = CheckForNodesCloseToPos(point, 10);
        if (candidateNode != -1) 
        {
            selectedNodes.Add(candidateNode);
            return true;
        }

        return false;
    }

    public int CheckForElementsCloseToPos(Vector2 point, float threshold)
    {
        foreach (int i in structure.Elements.GetIndexes())
        {
            Vector2 node1Pos = structure.Nodes[structure.Elements[i].Node1ID].Pos;
            Vector2 node2Pos = structure.Nodes[structure.Elements[i].Node2ID].Pos;
            if (point.DistanceToLineSegment(node1Pos, node2Pos) < threshold)
            {
                return i;
            }
        }

        return -1;
    }
    public int CheckForNodesCloseToPos(Vector2 point, float threshold)
    {
        foreach (int i in structure.Nodes.GetIndexes())
        {
            if (Vector2.Distance(point, structure.Nodes[i].Pos) <= threshold)
            {
                return i;
            }
        }

        return -1;
    }
    public void SoftSwitchState(Tool switchToTool = Tool.None)
    {
        CurrentTool = switchToTool;
        MultiInputStarted = false;
        selectPos1 = Vector2.Zero;
        selectPos2 = Vector2.Zero;
    }
    public void SwitchState(Tool switchToTool = Tool.None)
    {
        CurrentTool = switchToTool;
        MultiInputStarted = false;
        selectPos1 = Vector2.Zero;
        selectPos2 = Vector2.Zero;
        selectedElements.Clear();
        selectedNodes.Clear();
    }
    public void SetFirstSelectPos(Vector2 pos)
    {
        selectPos1 = pos;
        MultiInputStarted = true;
    }
    public void SetSecondSelectPos(Vector2 pos)
    {
        selectPos2 = pos;
    }
    public void AddNode()
    {
        structure.AddNode(new Node(selectPos1));
    }
    public void AddElement()
    {
        int node1 = structure.CheckForNodeCollisions(selectPos1);
        int node2 = structure.CheckForNodeCollisions(selectPos2);
        if (node1 == -1)
        {
            structure.AddNode(new Node(selectPos1));
            node1 = structure.Nodes.LastAddedIndex;
        }
        if (node2 == -1)
        {
            structure.AddNode(new Node(selectPos2));
            node2 = structure.Nodes.LastAddedIndex;
        }
        structure.AddElement(node1, node2, new Material());
    }

    public bool AddBoundaryConditions(BoundaryCondition boundaryCondition)
    {
        if (selectedNodes.Count == 0) return false;
        foreach (int i in selectedNodes)
        {
            structure.BoundaryConditions.Add();
        }
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
            Vector2 nodePos = structure.Nodes[i].Pos;
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
            positions.Add(structure.Nodes[i].Pos);
        }
        return positions;
    }
    public List<int> GetElementsWithinArea()
    {
        List<int> elements = new List<int>();
        List<int> nodesWithinArea = GetNodesWithinArea();
        foreach (int i in structure.Elements.GetIndexes())
        {
            if (nodesWithinArea.Contains(structure.Elements[i].Node1ID) && nodesWithinArea.Contains(structure.Elements[i].Node2ID))
            {
                elements.Add(i);
            }
        }

        return elements;
    }
}