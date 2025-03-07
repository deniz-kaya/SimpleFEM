using System.Numerics;
using SimpleFEM.Types;
using SimpleFEM.Types.Settings;

namespace SimpleFEM.Base;
using SimpleFEM.Types.StructureTypes;
using SimpleFEM.Interfaces;
using SimpleFEM.Extensions;

public class InMemoryStructure : IStructure
{
    public RecyclingList<Node> Nodes;
    public Dictionary<int, Load> Loads;
    public Dictionary<int, BoundaryCondition> BoundaryConditions;
    public RecyclingList<Element> Elements;
    private string StructureName;
    private StructureSettings settings;
    // TODO maybe replace null checks with setting to default from the structure creation screen
    // TODO URGENT change GetLoad and getboundarycondition to return said things
    public InMemoryStructure(string name, StructureSettings? settings) 
    {
        this.settings = settings ?? StructureSettings.Default;
        Loads = new Dictionary<int, Load>();
        BoundaryConditions = new Dictionary<int, BoundaryCondition>();
        StructureName = name;
        Nodes = new RecyclingList<Node>();
        Elements = new RecyclingList<Element>();
    }

    public StructureSettings GetStructureSettings()
    {
        return settings;
    }
    public string GetName() => StructureName;

    public bool ValidNodeID(int nodeID)
    {
        return Nodes.ValidIndex(nodeID);
    }

    public bool ValidElementID(int elementID)
    {
        return Elements.ValidIndex(elementID);
    }
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
        Loads[Nodes.LastAddedIndex] = default;
        BoundaryConditions[Nodes.LastAddedIndex] = default;

        return true;
    }

    public bool AddElement(Element element)
    {
        if (Nodes.ValidIndex(element.Node1ID) && Nodes.ValidIndex(element.Node2ID) && element.Node1ID != element.Node2ID)
        {
            bool duplicate = false;
            foreach (Element e in Elements)
            {
                if ((e.Node1ID == element.Node2ID && e.Node2ID == element.Node1ID) || (e.Node1ID == element.Node1ID && e.Node2ID == element.Node2ID))
                {
                    duplicate = true;
                    break;
                }
            }

            if (!duplicate)
            {
                Elements.Add(element);
                return true;
            }
        }
        return false;
    }

    public bool AddElement(Element element, out int index)
    {
        if (Nodes.ValidIndex(element.Node1ID) && Nodes.ValidIndex(element.Node2ID) && element.Node1ID != element.Node2ID)
        {
            bool duplicate = false;
            foreach (Element e in Elements)
            {
                if ((e.Node1ID == element.Node2ID && e.Node2ID == element.Node1ID) || (e.Node1ID == element.Node1ID && e.Node2ID == element.Node2ID))
                {
                    duplicate = true;
                    break;
                }
            }
            if (!duplicate)
            {
                Elements.Add(element);
                index = Elements.LastAddedIndex;
                return true;
            }
        }

        index = -1;
        return false;
    }

    public bool AddNode(Vector2 pos, out int index)
    {
        foreach (int i in Nodes.GetIndexes())
        {
            if (Nodes[i].Pos == pos)
            {
                index = i;
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

            Loads[nodeID] = default;
            BoundaryConditions[nodeID] = default;
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
            BoundaryConditions[nodeID] = boundaryCondition;;
        }
        else
        {
            throw new IndexOutOfRangeException("Invalid NodeID");
        }
    }

    public BoundaryCondition GetBoundaryCondition(int nodeID)
    {
        if (Nodes.ValidIndex(nodeID))
        {
            return BoundaryConditions[nodeID];
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
            Loads[nodeID] = load;
        }
        else
        {
            throw new IndexOutOfRangeException("Invalid NodeID");
        }
    }

    public Load GetLoad(int nodeID)
    {
        if (Nodes.ValidIndex(nodeID))
        {
            return Loads[nodeID];
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

    public int GetNodeCount()
    {
        return Nodes.Count;
    }

    public int GetElementCount()
    {
        return Elements.Count;
    }

    public int GetBoundaryConditionCount()
    {
        int count = 0;
        foreach (BoundaryCondition bc in BoundaryConditions.Values)
        {
            if (bc.FixedX){count++;}
            if (bc.FixedY){count++;}
            if (bc.FixedRotation){count++;}
        }

        return count;
    }

    public int GetLoadCount()
    {
        int count = 0;
        foreach (Load l in Loads.Values)
        {
            if (l.ForceX != 0) {count++;}
            if (l.ForceY != 0) {count++;}
            if (l.Moment != 0) {count++;}
        }

        return count;
    }
}