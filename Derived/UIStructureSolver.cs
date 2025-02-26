using SimpleFEM.Base;
using SimpleFEM.Interfaces;
using SimpleFEM.Types;
using SimpleFEM.Types.Settings;

namespace SimpleFEM.Derived;

public class UIStructureSolver : StructureSolver, IUIStructureHelper
{
    public UIStructureSolver(IStructure structure, StructureSolverSettings settings = default) : base(structure)
    {
        
    }

    public Queue<ISceneObject> GetSceneObjects(DrawSettings settings)
    {
        throw new NotImplementedException();
    }
}