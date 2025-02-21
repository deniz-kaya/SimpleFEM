namespace SimpleFEM.Interfaces;

public interface ILinearAlgebra
{
    public int GetRows();
    public int GetColumns();
    public float this[int row, int column] { get; set; }
}