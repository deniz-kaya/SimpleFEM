using SimpleFEM.UIHelpers;
using SimpleFEM.Interfaces;

namespace SimpleFEM;

public class UserInterface
{
    private UIStructureManager structureManager;
    private UISceneRenderer sceneRenderer;

    public UserInterface(IStructure structure)
    {
        structureManager = new UIStructureManager(structure);
        sceneRenderer = new UISceneRenderer();
    }
    
    
}