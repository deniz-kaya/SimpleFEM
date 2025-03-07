using System.ComponentModel;
using System.Numerics;
using Raylib_cs;
using SimpleFEM.Base;
using SimpleFEM.Interfaces;
using SimpleFEM.SceneObjects;
using SimpleFEM.Types.StructureTypes;
using SimpleFEM.Extensions;
using SimpleFEM.Types.Settings;

namespace SimpleFEM.Derived;

// TODO not necessarily here but overall, manage protected, private and public fields to make sense

public class UIStructureEditor : StructureEditor, IUIStructureHelper
{
    public Tool CurrentTool { get; private set; }
    
    private Types.StructureTypes.Material CurrentMaterial = Types.StructureTypes.Material.Steel;
    private Section CurrentSection = Section.UB;
    
    public bool Dragging { get; private set; }
    private StructureEditorSettings Settings;
    public Vector2 LastMousePosition { get; private set; }
    
    private int HoveredNode;
    private int HoveredElement;
    public UIStructureEditor(IStructure structure, StructureEditorSettings? settings) : base(structure)
    {   
        ResetHovered();
        ResetSelection();
        Settings = settings ?? StructureEditorSettings.Default;
        CurrentTool = Tool.Mouse_Select;
    }
    public void ResetHovered()
    {
        HoveredNode = -1;
        HoveredElement = -1;
    }
    public bool UpdateHoveredItems()
    {
        ResetHovered();
        HoveredNode = CheckForNodesCloseToPos(LivePos, Settings.IdleSelectionFeather);
        if (HoveredNode == -1)
        {
            HoveredElement = CheckForElementsCloseToPos(LivePos, Settings.IdleSelectionFeather);
            if (HoveredElement == -1)
            {
                return false;
            }
        }

        return true;
    }

    public Vector2 GetMousePositionChange()
    {
        return LivePos - LastMousePosition;
    }
    public void SetLivePos(Vector2 position)
    {
        LastMousePosition = LivePos;
        LivePos = position;
    }
    public void HandleMouseKeyDownEvent()
    {
        switch (CurrentTool)
        {
            case Tool.Select_Elements:
                HandleMultiInput();
                SelectElementsWithinArea();
                break;
            case Tool.Select_Nodes:
                HandleMultiInput();
                SelectNodesWithinArea();
                break;
            case Tool.Add_Element:
                HandleMultiInput();
                break;
            case Tool.Move:
                Dragging = true;
                break;
        }
    }

    public (Vector2? pos,BoundaryCondition bc, Load l) GetHoveredNodeProperties()
    {
        if (HoveredNode != -1)
        {
            Vector2 pos = Structure.GetNode(HoveredNode).Pos;
            return (pos, Structure.GetBoundaryCondition(HoveredNode),
                Structure.GetLoad(HoveredNode));
        }

        return (null, BoundaryCondition.Default, Load.Default);
    }
    public void HandleMouseKeyPressedEvent()
    {
        switch (CurrentTool)
        {
            case Tool.Add_Node:
                Vector2 pos = LivePos.RoundToNearest(Structure.GetStructureSettings().gridSpacing);
                Structure.AddNode(pos);
                break;
            case Tool.Mouse_Select:
                AddHoveredToSelected();
                break;
        }
    }

    public void AddHoveredToSelected()
    {
        if (HoveredNode != -1)
        {
            if (SelectedNodes.Contains(HoveredNode))
            {
                DeselectNode(HoveredNode);
            }
            else
            {
                SelectNode(HoveredNode);
            }
        }

        if (HoveredElement != -1)
        {
            if (SelectedElements.Contains(HoveredElement))
            {
                DeselectElement(HoveredElement);
            }
            else
            {
                SelectElement(HoveredElement);
            }
        }
    }

    public void SwitchTool(Tool newTool)
    {
        switch (newTool)
        {
            //TODO clean up
            case Tool.Add_Node:
                ResetSelection();
                break;
            case Tool.Add_Element:
                ResetSelection();
                break;
            case Tool.Select_Elements:
                break;
            case Tool.Select_Nodes:
                break;
            case Tool.Move:
                break;
            case Tool.Mouse_Select:
                break;
        }

        CurrentTool = newTool;
    }
    public void HandleMouseKeyUpEvent()
    {
        switch (CurrentTool)
        {
            case Tool.Select_Elements:
                FinaliseMultiInput();
                break;
            case Tool.Select_Nodes:
                FinaliseMultiInput();
                break;
            case Tool.Add_Element:
                FinaliseMultiInput();
                AddElementBetweenPositions();
                break;
            case Tool.Move:
                Dragging = false;
                break;
        }
    }
    
    private void AddElementBetweenPositions()
    {
        Vector2 position1 = MultiSelectLockedPos.RoundToNearest(Structure.GetStructureSettings().gridSpacing);
        Vector2 position2 = LivePos.RoundToNearest(Structure.GetStructureSettings().gridSpacing);
        
        //adds node, otherwise returns node index of node at position
        Structure.AddNode(position1, out int node1ID);
        Structure.AddNode(position2, out int node2ID);

        if (node1ID == -1 || node2ID == -1)
        {
            throw new Exception("Something went seriously wrong, did you forget to implement AddNode properly?");
        }
        // TODO change element and section behaviour
        Structure.AddElement(new Element(node1ID, node2ID, CurrentMaterial, CurrentSection));

    }
    private void FinaliseMultiInput()
    {
        MultiInputStarted = false;
    }
    private void HandleMultiInput()
    {
        if (!MultiInputStarted)
        {
            ResetSelection(); //if we are just starting multi input, reset the selection to prevent idleselection items from being in it
            MultiSelectLockedPos = LivePos;
            MultiInputStarted = true;
        }
    }
    
    
    //Rendering logic stuff
    //todo hovered logic
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
            case Tool.Add_Node:
                renderQueue.Enqueue(new SphereObject(LivePos.RoundToNearest(Structure.GetStructureSettings().gridSpacing), drawSettings.selectedNodeColor, drawSettings.nodeRadius));
                break;
            case Tool.Add_Element:
                if (!MultiInputStarted) break;
                float gridSpacing = Structure.GetStructureSettings().gridSpacing;
                renderQueue.Enqueue(new LineObject(
                    MultiSelectLockedPos.RoundToNearest(gridSpacing), 
                    LivePos.RoundToNearest(gridSpacing), 
                    drawSettings.selectedElementColor,
                    drawSettings.elementThickness
                    ));
                break;
            case Tool.Select_Elements:
            case Tool.Select_Nodes:
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
        foreach (int i in Structure.GetElementIndexesSorted())
        {
            if (i == HoveredElement) continue;
            
            Element e = Structure.GetElement(i);
            Vector2 position1 = Structure.GetNode(e.Node1ID).Pos;
            Vector2 position2 = Structure.GetNode(e.Node2ID).Pos;
            
            if (SelectedElements.Contains(i))
            {
                selectedElementsObject.AddLine(position1, position2);
            }
            else
            {
                elementsObject.AddLine(position1, position2);
            }
        }

        if (HoveredElement != -1 && Structure.ValidElementID(HoveredElement))
        {
            Element hoveredElement = Structure.GetElement(HoveredElement);
            Vector2 pos1 = Structure.GetNode(hoveredElement.Node1ID).Pos;
            Vector2 pos2 = Structure.GetNode(hoveredElement.Node2ID).Pos;
            renderQueue.Enqueue(new LineObject(pos1, pos2, drawSettings.hoveredElementColor,
                drawSettings.elementThickness));
        }

        renderQueue.Enqueue(selectedElementsObject);
        renderQueue.Enqueue(elementsObject);
    }
    private void QueueNodeSceneObjects(ref Queue<ISceneObject> renderQueue, DrawSettings drawSettings)
    {
        SpheresObject selectedNodesObject = new SpheresObject(drawSettings.selectedNodeColor, drawSettings.nodeRadius);
        SpheresObject nodesObject= new SpheresObject(drawSettings.nodeColor, drawSettings.nodeRadius);

        foreach (int i in Structure.GetNodeIndexesSorted())
        {
            if (i == HoveredNode) continue;
            Vector2 position = Structure.GetNode(i).Pos;
            if (SelectedNodes.Contains(i))
            {
                selectedNodesObject.AddSphere(position);
            }
            else
            {
                nodesObject.AddSphere(position);
            }
        }

        if (HoveredNode != -1 && Structure.ValidNodeID(HoveredNode))
        {
            Vector2 pos = Structure.GetNode(HoveredNode).Pos;
            renderQueue.Enqueue(new SphereObject(pos, drawSettings.hoveredNodeColor, drawSettings.nodeRadius));
        }
        renderQueue.Enqueue(selectedNodesObject);
        renderQueue.Enqueue(nodesObject);
    }
}