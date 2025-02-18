using System.Data.Common;
using System.Numerics;

namespace SimpleFEM;

public class Structure
    {
        public RecyclingList<Node> Nodes;
        public RecyclingList<Element> Elements;

        public string StructureName;

        static public Material TestMaterial = new Material {E = 1.0f, Poisson = 1.0f, Density = 1.0f};
        public Structure(string name)
        {
            Nodes = new RecyclingList<Node>();
            Elements = new RecyclingList<Element>();

            StructureName = name;
        }

        public void RemoveElement(int index)
        {
            Elements.RemoveAt(index);
        }
        public int CheckForNodeCollisions(Vector2 testPos)
        {
            foreach (int i in Nodes.GetIndexes())
            {
                if (Nodes[i].Pos == testPos)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool AddNode(Node node)
        {
            if (CheckForNodeCollisions(node.Pos) == -1)
            {
                Nodes.Add(node);
                return true;
            }

            return false;
        }

        public bool RemoveNodeConnectedElements(int id)
        {
            bool atLeastOneElementRemoved = false;
            foreach (int i in Elements.GetIndexes())
            {
                if (Elements[i].Node1ID == id || Elements[i].Node2ID == id)
                {
                    atLeastOneElementRemoved = true;
                    Elements.RemoveAt(i);
                }
            }

            return atLeastOneElementRemoved;
        }

        public void ModifyLoad(int nodeID, float forceX, float forceY, float moment)
        {
            
            if (!Nodes.ValidIndex(nodeID))
            {
                return;
            }
            Load modifiedLoad = new Load(nodeID, forceX, forceY, moment);
            foreach (int i in Loads.GetIndexes())
            {
                if (Loads[i].NodeID == nodeID)
                {
                    Loads[i] = modifiedLoad;
                    return;
                } 
            }
            
            //if code reaches here, load on this node does not exist therefore we must add it
            
            Loads.Add(modifiedLoad);
        }

        public void ModifyBoundaryCondition(int nodeID, bool fixedX, bool fixedY, bool fixedMoment)
        {
            if (!Nodes.ValidIndex(nodeID))
            {
                return;
            }

            BoundaryCondition modifiedBoundaryCondition = new BoundaryCondition(nodeID, fixedX, fixedY, fixedMoment);
            foreach (int i in BoundaryConditions.GetIndexes())
            {
                if (BoundaryConditions[i].NodeID == nodeID)
                {
                    BoundaryConditions[i] = modifiedBoundaryCondition;
                    return;
                }
            }

            // if code reaches here, that means that it hasn't found a boundary condition with given nodeID
            // so we should add it to the list

            BoundaryConditions.Add(modifiedBoundaryCondition);
        }

        public bool RemoveNode(int id)
        {
            RemoveNodeConnectedElements(id);
            RemoveNodeConnectedLoads(id);
            RemoveNodeConnectedBoundaryConditions(id);
            return Nodes.RemoveAt(id);
        }

        public bool CheckForElementCollisions(Element element)
        {
            foreach (int i in Elements.GetIndexes())
            {
                if (Elements[i].Node1ID == element.Node1ID && Elements[i].Node2ID == element.Node2ID)
                {
                    return false;
                }
                if (Elements[i].Node1ID == element.Node2ID && Elements[i].Node2ID == element.Node1ID)
                {
                    return false;
                }

            }
            
            return true;
            
        }
        
        public bool AddElement(int node1Id, int node2Id, Material material)
        {
            Element candidateElement = new Element(node1Id, node2Id, material);
            if (node1Id != node2Id && Nodes.ValidIndex(node1Id) && Nodes.ValidIndex(node2Id) && CheckForElementCollisions(candidateElement))
            {
                Elements.Add(candidateElement);
                return true;
            }

            return false;
        }
    }