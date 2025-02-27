using SimpleFEM.Interfaces;

namespace SimpleFEM.LinearAlgebra;

public struct Matrix6x6 : ILinearAlgebra
{
    private float[] mat;
    
    public int Rows => 6;
    public int Columns => 6;
    public Matrix6x6()
    {
        mat = new float[36];
    }
    public ref float this[int row, int col] {
        get => ref mat[row * 6 + col];
    }

    public static Matrix6x6 operator * (Matrix6x6 left, Matrix6x6 right)
    {
        Matrix6x6 final = new Matrix6x6();
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

    public static Matrix6x6 Transpose(Matrix6x6 m)
    {
        Matrix6x6 t = new Matrix6x6();
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 6; col++)
            {
                t[row,col] = m[col, row];
            }
        }

        return t;
    }
    public static Matrix6x6 operator *(float constant, Matrix6x6 matrix)
    {
        Matrix6x6 final = new Matrix6x6();
        for (int rows = 0; rows < 6; rows++)
        {
            for (int cols = 0; cols < 6; cols++)
            {
                final[rows, rows] = constant * matrix[rows, rows];
            }
        }

        return final;
    }

    public static Matrix6x6 Identity
    {
        get
        {
            Matrix6x6 identity = new Matrix6x6();
            for (int i = 0; i < 6; i++)
            {
                identity[i, i] = 1f;
                
            }
            return identity;
        }
    }
    public static void DebugPrint(Matrix6x6 matrix)
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