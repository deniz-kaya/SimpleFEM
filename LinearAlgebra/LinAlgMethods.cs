using System;
using System.Numerics;

namespace SimpleFEM.LinearAlgebra;

public static class LinAlgMethods
{
    public static Vector Solve(Matrix m, Vector v)
    {
        (Matrix l, Matrix u) = LUDecompose(m);
        // V = ME
        // V = LUE
        // Y = UE
        // V = LY
        Vector y = ForwardSubstitute(l, v);
        return BackwardSubstitute(u, y);
    }
    public static (Matrix L, Matrix U) LUDecompose(Matrix K)
    {
        if (K.Rows != K.Columns)
        {
            throw new ArgumentOutOfRangeException("Matrix must be square!");
        }

        int size = K.Rows;

        Matrix L = new Matrix(size, size);
        //L has 1's on the leading diagonal 
        for (int i = 0; i < size; i++)
        {
            L[i, i] = 1f;
        }
        Matrix U = new Matrix(size, size);

        for (int i = 0; i < size; i++)
        {
            //compute the relevant U row
            for (int col = i; col < size; col++)
            {
                float sum = 0;
                for (int k = 0; k < i; k++)
                {
                    sum += L[i, k] * U[k, col];
                }

                U[i, col] = K[i, col] - sum;
            }
            
            //compute the relevant L column
            //i+1 as we do not want to overwrite the leading diagonal
            for (int row = i + 1; row < size; row++)
            {
                float sum = 0;
                for (int k = 0; k < i; k++)
                {
                    sum += L[row, k] * U[k, i];
                }
                L[row, i] = (K[row, i] - sum) / U[i, i];
            }
        }

        return (L, U);
    }
    
    public static Vector ForwardSubstitute(Matrix m, Vector v)
    {
        if (m.Rows != m.Columns)
        {
            throw new ArgumentOutOfRangeException("Matrix must be square!");
        }

        if (m.Rows != v.Size)
        {
            throw new ArgumentOutOfRangeException("Vector dimension must be same as matrix cols/rows!");
        }

        int size = m.Rows;
        Vector x = new Vector(v.Size);
        for (int i = 0; i < size; i++)
        {
            float sum = 0;
            //column
            for (int j = 0; j < i; j++)
            {
                sum += m[i, j] * x[j];
            }

            x[i] = v[i] - sum;
        }

        return x;
    }

    public static Vector BackwardSubstitute(Matrix m, Vector v)
    {
        if (m.Rows != m.Columns)
        {
            throw new ArgumentOutOfRangeException("Matrix must be square!");
        }

        if (m.Rows != v.Size)
        {
            throw new ArgumentOutOfRangeException("Vector dimension must be same as matrix cols/rows!");
        }

        int size = m.Rows;
        Vector x = new Vector(v.Size);
        for (int i = size - 1; i >= 0; i--)
        {
            float sum = 0;

            if (m[i, i] == 0)
            {
                throw new DivideByZeroException("Zero in upper matrix rank, System is most likely unstable.");
            }
            for (int j = i; j < size; j++)
            {
                sum += m[i, j] * x[j];
            }
            
            x[i] = (v[i] - sum) / m[i,i];
        }

        return x;
    }
}