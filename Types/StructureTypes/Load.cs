namespace SimpleFEM.Types.StructureTypes;

public struct Load
{
    public Load(double forceX, double forceY, double moment)
    {
        this.ForceX = forceX;
        this.ForceY = forceY;
        this.Moment = moment;
    }
    public double ForceX;
    public double ForceY;
    public double Moment;
}
