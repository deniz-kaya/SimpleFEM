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

    public void SelectNearbyNode(Vector2 point)
    {
        selectedNodes.Clear();
        int candidateNode = CheckForNodesCloseToPos(point, 10);
        if (candidateNode != -1)
        {
            selectedNodes.Add(candidateNode);
        }
    }
    public int CheckForNodesCloseToPos(Vector2 point, int threshold)
    {
        foreach (int i in structure.Nodes.GetIndexes())
        {
            if (Vector2.Distance(point, structure.Nodes[i].pos) <= threshold)
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

    public void GetPositionInformation()
    {
        throw new NotImplementedException();
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