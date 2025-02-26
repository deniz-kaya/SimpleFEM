namespace SimpleFEM.Types.Settings;

public struct StructureSettings
{
    public float gridSpacing;

    public static StructureSettings Default
    {
        get
        {
            return new StructureSettings() {gridSpacing = 50f};
        }
    }
}