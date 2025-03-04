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

    //from eurocode
    public Material(float e)
    {
        this.E = e;
    }
    public static Material Steel => new Material(2.1e11f, 0.3f, 7850f);
    
    public static Material Dummy => new Material(1, 1, 1);
    
}