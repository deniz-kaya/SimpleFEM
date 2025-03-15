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
    public static Material Steel => new Material("Steel",2.1e11f);
    public static Material Aluminium => new Material("Aluminium",6.9e10f);
    public static Material Concrete => new Material("Concrete",1.7e10f);
    
    
    public static Material Dummy => new Material("Dummy",1f);
    
}