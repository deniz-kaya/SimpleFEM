namespace SimpleFEM.Types.StructureTypes;

public struct Section
{
    public Section(float i, float a)
    {
        I = i;
        A = a;
    }
    public float I;
    public float A;
    //533 x 312 x 273
    
    public static Section UB => new Section(72500f, 348f);
    public static Section Dummy => new Section(1, 1);

}