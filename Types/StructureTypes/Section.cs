namespace SimpleFEM.Types.StructureTypes;

public struct Section
{
    public Section(string description, float i, float a)
    {
        Description = description;
        I = i;
        A = a;
    }
    public string Description;
    public float I;
    public float A;
    //533 x 312 x 273
    
    public static Section UB => new Section("UB", 72500f, 348f);
    public static Section Dummy => new Section("Dummy", 1, 1);

}