using SimpleFEM.Interfaces;

namespace SimpleFEM.Extensions;

public struct Matrix : ILinearAlgebra
{
    private int rows;
    private int columns;
    private float[] data;
    public Matrix(int rows, int columns)
    {
        this.rows = rows;
        this.columns = columns;
        data = new float[rows * columns];
    }

    public int Rows => rows;
    public int Columns => columns;
    public ref float this[int row, int col] {
        get => ref data[row * columns + col];
    }
}