using System.Numerics;
using Raylib_cs;
using SimpleFEM.Base;
using SimpleFEM.Interfaces;
using SimpleFEM.SceneObjects;
using SimpleFEM.Types;
using SimpleFEM.Types.Settings;
using SimpleFEM.Types.StructureTypes;

namespace SimpleFEM.Derived;

public class UIStructureSolver : StructureSolver, IUIStructureHelper
{
    public float ExaggerationFactor;
    public UIStructureSolver(IStructure structure, StructureSolverSettings settings = default) : base(structure)
    {
        
    }

    private Dictionary<int, Vector2> GetDisplacedNodePositions()
    {
        Dictionary<int, Vector2> positions = new Dictionary<int, Vector2>();
        List<int> nodeIDs = structure.GetNodeIndexesSorted();

        for (int i = 0; i < nodeIDs.Count; i++)
        {
            int nodeID = nodeIDs[i];
            Vector2 currentPos = structure.GetNode(nodeID).Pos;
            int solutionVectorIndex = i * DOF;
            Vector2 displacement = new Vector2(CurrentSolution[solutionVectorIndex], CurrentSolution[solutionVectorIndex + 1]);
            Vector2 newPos = currentPos + (displacement * ExaggerationFactor);
            positions.Add(nodeID, newPos);
        }

        return positions;
    }
    
    public Queue<ISceneObject> GetSceneObjects(DrawSettings settings)
    {
        Queue<ISceneObject> renderQueue = new Queue<ISceneObject>();
        //background
        renderQueue.Enqueue(new BackgroundObject(Color.White));

        //grid
        // TODO variables
        renderQueue.Enqueue(new GridObject(200, 50f));
        Dictionary<int, Vector2> displacedNodes = GetDisplacedNodePositions();

        SpheresObject nodes = new SpheresObject(settings.nodeColor, settings.nodeRadius);
        foreach (Vector2 v in displacedNodes.Values)
        {
            nodes.AddSphere(v);
        }

        LinesObject elements = new LinesObject(settings.elementColor, settings.elementThickness);

        foreach (int i in structure.GetElementIndexesSorted())
        {
            Element e = structure.GetElement(i);
            elements.AddLine(displacedNodes[e.Node1ID], displacedNodes[e.Node2ID]);
        }

        //draw nodes last so that it overlaps the elements
        renderQueue.Enqueue(elements);
        renderQueue.Enqueue(nodes);
        return renderQueue;
    }
}
