namespace SimpleFEM.Types;

public struct StructureEditorSettings
{
    public float IdleSelectionFeather;

    public static StructureEditorSettings Default
    {
        get
        {
            return new StructureEditorSettings() { IdleSelectionFeather = 10f };
        }
    } 
}