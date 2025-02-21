namespace SimpleFEM.Types.StructureTypes;

public struct Material
{
    public Material(float e, float poisson, float density)
    {
        E = e;
        Poisson = poisson;
        Density = density;
    }
    public float E;
    public float Poisson;
    public float Density;

    public static Material Dummy => new Material(1, 1, 1);
    
}