namespace SimpleFEM.Types.Settings;

public struct StructureSettings
{
    /// <summary>
    /// Constructor for the StructureSettings struct.
    /// </summary>
    /// <param name="gridSpacing">Length of one side of grid, in metres</param>
    public StructureSettings(float gridSpacing)
    {
        this.GridSpacing = gridSpacing;
    }
    public float GridSpacing;

    public static StructureSettings Default => new StructureSettings() {GridSpacing = 0.5f};
}