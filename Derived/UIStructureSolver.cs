using System.Data;
using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using SimpleFEM.Base;
using SimpleFEM.Interfaces;
using SimpleFEM.SceneObjects;
using SimpleFEM.Types.Settings;
using SimpleFEM.Types.StructureTypes;
using SQLitePCL;

namespace SimpleFEM.Derived;

public class UIStructureSolver : StructureSolver, IUIStructureHelper
{
    public float ExaggerationFactor;
    
    public UIStructureSolver(IStructure structure, StructureSolverSettings settings = default) : base(structure)
    {
        ExaggerationFactor = 10f;
    }

    private bool displayLoads = false;
    private bool displayBoundaryConditions = false;
    private bool shouldDisplaySolution = false;
    public void DrawOperationWindow()   
    {
        ImGui.Checkbox("View loads", ref displayLoads);
        ImGui.Checkbox("View boundary conditions", ref displayBoundaryConditions);
        
        ImGui.InputFloat("Exaggeration Factor", ref ExaggerationFactor);
        ImGui.Separator();
        if (ImGui.Button("Solve System"))
        {
            if (Solve())
            {
                shouldDisplaySolution = true;
            }
            else
            {
                shouldDisplaySolution = false;
            }
        }
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
        renderQueue.Enqueue(new GridObject(SceneRenderer.SceneGridSlices, SceneRenderer.ScenePixelGridSpacing));

        if (StructureHasBeenChanged)
        {
            shouldDisplaySolution = false;
        }
            
        if (shouldDisplaySolution)
        {
            QueueCurrentSolutionSceneObjects(ref renderQueue, settings);
            return renderQueue;
        }
        
        QueueNodesAndElementsSceneObject(ref renderQueue, settings);

        if (displayBoundaryConditions)
        {
            
        }

        if (displayLoads)
        {
            
        }
        return renderQueue;
    }


    public void QueueCurrentSolutionBCObjects(ref Queue<ISceneObject> renderQueue, DrawSettings settings)
    {
        throw new NotImplementedException();
    }
    public void QueueCurrentSolutionLoadObjects(ref Queue<ISceneObject> renderQueue, DrawSettings settings)
    {
        throw new NotImplementedException();
    }
    public void QueueCurrentSolutionSceneObjects(ref Queue<ISceneObject> renderQueue, DrawSettings settings)
    {
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
    }

    public void QueueNodesAndElementsSceneObject(ref Queue<ISceneObject> renderQueue, DrawSettings settings)
    {
        SpheresObject nodes = new SpheresObject(settings.nodeColor, settings.nodeRadius);
        foreach (int nodeID in structure.GetNodeIndexesSorted())
        {
            nodes.AddSphere(structure.GetNode(nodeID).Pos);
        }

        LinesObject elements = new LinesObject(settings.elementColor, settings.elementThickness);

        foreach (int elementID in structure.GetElementIndexesSorted())
        {
            Element e = structure.GetElement(elementID);
            Vector2 pos1 = structure.GetNode(e.Node1ID).Pos;
            Vector2 pos2 = structure.GetNode(e.Node2ID).Pos;
            elements.AddLine(pos1, pos2);
        }
        renderQueue.Enqueue(elements);
        renderQueue.Enqueue(nodes);
    }
}
