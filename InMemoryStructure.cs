using System.Numerics;

namespace SimpleFEM;

public class InMemoryStructure : BaseStructure
{
    public RecyclingList<Node> Nodes;
    public RecyclingList<Element> Elements;

    public InMemoryStructure(string name) : base(name)
    {
        Nodes = new RecyclingList<Node>();
        Elements = new RecyclingList<Element>();
    }
    
    
    public override bool AddNode(Vector2 pos)
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

    public override bool AddElement(Element element)
    {
        if (Nodes.ValidIndex(element.Node1ID) && Nodes.ValidIndex(element.Node2ID) && element.Node1ID != element.Node2ID)
        {
            Elements.Add(element);
            return true;
        }
        return false;
    }

    public override bool AddElement(Element element, out int index)
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

    public override bool AddNode(Vector2 pos, out int index)
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

    public override void RemoveNode(int nodeID)
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
    
    public override void RemoveElement(int ElementID)
    {
        if (Elements.ValidIndex(ElementID))
        {
            Elements.RemoveAt(ElementID);
        }
        else
        {
            throw new IndexOutOfRangeException("Invalid ElementID");
        }
    }
    
    public override void SetBoundaryCondition(int nodeID, BoundaryCondition boundaryCondition)
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

    public override void GetBoundaryCondition(int nodeID, out BoundaryCondition boundaryCondition)
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

    public override void SetLoad(int nodeID, Load load)
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

    public override void GetLoad(int nodeID, out Load load)
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
}