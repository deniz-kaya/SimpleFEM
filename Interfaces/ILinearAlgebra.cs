namespace SimpleFEM.Interfaces;

public interface ILinearAlgebra
{
    public int Rows { get; }
    public int Columns { get; }
    public ref float this[int row, int column] { get; }
}