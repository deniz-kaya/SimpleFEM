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
}
