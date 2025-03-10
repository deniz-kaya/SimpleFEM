using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using SimpleFEM.Base;
using SimpleFEM.Interfaces;
using SimpleFEM.SceneObjects;
using SimpleFEM.Types.Settings;
using SimpleFEM.Types.StructureTypes;

namespace SimpleFEM.Derived;

public class UIStructureSolver : StructureSolver, IUIStructureHelper
{
    private float _exaggerationFactor;
    
    public UIStructureSolver(IStructure structure, StructureSolverSettings settings = default) : base(structure)
    {
        _exaggerationFactor = 10f;
    }

    private bool _displayLoads;
    private bool _displayBoundaryConditions;
    private bool _shouldDisplaySolution;
    public void DrawOperationWindow()   
    {
        ImGui.Checkbox("View loads", ref _displayLoads);
        ImGui.Checkbox("View boundary conditions", ref _displayBoundaryConditions);
        
        ImGui.InputFloat("Exaggeration Factor", ref _exaggerationFactor);
        ImGui.Separator();
        if (ImGui.Button("Solve System"))
        {
            _shouldDisplaySolution = Solve();
        }
    }
    public void HandlePopups()
    {
        DefineSolverErrorModal();
        if (ErrorDuringSolution)
        {
            ImGui.OpenPopup("Solver Error");
            ErrorDuringSolution = false;
        }
    }
    private void DefineSolverErrorModal()
    {
        if (ImGui.BeginPopupModal("Solver Error", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text("An error occured while solving system!");
            ImGui.SeparatorText("Error type:");
            ImGui.Text(LastError.GetType().Name);
            ImGui.SeparatorText("Error message:");
            ImGui.TextWrapped(LastError.Message);
            
            if (ImGui.Button("Close"))
            {
                ImGui.CloseCurrentPopup();
            }
        }
    }
    private Dictionary<int, Vector2> GetDisplacedNodePositions()
    {
        Dictionary<int, Vector2> positions = new Dictionary<int, Vector2>();
        List<int> nodeIDs = Structure.GetNodeIndexesSorted();

        for (int i = 0; i < nodeIDs.Count; i++)
        {
            int nodeID = nodeIDs[i];
            Vector2 currentPos = Structure.GetNode(nodeID).Pos;
            int solutionVectorIndex = i * DOF;
            Vector2 displacement = new Vector2(CurrentSolution[solutionVectorIndex], CurrentSolution[solutionVectorIndex + 1]);
            Vector2 newPos = currentPos + (displacement * _exaggerationFactor);
            positions.Add(nodeID, newPos);
        }

        return positions;
    }
    public Vector2 GetSceneCoordinates(Vector2 realPosition)
    {
        return (realPosition * SceneRenderer.ScenePixelGridSpacing) / Structure.GetStructureSettings().GridSpacing;
    }
    public Vector2 GetRealCoordinates(Vector2 screenPosition)
    {
        return (screenPosition / SceneRenderer.ScenePixelGridSpacing) * Structure.GetStructureSettings().GridSpacing;
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
            _shouldDisplaySolution = false;
        }
            
        if (_shouldDisplaySolution)
        {
            QueueCurrentSolutionSceneObjects(ref renderQueue, settings);
            return renderQueue;
        }
        
        QueueNodesAndElementsSceneObject(ref renderQueue, settings);

        if (_displayBoundaryConditions)
        {
            
        }

        if (_displayLoads)
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
    private void QueueCurrentSolutionSceneObjects(ref Queue<ISceneObject> renderQueue, DrawSettings settings)
    {
        Dictionary<int, Vector2> displacedNodes = GetDisplacedNodePositions();

        SpheresObject nodes = new SpheresObject(settings.NodeColor, settings.NodeRadius);
        foreach (Vector2 v in displacedNodes.Values)
        {
            nodes.AddSphere(GetSceneCoordinates(v));
        }

        LinesObject elements = new LinesObject(settings.ElementColor, settings.ElementThickness);

        foreach (int i in Structure.GetElementIndexesSorted())
        {
            Element e = Structure.GetElement(i);
            elements.AddLine(GetSceneCoordinates(displacedNodes[e.Node1ID]), GetSceneCoordinates(displacedNodes[e.Node2ID]));
        }

        //draw nodes last so that it overlaps the elements
        renderQueue.Enqueue(elements);
        renderQueue.Enqueue(nodes);  
    }

    private void QueueNodesAndElementsSceneObject(ref Queue<ISceneObject> renderQueue, DrawSettings settings)
    {
        SpheresObject nodes = new SpheresObject(settings.NodeColor, settings.NodeRadius);
        foreach (int nodeID in Structure.GetNodeIndexesSorted())
        {
            nodes.AddSphere(GetSceneCoordinates(Structure.GetNode(nodeID).Pos));
        }

        LinesObject elements = new LinesObject(settings.ElementColor, settings.ElementThickness);

        foreach (int elementID in Structure.GetElementIndexesSorted())
        {
            Element e = Structure.GetElement(elementID);
            Vector2 pos1 = GetSceneCoordinates(Structure.GetNode(e.Node1ID).Pos);
            Vector2 pos2 = GetSceneCoordinates(Structure.GetNode(e.Node2ID).Pos);
            elements.AddLine(pos1, pos2);
        }
        renderQueue.Enqueue(elements);
        renderQueue.Enqueue(nodes);
    }
}
