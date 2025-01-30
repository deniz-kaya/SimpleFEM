using System.Collections;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace SimpleFEM;

public class Data
{
    public class RecyclingList<T> : IEnumerable<T>
    {
        private T[] elements;
        private Stack<int> freeSpots;

        private bool[] occupied;

        private int elementCount;

        public bool Exists(T item, out int index)
        {
            index = -1;
            foreach (int i in GetIndexes())
            {
                if (EqualityComparer<T>.Default.Equals(elements[i], item))
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }

        public bool Exists(T item)
        {
            foreach (int i in GetIndexes())
            {
                if (EqualityComparer<T>.Default.Equals(elements[i], item))
                {
                    return true;
                }
            }
            return false;
        }
        
        public int Count {
            get { return elementCount; }
            private set
            {
                elementCount = value;
            }
        }

        public bool ExistsAt(int index)
        {
            return occupied[index];
        }
        public RecyclingList(int initialCapacity = 100)
        {
            elements = new T[initialCapacity];
            occupied = new bool[initialCapacity];
            elementCount = 0;
            freeSpots = new Stack<int>();
        }

        public T this[int index]
        {
            get
            {
                if (!ValidIndex(index))
                {
                    throw new IndexOutOfRangeException("Invalid index.");
                }
                else
                {
                    return elements[index];
                }
            }
            set
            {
                if (!ValidIndex(index))
                {
                    throw new IndexOutOfRangeException("Invalid index.");
                }
                else
                {
                    elements[index] = value;
                }
            }
        }

        public bool ValidIndex(int index)
        {
            bool indexTooSmall = index < 0;
            bool isntOccupied = !occupied[index];
            return !(indexTooSmall || isntOccupied);
        }
        public bool RemoveAt(int index)
        {
            if (ValidIndex(index))
            {
                occupied[index] = false;
                elementCount--;
                freeSpots.Push(index);
                return true;
            }

            return false;
        }
        
        public bool Remove(T item)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                if (occupied[i])
                {
                    if (EqualityComparer<T>.Default.Equals(elements[i], item))
                    {
                        RemoveAt(i);
                        return true;
                    }
                }
            }

            return false;
        }
        
        public void Add(T item)
        {
            int index;
            if (freeSpots.Count != 0)
            {
                index = freeSpots.Pop();
            }
            else
            {
                index = elementCount;
            }
            elements[index] = item;
            occupied[index] = true;
            elementCount++;
        }

        public List<int> GetIndexes()
        {
            List<int> indexes = new List<int>();
            for (int i = 0; i < elements.Length; i++)
            {
                if (occupied[i])
                {
                    indexes.Add(i);
                }
            }
            return indexes;
        }
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < elements.Length; i++)
            {
                if (occupied[i])
                {
                    yield return elements[i];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    public struct Node
    {
        public Node(double x, double y)
        {
            X = x; Y = y;
        }
        public double X;
        public double Y;

        public (double, double) Pos
        {
            get
            {
                return (X, Y);
            }
            set
            {
                X = value.Item1;
                Y = value.Item2;
            }
        }
        public double GetSquareDistance(double x, double y)
        {
            return Math.Pow(Math.Abs(X - x), 2) + Math.Pow(Math.Abs(Y - y), 2);
        }
    }

    public struct BoundaryCondition
    {
        public BoundaryCondition(int nodeID, bool fixedY, bool fixedX, bool fixedMoment)
        {
            this.NodeID = nodeID;
            this.FixedY = fixedY;
            this.FixedX = fixedX;
            this.FixedMoment = fixedMoment;
        }
        public int NodeID;
        public bool FixedY;
        public bool FixedX;
        public bool FixedMoment;
    }

    public struct Material
    {
        public double E;
        public double Poisson;
        public double Density;
    }

    public struct Section
    {
        public double I;
        public double A;
    }

    public class Element
    {
        public int Node1Id;
        public int Node2Id;
        public Material ElementMaterial;

        public Element(int node1Id, int node2Id, Material elementMaterial)
        {
            Node1Id = node1Id;
            Node2Id = node2Id;
            ElementMaterial = elementMaterial;
        }
    }

    public class Load
    {
        public Load(int nodeID, double forceX, double forceY, double moment)
        {
            this.NodeID = nodeID;
            this.ForceX = forceX;
            this.ForceY = forceY;
            this.Moment = moment;
        }
        public int NodeID;
        public double ForceX;
        public double ForceY;
        public double Moment;
    }

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

        public bool AddNode(double x, double y)
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

        public bool RemoveElement(int id)
        {
            if (Elements.ValidIndex(id))
            {
                Elements.RemoveAt(id);
                return true;
            }

            return false;
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
}