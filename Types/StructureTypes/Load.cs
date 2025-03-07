namespace SimpleFEM.Types.StructureTypes;

public struct Load
{
    public Load(float forceX, float forceY, float moment)
    {
        this.ForceX = forceX;
        this.ForceY = forceY;
        this.Moment = moment;
    }
    public float ForceX;
    public float ForceY;
    public float Moment;
    public bool IsDefault => ForceX == 0 && ForceY == 0 && Moment == 0;
    public static Load Default => new(0f, 0f, 0f);
}
