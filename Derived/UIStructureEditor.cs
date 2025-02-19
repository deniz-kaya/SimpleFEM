using System.ComponentModel;
using System.Numerics;
using Raylib_cs;
using SimpleFEM.Base;
using SimpleFEM.Interfaces;
using SimpleFEM.SceneObjects;
using SimpleFEM.Types;
using SimpleFEM.Types.StructureTypes;
using SimpleFEM.Extensions;
namespace SimpleFEM.Derived;

// TODO not necessarily here but overall, manage protected, private and public fields to make sense

public class UIStructureEditor : StructureEditor, IUIStructureHelper
{
    public Tool CurrentTool { get; private set; }
    private StructureEditorSettings Settings;
    
    public UIStructureEditor(IStructure structure, StructureEditorSettings? settings) : base(structure)
    {   
        Settings = settings ?? StructureEditorSettings.Default;
        DoIdleSelection = true;
        CurrentTool = Tool.None;
    }
    /// <summary>
    /// Tries selecting nodes around the position, if not found tries selecting elements instead.
    /// This order is to make sure that nodes can also be selected, as otherwise their valid selecting areas would mostly be covered by elements' ones.
    /// </summary>
    /// <param name="position">the position to select around</param>
    /// <returns>True if selection was successful, false if otherwise.</returns>
    public bool IdleSelection()
    {
        if (!DoIdleSelection) return false;
        if (MultiInputStarted) return false;
        if (SelectedNodes.Count + SelectedElements.Count > 1) return false;
        if (!SelectNearbyNode(LivePos, Settings.IdleSelectionFeather))
        {
            return SelectNearbyElement(LivePos, Settings.IdleSelectionFeather);
        }
        return true;
    }

    public void SetLivePos(Vector2 position)
    {
        LivePos = position;
    }
    public void HandleMouseKeyDownEvent()
    {
        switch (CurrentTool)
        {
            case Tool.SelectElements:
                
                HandleMultiInput();
                SelectElementsWithinArea();
                break;
            case Tool.SelectNodes:
                HandleMultiInput();
                SelectNodesWithinArea();
                break;
            case Tool.AddElement:
                HandleMultiInput();
                break;
        }
    }

    public void HandleMouseKeyPressedEvent()
    {
        switch (CurrentTool)
        {
            case Tool.AddNode:
                Vector2 pos = LivePos.RoundToNearest(Structure.GetStructureSettings().gridSpacing);
                Console.WriteLine(Structure.AddNode(pos));
                break;
        }
    }  
    public void SwitchTool(Tool newTool)
    {
        switch (newTool)
        {
            case Tool.AddNode:
                ResetSelection();
                DoIdleSelection = false;
                break;
            case Tool.AddElement:
                ResetSelection();
                DoIdleSelection = false;
                break;
            case Tool.Move:
                DoIdleSelection = true;
                break;
        }

        CurrentTool = newTool;
    }
    public void HandleMouseKeyUpEvent()
    {
        switch (CurrentTool)
        {
            case Tool.SelectElements:
                FinaliseMultiInput();
                break;
            case Tool.SelectNodes:
                FinaliseMultiInput();
                break;
            case Tool.AddElement:
                FinaliseMultiInput();
                AddElementBetweenPositions();
                break;
        }
    }
    
    private void AddElementBetweenPositions()
    {
        Vector2 position1 = MultiSelectLockedPos.RoundToNearest(Structure.GetStructureSettings().gridSpacing);
        Vector2 position2 = LivePos.RoundToNearest(Structure.GetStructureSettings().gridSpacing);
        if (position1 == position2) return;
        // useless implementation
        // int position1index = -1;
        // int position2index = -1;
        // foreach (int i in Structure.GetNodeIndexesSorted())
        // {
        //     Vector2 nodePosition = Structure.GetNode(i).Pos;
        //     if (position1 == nodePosition)
        //     {
        //         position1Exists = true;
        //     }
        //     else if (position2 == nodePosition)
        //     {
        //         position2Exists = true;
        //     }
        //
        //     if (position1Exists && position2Exists)
        //     {
        //         break;
        //     }
        // }
        //
        // if (!position1Exists)
        // {
        //     Structure.AddNode(position1);
        // }
        //
        // if (!position2Exists)
        // {
        //     Structure.AddNode(position2);
        // }
        // Structure.AddElement(new Element())

        //if a node exists on the position, AddNode(p, out ID) returns the ID of the existing node.
        int node1ID = -1;
        Structure.AddNode(position1, out node1ID);
        int node2ID = -1;
        Structure.AddNode(position2, out node2ID);

        if (node1ID == -1 || node2ID == -1)
        {
            throw new Exception("Something went seriously wrong, did you forget to implement AddNode properly?");
        }
        // TODO change element and section behaviour
        Structure.AddElement(new Element(node1ID, node2ID));

    }
    private void FinaliseMultiInput()
    {
        MultiInputStarted = false;
        if (EmptySelection && CurrentTool != Tool.AddElement)
        {
            DoIdleSelection = true;
        }
    }
    private void HandleMultiInput()
    {
        if (!MultiInputStarted)
        {
            MultiSelectLockedPos = LivePos;
            DoIdleSelection = false;
            MultiInputStarted = true;
        }
    }
    
    
    //Rendering logic stuff
    public Queue<ISceneObject> GetSceneObjects(DrawSettings drawSettings)
    {
        Queue<ISceneObject> renderQueue = new Queue<ISceneObject>();
        //background
        renderQueue.Enqueue(new BackgroundObject(Color.White));
        
        //grid
        // TODO variables
        renderQueue.Enqueue(new GridObject(200, 50f));
        
        //Elements
        QueueElementSceneObjects(ref renderQueue, drawSettings);
        
        //Nodes
        QueueNodeSceneObjects(ref renderQueue, drawSettings);
        
        //Tool based
        switch (CurrentTool)
        {
            case Tool.AddNode:
                renderQueue.Enqueue(new SphereObject(LivePos, drawSettings.selectedNodeColor, drawSettings.nodeRadius));
                break;
            case Tool.AddElement:
                if (!MultiInputStarted) break;
                float gridSpacing = Structure.GetStructureSettings().gridSpacing;
                renderQueue.Enqueue(new LineObject(
                    MultiSelectLockedPos.RoundToNearest(gridSpacing), 
                    LivePos.RoundToNearest(gridSpacing), 
                    drawSettings.selectedElementColor,
                    drawSettings.elementThickness
                    ));
                break;
            case Tool.SelectElements:
            case Tool.SelectNodes:
                if (MultiInputStarted)
                {
                    renderQueue.Enqueue(new SelectionBoxObject(MultiSelectLockedPos, LivePos, drawSettings.selectionBoxColor));
                }

                break;
        }

        return renderQueue;
    }
    private void QueueElementSceneObjects(ref Queue<ISceneObject> renderQueue, DrawSettings drawSettings)
    {
        LinesObject selectedElementsObject = new LinesObject(drawSettings.selectedElementColor, drawSettings.elementThickness);
        LinesObject elementsObject = new LinesObject(drawSettings.elementColor, drawSettings.elementThickness);

        int selectedElementListIndexTracker = 0;
        foreach (int i in Structure.GetElementIndexesSorted())
        {
            Element e = Structure.GetElement(i);
            Vector2 position1 = Structure.GetNode(e.Node1ID).Pos;
            Vector2 position2 = Structure.GetNode(e.Node2ID).Pos;
            
            if (SelectedElements.Count > selectedElementListIndexTracker && i == SelectedElements.ElementAt(selectedElementListIndexTracker))
            {
                selectedElementListIndexTracker++;
                selectedElementsObject.AddLine(position1, position2);
            }
            else
            {
                elementsObject.AddLine(position1, position2);
            }
        }
        
        renderQueue.Enqueue(selectedElementsObject);
        renderQueue.Enqueue(elementsObject);
    }
    private void QueueNodeSceneObjects(ref Queue<ISceneObject> renderQueue, DrawSettings drawSettings)
    {
        SpheresObject selectedNodesObject = new SpheresObject(drawSettings.selectedNodeColor, drawSettings.nodeRadius);
        SpheresObject nodesObject= new SpheresObject(drawSettings.nodeColor, drawSettings.nodeRadius);

        int selectedNodesListIndexTracker = 0;
        foreach (int i in Structure.GetNodeIndexesSorted())
        {
            Vector2 position = Structure.GetNode(i).Pos;
            if (SelectedNodes.Count > selectedNodesListIndexTracker && i == SelectedNodes.ElementAt(selectedNodesListIndexTracker))
            {
                selectedNodesListIndexTracker++;
                selectedNodesObject.AddSphere(position);
            }
            else
            {
                nodesObject.AddSphere(position);
            }
        }
        renderQueue.Enqueue(selectedNodesObject);
        renderQueue.Enqueue(nodesObject);
    }
}