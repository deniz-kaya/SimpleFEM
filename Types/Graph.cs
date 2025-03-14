using System.Collections.Generic;
using System.Linq;

namespace SimpleFEM.Types;

public class Graph
{
    private Dictionary<int, List<int>> _adjacencyList;
    public Graph()
    {
        //init adjacency list
        _adjacencyList = new Dictionary<int, List<int>>();
    }
    public int NodeCount => _adjacencyList.Count;
    public void AddVertex(int vertexID)
    {
        //add the vertex with ID if it doesnt exist
        if (!_adjacencyList.ContainsKey(vertexID))
        {
            _adjacencyList[vertexID] = new List<int>();
        }
    }

    public void AddEdge(int vertex1ID, int vertex2ID)
    {
        //add the vertices of the edge, if they dont exist
        AddVertex(vertex1ID);
        AddVertex(vertex2ID);
        
        //add the connection between vertices to their adjacency lists
        _adjacencyList[vertex1ID].Add(vertex2ID);
        _adjacencyList[vertex2ID].Add(vertex1ID);
    }

    public bool IsConnected()
    {
        //create hash set which will contain visited nodes
        HashSet<int> visitedNodes = new HashSet<int>();

        //start depth first search on the first node of the graph
        DepthFirstSearch(_adjacencyList.Keys.First(), visitedNodes);
        
        //if this is true, the structure is fully connected as all nodes were visited
        if (visitedNodes.Count == _adjacencyList.Count)
        {
            return true;
        }

        return false;
    }

    private void DepthFirstSearch(int currentNode, HashSet<int> visitedNodes)
    {
        //visitedNodes hashset is passed by reference as it is a reference type, unlike e.g. arrays
        //this means that it can be changed within this subroutine
        
        //.Add function returns false if it exists in the set, otherwise adds and returns true
        if (!visitedNodes.Add(currentNode))
        {
            return;
        }

        //repeat the procedure for each node that is connected to the current one
        foreach (int connectedNode in _adjacencyList[currentNode])
        {
            DepthFirstSearch(connectedNode, visitedNodes);
        }
    }
}