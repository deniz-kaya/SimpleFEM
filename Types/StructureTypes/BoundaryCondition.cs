using System.Security.AccessControl;

namespace SimpleFEM.Types.StructureTypes;

public struct BoundaryCondition
{
    public BoundaryCondition(bool fixedX, bool fixedY, bool fixedRotation)
    {
        this.FixedX = fixedX;
        this.FixedY = fixedY;
        this.FixedRotation = fixedRotation;
    }
    public bool FixedY;
    public bool FixedX;
    public bool FixedRotation;
    public bool IsDefault => !FixedX && !FixedY && !FixedRotation;
    public static BoundaryCondition Default => new BoundaryCondition(false, false, false);
}