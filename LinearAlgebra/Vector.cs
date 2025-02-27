namespace SimpleFEM.LinearAlgebra;

public struct Vector
{
    private float[] data;
    public Vector(int size)
    {
        data = new float[size];
    }
    public int Size => data.Length;
    public ref float this[int index]
    {
        get => ref data[index];
    }

    public void DebugPrint()
    {
        Console.WriteLine();
        for (int i = 0; i < data.Length; i++)
        {
            Console.Write($"{data[i]:0.##E+0} ");
        }
    }
}