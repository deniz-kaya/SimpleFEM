namespace SimpleFEM.Types.Settings;

public struct StructureSettings
{
    public StructureSettings(float gridSpacing)
    {
        GridSpacing = gridSpacing;
    }
    public float GridSpacing;

    public static StructureSettings Default => new StructureSettings(0.5f);
}