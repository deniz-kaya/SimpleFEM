namespace SimpleFEM.Types.StructureTypes;

public struct Material
{
    //todo ctor and variable naming conventions
    public Material(string description, float e)
    {
        Description = description;
        E = e;
    }
    public float E;
    public string Description;

    //from eurocode
    public Material(float e)
    {
        this.E = e;
    }
    public static Material Steel => new Material("Steel",2.1e11f);
    
    public static Material Dummy => new Material("Dummy",1f);
    
}