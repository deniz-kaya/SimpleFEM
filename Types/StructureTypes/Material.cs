namespace SimpleFEM.Types.StructureTypes;

public struct Material
{
    //todo ctor and variable naming conventions
    public Material(string description, float e, float yield)
    {
        Description = description;
        E = e;
        Yield = yield;
    }
    public float E;
    public float Yield;
    public string Description;

    //from eurocode
    public static Material Steel235 => new Material("Steel 235",2.1e11f, 2.35e8f);
    public static Material Steel275 => new Material("Steel 275",2.1e11f, 2.75e8f);
    public static Material Steel355 => new Material("Steel 355",2.1e11f, 3.55e8f);
    
    
    public static Material Dummy => new Material("Dummy",1f ,1f);
    
}