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
    
    public static Section UB533x312x273 => new Section("UB 533 x 312x 273", 0.00199f, 0.0348f);
    public static Section UC254x254x132 => new Section("UC 254 x 254x 132", 0.000225f, 0.0168f);
    public static Section SHS100x100x5 = new Section("SHS 100 x 100 x 5", 2.71e-6f, 0.00187f); 
    public static Section Dummy => new Section("Dummy", 1, 1);

}