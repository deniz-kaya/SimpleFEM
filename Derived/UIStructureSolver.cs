using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using SimpleFEM.Base;
using SimpleFEM.Interfaces;
using SimpleFEM.SceneObjects;
using SimpleFEM.Types.Settings;
using SimpleFEM.Types.StructureTypes;

namespace SimpleFEM.Derived;

public class UIStructureSolver : StructureSolver
{
    private float _exaggerationFactor;
    
    public UIStructureSolver(IStructure structure) : base(structure)
    {
        //default exaggeration factor
        _exaggerationFactor = 10f;
    }

    private bool _shouldDisplaySolution;
    public void DrawOperationWindow()   
    {
        ImGui.SeparatorText("Exaggeration");
        ImGui.InputFloat("Exaggeration Factor", ref _exaggerationFactor);
        ImGui.Separator();
        if (ImGui.Button("Solve System"))
        {
            _shouldDisplaySolution = Solve();
        }
    }
    public void HandlePopups()
    {
        //define popups and set states to open according to flags
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
            //display error type and the error message in a locking modal
            ImGui.Text("An error occured while solving system!");
            ImGui.SeparatorText("Error type:");
            ImGui.Text((string)LastError.GetType().Name);
            ImGui.SeparatorText("Error message:");
            ImGui.TextWrapped((string)LastError.Message);
            
            if (ImGui.Button("Close"))
            {
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
    }

    private Dictionary<int, Vector2> GetDisplacedNodePositions()
    {
        Dictionary<int, Vector2> positions = new Dictionary<int, Vector2>();
        List<int> nodeIDs = Structure.GetNodeIndexesSorted();

        //for every node ID in the structure, find the relevant displacements and use them to calculate the new position, store this in a dictionary
        for (int i = 0; i < nodeIDs.Count; i++)
        {

            int nodeID = nodeIDs[i];
            Vector2 currentPos = Structure.GetNode(nodeID).Pos;
            //the starting index of the current node for the 
            int solutionVectorIndex = i * DOF;
            //get the x-y displacement of the node from the solution vector
            Vector2 displacement = new Vector2(CurrentSolution[solutionVectorIndex], CurrentSolution[solutionVectorIndex + 1]);
            //add the displacement exaggerated by the exaggeration factor to the current pos to find the new pos
            Vector2 newPos = currentPos + (displacement * _exaggerationFactor);
            //add new position of the node ID into the dictionary
            positions.Add(nodeID, newPos);
        }

        return positions;
    }
    public Vector2 GetSceneCoordinates(Vector2 realPosition)
    {
        return (realPosition * UISceneRenderer.ScenePixelGridSpacing) / Structure.GetStructureSettings().GridSpacing;
    }
    public Vector2 GetRealCoordinates(Vector2 screenPosition)
    {
        return (screenPosition / UISceneRenderer.ScenePixelGridSpacing) * Structure.GetStructureSettings().GridSpacing;
    }
    public Queue<ISceneObject> GetSceneObjects(DrawSettings settings)
    {
        Queue<ISceneObject> renderQueue = new Queue<ISceneObject>();
        //background
        renderQueue.Enqueue(new BackgroundObject(Color.White));

        //grid
        renderQueue.Enqueue(new GridObject(UISceneRenderer.SceneGridSlices, UISceneRenderer.ScenePixelGridSpacing));
        
        //do not display the solution if the structure has been changed
        if (StructureHasBeenChanged)
        {
            _shouldDisplaySolution = false;
        }
        
        
        if (_shouldDisplaySolution)
        {
            QueueCurrentSolutionSceneObjects(ref renderQueue, settings);
        }
        else
        {
            //show the default objects
            QueueNodesAndElementsSceneObject(ref renderQueue, settings);
        }

        return renderQueue;
    }
    private void QueueCurrentSolutionSceneObjects(ref Queue<ISceneObject> renderQueue, DrawSettings settings)
    {
        //get the displaced node positions
        Dictionary<int, Vector2> displacedNodes = GetDisplacedNodePositions();

        CirclesObject nodes = new CirclesObject(settings.NodeColor, settings.NodeRadius);
        
        //add the displaced nodes into the renbder queue
        foreach (Vector2 v in displacedNodes.Values)
        {
            nodes.AddCircle(GetSceneCoordinates(v));
        }

        LinesObject elements = new LinesObject(settings.ElementColor, settings.ElementThickness);

        //add the elements connecting the displaced nodes into the render queue
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
        CirclesObject nodes = new CirclesObject(settings.NodeColor, settings.NodeRadius);
        //get node positions and add to render queue
        foreach (int nodeID in Structure.GetNodeIndexesSorted())
        {
            nodes.AddCircle(GetSceneCoordinates(Structure.GetNode(nodeID).Pos));
        }

        LinesObject elements = new LinesObject(settings.ElementColor, settings.ElementThickness);

        //get the node positions of the element, and add to render queue
        foreach (int elementID in Structure.GetElementIndexesSorted())
        {
            Element e = Structure.GetElement(elementID);
            Vector2 pos1 = GetSceneCoordinates(Structure.GetNode(e.Node1ID).Pos);
            Vector2 pos2 = GetSceneCoordinates(Structure.GetNode(e.Node2ID).Pos);
            elements.AddLine(pos1, pos2);
        }
        //add nodes last so that it overlaps elements
        renderQueue.Enqueue(elements);
        renderQueue.Enqueue(nodes);
    }
}
