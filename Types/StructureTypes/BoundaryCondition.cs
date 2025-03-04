namespace SimpleFEM.Types.StructureTypes;

public struct BoundaryCondition
{
    public BoundaryCondition(bool fixedY, bool fixedX, bool fixedRotation)
    {
        this.FixedY = fixedY;
        this.FixedX = fixedX;
        this.FixedRotation = fixedRotation;
    }
    public bool FixedY;
    public bool FixedX;
    public bool FixedRotation;
}