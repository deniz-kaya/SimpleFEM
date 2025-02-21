using SimpleFEM.Interfaces;

namespace SimpleFEM.Extensions;

public class Matrix : ILinearAlgebra
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

    public int GetRows() => rows;
    public int GetColumns() => columns;
    public float this[int row, int col] 
    {
        get => data[row * columns + col];
        set => data[row * columns + col] = value;
    }
}