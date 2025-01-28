using System.Net.NetworkInformation;
using Microsoft.VisualBasic;

namespace SimpleFEM;


public class FunkyList<T>
{
    
    private Dictionary<int, T> dict;
    private int count;

    public Dictionary<int, T>.KeyCollection Keys()
    {
        return dict.Keys;
    }
    
    public void Remove(int index)
    {
        dict.Remove(index);
    }

    public void Add(T item)
    {
        dict.Add(count++, item);
    }

    public T this[int index]
    {
        get
        {
            return dict[dict.Keys.ElementAt(index)];
        }
        set 
        {
            dict[dict.Keys.ElementAt(index)] = 
        }
        
    }
}

public static class MatrixHelper 
{
    public static double[,] ScaleMatrix(double[,] matrix, double scale)
    {
        Dictionary<int, string> fart;
        for (int x = 0; x < matrix.GetLength(0); x++)
        {
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                matrix[x, y] *= scale;
            }
        }

        return matrix;
    }

    public static void PrintMatrix(double[,] matrix)
    {
        for (int x = 0; x < matrix.GetLength(0); x++)
        {
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                Console.Write(matrix[x,y]);
            }
            Console.WriteLine();
        }
    }
}
public class Structure
{
    public Structure()
    {
        
    }
}

public class Node
{
    public readonly int id;
    public double[] pos;
    public double[] DOFindex;
    public double[] forces; 
        
    public Node(int id, double[] pos, double[] forces)
    {
        this.id = id;
        this.pos = pos;
        this.DOFindex = [2 * id, 2 * id + 1];
        this.forces = forces;
    }
}

public class Material
{
    public double E;
    public double A;

    public Material(double E, double A)
    {
        this.E = E;
        this.A = A;
    }
}



public class Element
{
    public readonly int id;
    public Node node1;
    public Node node2;
    public Material material;
    public double length;
    public double[,] LocalStiffness;

    public Element(int id, Node node1, Node node2, Material material)
    {
        this.id = id;
        this.node1 = node1;
        this.node2 = node2;
        this.material = material;
        this.length = GetLength();
        this.LocalStiffness = GetStiffnessMatrix();
    }

    public double GetLength() //profile and optimise if i need to
    {
        double dx = node1.pos[0] - node2.pos[0];
        double dy = node1.pos[1] - node2.pos[1];
        double len2 = Math.Pow(dx, 2) + Math.Pow(dy, 2);
        return Math.Sqrt(len2);
    }

    private double[,] GetStiffnessMatrix()
    {
        // stiffness
        double stiffness = (this.material.E * this.material.A) / this.length;

        //cosines
        double c = (this.node2.pos[0] - this.node1.pos[0]) / this.length;
        double s = (this.node2.pos[1] - this.node1.pos[1]) / this.length;

        //matrix
        double[,] cosines = new double[4, 4]
        {
            { Math.Pow(c, 2), c * s, -1 * Math.Pow(c, 2), -1 * c * s },
            { c * s, Math.Pow(s, 2), -1 * c * s, -1 * Math.Pow(s, 2) },
            { -1 * Math.Pow(c, 2), -1 * c * s, Math.Pow(c, 2), c * s },
            { -1 * c * s, -1 * Math.Pow(s, 2), c * s, Math.Pow(s, 2) }
        };
        return MatrixHelper.ScaleMatrix(cosines, stiffness);
    }
}

public class Structure
{
    private List<Node> nodes;
    private List<Element> elements;
    
    public Structure()
    {
            
    }

    public void AddNode(Node node)
    {
        nodes.Add(node);
    }
}