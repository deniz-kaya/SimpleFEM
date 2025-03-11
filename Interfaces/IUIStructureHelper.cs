using System.Collections.Generic;
using SimpleFEM.Types.Settings;

namespace SimpleFEM.Interfaces;

public interface IUIStructureHelper
{
    public Queue<ISceneObject> GetSceneObjects(DrawSettings drawSettings);
}