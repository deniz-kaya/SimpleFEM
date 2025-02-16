using System.Data.Common;

namespace SimpleFEM;

public class Structure
    {
        public RecyclingList<Node> Nodes;
        public RecyclingList<Element> Elements;
        public RecyclingList<Load> Loads;
        public RecyclingList<BoundaryCondition> BoundaryConditions;

        public string StructureName;
        
        static public Material TestMaterial = new Material {E = 1.0f, Poisson = 1.0f, Density = 1.0f};
        public Structure(string name)
        {
            Nodes = new RecyclingList<Node>();
            Elements = new RecyclingList<Element>();
            Loads = new RecyclingList<Load>();
            BoundaryConditions = new RecyclingList<BoundaryCondition>();

            StructureName = name;
        }


        public bool CheckForNodeCollisions(Node node)
        {
            foreach (Node n in Nodes)
            {
                if (n.X == node.X && n.Y == node.Y)
                {
                    return true;
                }
            }

            return false;
        }

        public bool AddNode(float x, float y)
        {
            Node candidateNode = new Node(x, y);
            if (!CheckForNodeCollisions(candidateNode))
            {
                Nodes.Add(candidateNode);
                return true;
            }

            return false;
        }

        public bool RemoveNodeConnectedBoundaryConditions(int id)
        {
            foreach (int i in BoundaryConditions.GetIndexes())
            {
                if (BoundaryConditions[i].NodeID == id)
                {
                    BoundaryConditions.RemoveAt(i);
                    return true;
                }
            }
            
            return false;
        }
        public bool RemoveNodeConnectedLoads(int id)
        {
            foreach (int i in Loads.GetIndexes())
            {
                if (Loads[i].NodeID == id)
                {
                    Loads.RemoveAt(i);
                    return true;
                }
            }
            
            return false;
        }
        public bool RemoveNodeConnectedElements(int id)
        {
            bool returnVal = false;
            foreach (int i in Elements.GetIndexes())
            {
                if (Elements[i].Node1Id == id || Elements[i].Node2Id == id)
                {
                    returnVal = true;
                    Elements.RemoveAt(i);
                }
            }

            return returnVal;
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
                if (Elements[i].Node1Id == element.Node1Id && Elements[i].Node2Id == element.Node2Id)
                {
                    return false;
                }
            }
            
            return true;
            
        }
        
        public bool AddElement(int node1Id, int node2Id, Material material)
        {
            Element candidateElement = new Element(node1Id, node2Id, material);
            if (node1Id != node2Id && Nodes.ValidIndex(node1Id)  && Nodes.ValidIndex(node2Id) && CheckForElementCollisions(candidateElement))
            {
                Elements.Add(candidateElement);
                return true;
            }

            return false;
        }
    }