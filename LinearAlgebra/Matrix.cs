using System;
using SimpleFEM.Interfaces;

namespace SimpleFEM.LinearAlgebra;

public struct Matrix
{
    private int rows;
    private int columns;
    //matrix data stored in one dimensional list
    private float[] data;
    public Matrix(int rows, int columns)
    {
        this.rows = rows;
        this.columns = columns;
        data = new float[rows * columns];
    }
    
    public int Rows => rows;
    public int Columns => columns;
    
    //convert two dimensional row-col reference to cells to one dimensional index of the array
    public ref float this[int row, int col] {
        get
        {
            if (row < rows && col < columns && row > -1 && col > -1)
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
        //this is a criteria that is required for matrix multiplication
        if (left.Columns != right.Rows)
        {
            throw new ArgumentException("First matrix columns is not equal to the second matrix rows!");
        }
        //an axn matrix multiplied by a nxb matrix results in an axb matrix
        Matrix final = new Matrix(left.Rows, right.Columns);
        for (int row = 0; row < left.Rows; row++)
        {
            for (int col = 0; col < right.Columns; col++)
            {
                //calculate the dot product of the corresponding row-column pair and add it to the final matrix
                for (int k = 0; k < left.Columns; k++)
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
        //scale each cell in the matrix by given scalar value
        for (int rows = 0; rows < matrix.Rows; rows++)
        {
            for (int cols = 0; cols < matrix.Columns; cols++)
            {
                final[rows, cols] = scalar * matrix[rows, cols];
            }
        }
    
        return final;
    }
}