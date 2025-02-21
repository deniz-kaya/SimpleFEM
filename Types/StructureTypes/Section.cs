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
    
    public static Section Dummy => new Section(1, 1);

}