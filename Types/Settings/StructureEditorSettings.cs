namespace SimpleFEM.Types.Settings;

public struct StructureEditorSettings
{
    public float IdleSelectionFeather;

    public static StructureEditorSettings Default
    {
        get
        {
            return new StructureEditorSettings() { IdleSelectionFeather = 0.05f };
        }
    } 
}