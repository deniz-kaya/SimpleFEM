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
}