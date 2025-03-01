namespace SimpleFEM;

public class Graph
{
    private Dictionary<int, List<int>> adjacencyList;
    public Graph()
    {
        adjacencyList = new Dictionary<int, List<int>>();
        
    }

    public void AddVertex(int vertexID)
    {
        if (!adjacencyList.ContainsKey(vertexID))
        {
            adjacencyList[vertexID] = new List<int>();
        }
    }

    public void AddEdge(int vertex1ID, int vertex2ID)
    {
        AddVertex(vertex1ID);
        AddVertex(vertex2ID);
        
        adjacencyList[vertex1ID].Add(vertex2ID);
        adjacencyList[vertex2ID].Add(vertex1ID);
    }
    
    public bool IsConnected()
    {
        //implement graph traversal and keep track of visited vertices, if all aren't visited, something wrong
        HashSet<int> visitedNodes = new HashSet<int>();

        DFS(adjacencyList.Keys.First(), visitedNodes);
        
        if (visitedNodes.Count == adjacencyList.Count)
        {
            return true;
        }

        return false;
    }

    public void DFS(int currentNode, HashSet<int> visitedNodes)
    {
        //visitedNodes hashset is passed by reference as it is a reference type, unlike e.g. arrays
        //this means that it can be changed within this subroutine
        
        //.Add returns false if it exists in the set, otherwise adds and returns true
        if (!visitedNodes.Add(currentNode))
        {
            return;
        }

        foreach (int connectedNode in adjacencyList[currentNode])
        {
            DFS(connectedNode, visitedNodes);
        }
    }
}