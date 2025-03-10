namespace SimpleFEM.Types.Settings;

public struct StructureEditorSettings
{
    public float HoveringDistanceTreshold;

    public static StructureEditorSettings Default => new StructureEditorSettings() { HoveringDistanceTreshold = 10f };
}