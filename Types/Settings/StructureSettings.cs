namespace SimpleFEM.Types.Settings;

public struct StructureSettings
{
    /// <summary>
    /// Constructor for the StructureSettings struct.
    /// </summary>
    /// <param name="gridSpacing">Length of one side of grid, in metres</param>
    public StructureSettings(float gridSpacing)
    {
        this.gridSpacing = gridSpacing;
    }
    public float gridSpacing;

    public static StructureSettings Default
    {
        get
        {
            return new StructureSettings() {gridSpacing = 0.5f};
        }
    }
}