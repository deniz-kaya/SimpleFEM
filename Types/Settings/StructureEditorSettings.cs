namespace SimpleFEM.Types.Settings;

public struct StructureEditorSettings
{
    public float HoveringDistanceTreshold;

    public StructureEditorSettings(float hoveringDistanceTreshold)
    {
        HoveringDistanceTreshold = hoveringDistanceTreshold;
    }
    public static StructureEditorSettings Default => new StructureEditorSettings(10f);
}