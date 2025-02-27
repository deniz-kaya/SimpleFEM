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
        throw new NotImplementedException();
    }
}