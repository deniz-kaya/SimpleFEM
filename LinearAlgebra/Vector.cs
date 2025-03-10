namespace SimpleFEM.LinearAlgebra;

public struct Vector
{
    private float[] _data;
    public Vector(int size)
    {
        _data = new float[size];
    }
    public int Size => _data.Length;
    public ref float this[int index]
    {
        get => ref _data[index];
    }

    public void DebugPrint()
    {
        Console.WriteLine();
        for (int i = 0; i < _data.Length; i++)
        {
            Console.Write($"{_data[i]:0.##E+0} ");
        }
    }
}