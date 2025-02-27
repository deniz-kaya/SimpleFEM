using SimpleFEM.Interfaces;

namespace SimpleFEM.LinearAlgebra;

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

    public float Rank
    {
        get
        {
            float rank = 0;
            for (int i = 0; i < rows; i++)
            {
                rank += this[i, i];
            }
            return rank;
        }
    }
    public ref float this[int row, int col] {
        get
        {
            if (row < rows && col < columns)
            {
                return ref data[row * columns + col];
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }
    }

    public void DebugPrint()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Console.Write($"{this[r,c]:0.##E+0} ");
            }
            
            Console.WriteLine();
        }
    }
}