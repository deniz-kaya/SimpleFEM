namespace SimpleFEM.Types.StructureTypes;

public struct Material
{
    public Material(double e, double poisson, double density)
    {
        E = e;
        Poisson = poisson;
        Density = density;
    }
    public double E;
    public double Poisson;
    public double Density;

    public static Material Dummy => new Material(1, 1, 1);
    
}