namespace SimpleFEM.Interfaces;

public interface IUIStructureHelper
{
    public Queue<ISceneObject> GetSceneObjects(DrawSettings drawSettings);
}