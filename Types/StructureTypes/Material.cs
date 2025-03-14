namespace SimpleFEM.Types.StructureTypes;

public struct Material
{
    public Material(string description, float e)
    {
        Description = description;
        E = e;
    }
    public float E;
    public string Description;

    //default materials are from eurocode
    public static Material Steel235 => new Material("Steel 235",2.1e11f);
    public static Material Steel275 => new Material("Steel 275",2.1e11f);
    public static Material Steel355 => new Material("Steel 355",2.1e11f);
    
    
    public static Material Dummy => new Material("Dummy",1f);
    
}