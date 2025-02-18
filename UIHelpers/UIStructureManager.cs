namespace SimpleFEM.UIHelpers;

public class UIStructureManager : StructureManager, IUIStructureHelper
{
    public UIStructureManager(IStructure structure) : base(structure)
    {
                
    }

    public UIStructureManager(IStructure structure, Settings settings) : base(structure, settings)
    {
        
    }

    public Queue<ISceneObject> GetSceneObjects(UserSettings drawSettings)
    {
        // TODO Consider implementing merge sort and binary search for SelectedElements and SelectedNodes
        Queue<ISceneObject> renderQueue = new Queue<ISceneObject>();
        
        return renderQueue;
    }
    
    public void DrawGrid
}