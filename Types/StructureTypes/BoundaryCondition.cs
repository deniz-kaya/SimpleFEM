namespace SimpleFEM.Types.StructureTypes;

public struct BoundaryCondition
{
    public BoundaryCondition(bool fixedY, bool fixedX, bool fixedMoment)
    {
        this.FixedY = fixedY;
        this.FixedX = fixedX;
        this.FixedMoment = fixedMoment;
    }
    public bool FixedY;
    public bool FixedX;
    public bool FixedMoment;
}