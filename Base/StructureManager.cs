using System.Collections.Immutable;
using System.ComponentModel;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SimpleFEM;

public class StructureManager
{
    public struct Settings
    {
        
        public float IdleSelectionFeather;

        public Settings(float idleSelectionFeather)
        {
            IdleSelectionFeather = idleSelectionFeather;
        }

        public static Settings Default = new Settings(10f);

    }
    
    public Tool CurrentTool { get; private set; }
    protected List<int> SelectedElements { get; private set; }
    protected List<int> SelectedNodes { get; private set; }
    protected bool MultiInputStarted { get; private set; }
    protected bool MultiInputCompleted { get; private set; }
    public bool EmptySelection => SelectedElements.Count == 0 && SelectedNodes.Count == 0;
    protected Vector2 SelectionPos1;
    protected Vector2 SelectionPos2;
    protected IStructure Structure;
    protected Settings settings;

    protected bool IdleSelection(Vector2 pos)
    {
        bool successfulSelection = false;
        if (MultiInputStarted) return false;
        if (!EmptySelection) return false;
        if (!SelectNearbyNode(pos))
        {
            return SelectNearbyElement(pos);
        }
        return true;
    }
    public StructureManager(IStructure structure, Settings? settings = null)
    {
        this.settings = settings ?? Settings.Default;
        Structure = structure;

        MultiInputStarted = false;
    }

    public void ResetSelection()
    {
        SelectedElements.Clear();
        SelectedNodes.Clear();
    }
    public bool SelectNearbyElement(Vector2 point)
    {
        ResetSelection();
        int candidateElement = CheckForElementsCloseToPos(point, settings.IdleSelectionFeather);
        if (candidateElement != -1)
        {
            SelectedElements.Add(candidateElement);
            return true;
        }

        return false;
    }
    public int CheckForElementsCloseToPos(Vector2 point, float threshold)
    {
        foreach (int i in Structure.GetElementIndexes())
        {
            Element e = Structure.GetElement(i);
            Vector2 node1Pos = Structure.GetNode(e.Node1ID).Pos;
            Vector2 node2Pos = Structure.GetNode(e.Node2ID).Pos;
            if (point.DistanceToLineSegment(node1Pos, node2Pos) < threshold)
            {
                return i;
            }
        }

        return -1;
    }
    public bool SelectNearbyNode(Vector2 point)
    {
        ResetSelection();
        int candidateNode = CheckForNodesCloseToPos(point, settings.IdleSelectionFeather);
        if (candidateNode != -1) 
        {
            SelectedNodes.Add(candidateNode);
            return true;
        }

        return false;
    }
    private int CheckForNodesCloseToPos(Vector2 point, float threshold)
    {
        foreach (int i in Structure.GetNodeIndexes())
        {
            if (Vector2.Distance(point, Structure.GetNode(i).Pos) <= threshold)
            {
                return i;
            }
        }

        return -1;
    }

    protected void DeleteSelectedNodes()
    {
        foreach (int i in SelectedNodes)
        {
            Structure.RemoveNode(i);
        }

        SelectedNodes.Clear();
    }

    protected void DeleteSelectedElements()
    {
        foreach (int i in SelectedElements)
        {
            Structure.RemoveElement(i);
        }
        SelectedElements.Clear();
    }

    protected void SelectElementsWithinArea()
    {
        SelectedElements.Clear();
        SelectedElements = GetElementsWithinArea(SelectionPos1, SelectionPos2);
    }
    protected void SelectNodesWithinArea()
    {
        SelectedNodes.Clear();
        SelectedNodes = GetNodesWithinArea(SelectionPos1, SelectionPos2);
    }
    protected List<int> GetNodesWithinArea(Vector2 pos1, Vector2 pos2)
    {
        List<int> nodes = new List<int>();
        foreach (int i in Structure.GetNodeIndexes())
        {
            Vector2 nodePos = Structure.GetNode(i).Pos;
            if (nodePos.LiesWithinRect(pos1, pos2))
            {
                nodes.Add(i);
            }
        }

        return nodes;
    }

    protected List<int> GetElementsWithinArea(Vector2 pos1, Vector2 pos2)
    {
        List<int> elements = new List<int>();
        List<int> nodesWithinArea = GetNodesWithinArea(pos1, pos2);
        foreach (int i in Structure.GetElementIndexes())
        {
            Element e = Structure.GetElement(i);
            if (nodesWithinArea.Contains(e.Node1ID) && nodesWithinArea.Contains(e.Node2ID))
            {
                elements.Add(i);
            }
        }

        return elements;
    }
    
}