using System.Numerics;
using SimpleFEM.Extensions;
using SimpleFEM.Types.StructureTypes;
using SimpleFEM.Interfaces;

namespace SimpleFEM.Base;

public class StructureEditor
{
    protected List<int> SelectedElements { get; private set; }
    protected List<int> SelectedNodes { get; private set; }
    public int SelectedNodeCount => SelectedNodes.Count;
    public int SelectedElementCount => SelectedElements.Count;
    protected bool MultiInputStarted;
    public bool EmptySelection => SelectedElements.Count == 0 && SelectedNodes.Count == 0;

    protected Vector2 MultiSelectLockedPos;
    protected Vector2 LivePos;
    protected IStructure Structure;
    public StructureEditor(IStructure structure)
    {
        
        SelectedElements = new List<int>();
        SelectedNodes = new List<int>();
        Structure = structure;
        MultiInputStarted = false;
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


    
    // todo error handling potential with bool instead of void in calling method
    public void SelectNode(int nodeID)
    {
        if (!SelectedNodes.Contains(nodeID))
        {
            SelectedNodes.Add(nodeID);
        }
    }

    public void DeselectNode(int nodeID)
    {
        if (SelectedNodes.Contains(nodeID))
        {
            SelectedNodes.Remove(nodeID);
        }
    }

    public void DeselectElement(int elementID)
    {
        if (SelectedElements.Contains(elementID))
        {
            SelectedElements.Remove(elementID);
        }
    }
    public void SelectElement(int elementID)
    {
        if (!SelectedElements.Contains(elementID))
        {
            SelectedElements.Add(elementID);
        }
    }
    public int CheckForNodesCloseToPos(Vector2 point, float threshold)
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

    public void DeleteSelectedNodes()
    {
        foreach (int i in SelectedNodes)
        {
            Structure.RemoveNode(i);
        }

        SelectedNodes.Clear();
    }

    public void DeleteSelectedElements()
    {
        foreach (int i in SelectedElements)
        {
            Structure.RemoveElement(i);
        }
        SelectedElements.Clear();
    }

    public void AddBoundaryConditionToSelectedNodes(BoundaryCondition boundaryCondition)
    {
        foreach (int i in SelectedNodes)
        {
            Structure.SetBoundaryCondition(i, boundaryCondition);
        }
    }
    public void AddLoadToSelectedNodes(Load load)
    {
        foreach (int i in SelectedNodes)
        {
            Structure.SetLoad(i, load);
        }
    }
    protected void SelectElementsWithinArea()
    {
        SelectedElements.Clear();
        SelectedElements = GetElementsWithinArea(MultiSelectLockedPos, LivePos);
    }
    protected void SelectNodesWithinArea()
    {
        SelectedNodes.Clear();
        SelectedNodes = GetNodesWithinArea(MultiSelectLockedPos, LivePos);
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