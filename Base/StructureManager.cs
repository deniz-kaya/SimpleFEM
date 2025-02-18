using System.Numerics;
using SimpleFEM.Extensions;
using SimpleFEM.Types.StructureTypes;
using SimpleFEM.Interfaces;

namespace SimpleFEM.Base;

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
    
    protected List<int> SelectedElements { get; private set; }
    protected List<int> SelectedNodes { get; private set; }
    protected bool MultiInputStarted { get; private set; }
    protected bool MultiInputCompleted { get; private set; }
    public bool EmptySelection => SelectedElements.Count == 0 && SelectedNodes.Count == 0;
    protected Vector2 SelectionPos1;
    protected Vector2 SelectionPos2;
    protected IStructure Structure;
    protected Settings settings;
    
    public StructureManager(IStructure structure, Settings? settings = null)
    {
        this.settings = settings ?? Settings.Default;
        
        SelectedElements = new List<int>();
        SelectedNodes = new List<int>();
        Structure = structure;
        MultiInputStarted = false;
    }
    
    /// <summary>
    /// Tries selecting nodes around the position, if not found tries selecting elements instead.
    /// This order is to make sure that nodes can also be selected, as otherwise their valid selecting areas would mostly be covered by elements' ones.
    /// </summary>
    /// <param name="position">the position to select around</param>
    /// <returns>True if selection was successful, false if otherwise.</returns>
    protected bool IdleSelection(Vector2 position)
    {

        if (MultiInputStarted) return false;
        if (!EmptySelection) return false;
        if (!SelectNearbyNode(position))
        {
            return SelectNearbyElement(position);
        }
        return true;
    }
    /// <summary>
    /// Resets the selection lists.
    /// </summary>
    public void ResetSelection()
    {
        SelectedElements.Clear();
        SelectedNodes.Clear();
    }
    
    /// <summary>
    /// Tries selecting an element nearby the position.
    /// </summary>
    /// <param name="position">the position to select around</param>
    /// <returns>True if an element was selected.</returns>
    public bool SelectNearbyElement(Vector2 position)
    {
        ResetSelection();
        int candidateElement = CheckForElementsCloseToPos(position, settings.IdleSelectionFeather);
        if (candidateElement != -1)
        {
            SelectedElements.Add(candidateElement);
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// Checks for elements that are within the threshold distance to the position.
    /// Returns the element index that is smallest out of the qualifying elements. 
    /// </summary>
    /// <param name="position">the position to check around</param>
    /// <param name="threshold">acceptable distance to the node</param>
    /// <returns>-1 if there are no elements within the threshold distance, the element ID otherwise</returns>
    public int CheckForElementsCloseToPos(Vector2 position, float threshold)
    {
        foreach (int i in Structure.GetElementIndexesSorted())
        {
            Element e = Structure.GetElement(i);
            Vector2 node1Pos = Structure.GetNode(e.Node1ID).Pos;
            Vector2 node2Pos = Structure.GetNode(e.Node2ID).Pos;
            if (position.DistanceToLineSegment(node1Pos, node2Pos) < threshold)
            {
                return i;
            }
        }

        return -1;
    }
    
    /// <summary>
    /// Tries selecting a node that is nearby the position.
    /// </summary>
    /// <param name="position">the position to select around</param>
    /// <returns>True if a node was selected, false otherwise.</returns>
    public bool SelectNearbyNode(Vector2 position)
    {
        ResetSelection();
        int candidateNode = CheckForNodesCloseToPos(position, settings.IdleSelectionFeather);
        if (candidateNode != -1) 
        {
            SelectedNodes.Add(candidateNode);
            return true;
        }

        return false;
    }
    private int CheckForNodesCloseToPos(Vector2 point, float threshold)
    {
        foreach (int i in Structure.GetNodeIndexesSorted())
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
        foreach (int i in Structure.GetNodeIndexesSorted())
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
        foreach (int i in Structure.GetElementIndexesSorted())
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