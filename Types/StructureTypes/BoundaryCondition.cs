using System.Security.AccessControl;

namespace SimpleFEM.Types.StructureTypes;

public struct BoundaryCondition
{
    public BoundaryCondition(bool fixedX, bool fixedY, bool fixedRotation)
    {
        FixedX = fixedX;
        FixedY = fixedY;
        FixedRotation = fixedRotation;
    }
    public bool FixedY;
    public bool FixedX;
    public bool FixedRotation;
    public bool IsDefault => !FixedX && !FixedY && !FixedRotation;
    public static BoundaryCondition Default => new BoundaryCondition(false, false, false);
}