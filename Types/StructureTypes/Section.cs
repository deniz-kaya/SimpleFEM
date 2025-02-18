namespace SimpleFEM.Types.StructureTypes;

public struct Section
{
    public Section(double i, double a)
    {
        I = i;
        A = a;
    }
    public double I;
    public double A;
    
    public static Section Dummy => new Section(1, 1);

}