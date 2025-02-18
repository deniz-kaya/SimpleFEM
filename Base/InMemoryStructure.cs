using System.Numerics;

namespace SimpleFEM.Base;
using SimpleFEM.Types.StructureTypes;
using SimpleFEM.Interfaces;
using SimpleFEM.Extensions;

public class InMemoryStructure : IStructure
{
    public RecyclingList<Node> Nodes;
    public RecyclingList<Element> Elements;
    private string StructureName;
    public InMemoryStructure(string name) 
    {
        StructureName = name;
        Nodes = new RecyclingList<Node>();
        Elements = new RecyclingList<Element>();
    }
    
    public string GetName() => StructureName;
    
    public bool AddNode(Vector2 pos)
    {
        foreach (Node n in Nodes)
        {
            if (n.Pos == pos)
            {
                return false;
            }
        } 
        
        Nodes.Add(new Node(pos));
        return true;
    }

    public bool AddElement(Element element)
    {
        if (Nodes.ValidIndex(element.Node1ID) && Nodes.ValidIndex(element.Node2ID) && element.Node1ID != element.Node2ID)
        {
            Elements.Add(element);
            return true;
        }
        return false;
    }

    public bool AddElement(Element element, out int index)
    {
        if (Nodes.ValidIndex(element.Node1ID) && Nodes.ValidIndex(element.Node2ID) && element.Node1ID != element.Node2ID)
        {
            Elements.Add(element);
            index = Elements.LastAddedIndex;
            return true;
        }

        index = -1;
        return false;
    }

    public bool AddNode(Vector2 pos, out int index)
    {
        foreach (Node n in Nodes)
        {
            if (n.Pos == pos)
            {
                index = -1;
                return false;
            }
        } 
        
        Nodes.Add(new Node(pos));
        index = Nodes.LastAddedIndex;
        return true;
    }

    public void RemoveNode(int nodeID)
    {
        if (Nodes.ValidIndex(nodeID))
        {
            foreach (int i in Elements.GetIndexes())
            {
                if (Elements[i].Node1ID == nodeID || Elements[i].Node2ID == nodeID)
                {
                    Elements.RemoveAt(i);
                }
            }

            Nodes.RemoveAt(nodeID);
        }
        else
        {
            throw new IndexOutOfRangeException("Invalid NodeID");
        }
        
    }
    
    public void RemoveElement(int elementID)
    {
        if (Elements.ValidIndex(elementID))
        {
            Elements.RemoveAt(elementID);
        }
        else
        {
            throw new IndexOutOfRangeException("Invalid ElementID");
        }
    }

    public Node GetNode(int nodeID)
    {
        if (Nodes.ValidIndex(nodeID))
        {
            return Nodes[nodeID];
        }
        else
        {
            throw new IndexOutOfRangeException("Invalid NodeID");
        }
    }

    public Element GetElement(int elementID)
    {
        if (Elements.ValidIndex(elementID))
        {
            return Elements[elementID];
        }
        else
        {
            throw new IndexOutOfRangeException("Invalid NodeID");
        }
    }
    
    public void SetBoundaryCondition(int nodeID, BoundaryCondition boundaryCondition)
    {
        if (Nodes.ValidIndex(nodeID))
        {
            Nodes[nodeID].SetBoundaryCondition(boundaryCondition);
        }
        else
        {
            throw new IndexOutOfRangeException("Invalid NodeID");
        }
    }

    public void GetBoundaryCondition(int nodeID, out BoundaryCondition boundaryCondition)
    {
        if (Nodes.ValidIndex(nodeID))
        {
            boundaryCondition = Nodes[nodeID].BoundaryCondition;
        }
        else
        {
            throw new IndexOutOfRangeException("Invalid NodeID");
        }

    }

    public void SetLoad(int nodeID, Load load)
    {
        if (Nodes.ValidIndex(nodeID))
        {
            Nodes[nodeID].SetLoad(load);
        }
        else
        {
            throw new IndexOutOfRangeException("Invalid NodeID");
        }
    }

    public void GetLoad(int nodeID, out Load load)
    {
        if (Nodes.ValidIndex(nodeID))
        {
            load = Nodes[nodeID].Load;
        }
        else
        {
            throw new IndexOutOfRangeException("Invalid NodeID");
        }
    }

    public List<int> GetElementIndexesSorted()
    {
        return Elements.GetIndexes();
    }

    public List<int> GetNodeIndexesSorted()
    {
        return Nodes.GetIndexes();
    }
}