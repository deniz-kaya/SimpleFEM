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
}