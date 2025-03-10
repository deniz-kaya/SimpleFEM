using SimpleFEM.Interfaces;

namespace SimpleFEM.LinearAlgebra;

public struct Matrix6 : ILinearAlgebra
{
    private float[] _mat;
    
    public int Rows => 6;
    public int Columns => 6;
    public Matrix6()
    {
        _mat = new float[36];
    }
    public ref float this[int row, int col] {
        get => ref _mat[row * 6 + col];
    }

    public static Matrix6 operator * (Matrix6 left, Matrix6 right)
    {
        Matrix6 final = new Matrix6();
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 6; col++)
            {
                for (int k = 0; k < 6; k++)
                {
                    final[row, col] += left[row, k] * right[k, col];
                }
            }
        }
        return final;
    }

    public static Matrix6 Transpose(Matrix6 m)
    {
        Matrix6 t = new Matrix6();
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 6; col++)
            {
                t[row,col] = m[col, row];
            }
        }

        return t;
    }
    public static Matrix6 operator *(float constant, Matrix6 matrix)
    {
        Matrix6 final = new Matrix6();
        for (int rows = 0; rows < 6; rows++)
        {
            for (int cols = 0; cols < 6; cols++)
            {
                final[rows, rows] = constant * matrix[rows, rows];
            }
        }

        return final;
    }

    public static Matrix6 Identity
    {
        get
        {
            Matrix6 identity = new Matrix6();
            for (int i = 0; i < 6; i++)
            {
                identity[i, i] = 1f;
                
            }
            return identity;
        }
    }
    public static void DebugPrint(Matrix6 matrix)
    {
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 6; col++)
            {
                Console.Write(matrix[row, col]);
            }

            Console.WriteLine();
        }
    }
}