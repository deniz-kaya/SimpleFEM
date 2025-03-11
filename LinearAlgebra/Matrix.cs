using System;
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

    public static Matrix operator *(Matrix left, Matrix right)
    {
        if (left.Columns != right.Rows)
        {
            throw new ArgumentException("First matrix columns is not equal to the second matrix rows!");
        }
        Matrix final = new Matrix(left.Rows, right.Columns);
        for (int row = 0; row < left.Rows; row++)
        {
            for (int col = 0; col < right.Columns; col++)
            {
                for (int k = 0; k < 6; k++)
                {
                    final[row, col] += left[row, k] * right[k, col];
                }
            }
        }
        return final;
    }

    public static Matrix operator *(float scalar, Matrix matrix)
    {
        Matrix final = new Matrix(matrix.Rows, matrix.Columns);
        for (int rows = 0; rows < matrix.Rows; rows++)
        {
            for (int cols = 0; cols < matrix.Columns; cols++)
            {
                final[rows, rows] = scalar * matrix[rows, rows];
            }
        }

        return final;
    }
    public void DebugPrint()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Console.Write($"{Math.Abs(this[r,c]):0E+00} ");
            }
            
            Console.WriteLine();
        }
    }
}