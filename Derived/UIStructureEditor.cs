using System.ComponentModel;
using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using SimpleFEM.Base;
using SimpleFEM.Interfaces;
using SimpleFEM.SceneObjects;
using SimpleFEM.Types.StructureTypes;
using SimpleFEM.Extensions;
using SimpleFEM.Types.Settings;
using Material = SimpleFEM.Types.StructureTypes.Material;

namespace SimpleFEM.Derived;

// TODO not necessarily here but overall, manage protected, private and public fields to make sense

public class UIStructureEditor : StructureEditor, IUIStructureHelper
{
    public Tool CurrentTool { get; private set; }

    private int CurrentMaterialID;
    private int CurrentSectionID;
    
    public bool Dragging { get; private set; }
    private StructureEditorSettings Settings;
    public Vector2 LastMousePosition { get; private set; }
    
    private int HoveredNode;
    private int HoveredElement;
    public UIStructureEditor(IStructure structure, StructureEditorSettings settings) : base(structure)
    {   
        ResetHovered();
        ResetSelection();

        //todo maybe find a better way to do this rather than getting the whole list of elements
        CurrentMaterialID = structure.GetMaterialIndexesSorted().First();
        CurrentSectionID = structure.GetSectionIndexesSorted().First();
        Settings = settings;
        CurrentTool = Tool.Mouse_Select;
    }

    //imgui windows
    public Vector2 GetSceneCoordinates(Vector2 realPosition)
    {
        return (realPosition * SceneRenderer.ScenePixelGridSpacing) / Structure.GetStructureSettings().gridSpacing;
    }
    public Vector2 GetRealCoordinates(Vector2 screenPosition)
    {
        return (screenPosition / SceneRenderer.ScenePixelGridSpacing) * Structure.GetStructureSettings().gridSpacing;
    }

    public void DrawOperationWindow()
    {
        MaterialSelectComboBox();
        
        SectionSelectComboBox();
    }
    public void DrawHoveredPropertiesViewer()
    {
        if (HoveredElement == -1 && HoveredNode == -1)
        {
            ImGui.Text("Nothing hovered!");
            return;
        }

        if (HoveredNode != -1)
        {
            Vector2 pos = Structure.GetNode(HoveredNode).Pos;
            BoundaryCondition bc = Structure.GetBoundaryCondition(HoveredNode);
            Load l = Structure.GetLoad(HoveredNode);
            ImGui.Text($"Pos: {pos.ToString()}");
            ImGui.NewLine();
            ImGui.SeparatorText("Boundary Condition");
            if (!bc.IsDefault)
            {
                if (ImGui.BeginTable("Boundary Conditions", 2, ImGuiTableFlags.Borders))
                {
                    ImGui.TableNextColumn();
                    ImGui.Text("Fixed X");
                    ImGui.TableNextColumn();
                    ImGui.Text(bc.FixedX.ToString());
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text("Fixed Y");
                    ImGui.TableNextColumn();
                    ImGui.Text(bc.FixedY.ToString());
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text("Fixed Rotation");
                    ImGui.TableNextColumn();
                    ImGui.Text(bc.FixedRotation.ToString());

                    ImGui.EndTable();

                }
            }
            else
            {
                ImGui.Text("Node has no boundary conditions!");
            }

            ImGui.NewLine();
            ImGui.SeparatorText("Load");
            if (!l.IsDefault)
            {
                if (ImGui.BeginTable("Load", 2, ImGuiTableFlags.Borders))
                {
                    //TOdo naming
                    ImGui.TableNextColumn();
                    ImGui.Text("X Load");
                    ImGui.TableNextColumn();
                    ImGui.Text(l.ForceX.ToString());
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text("Y Load");
                    ImGui.TableNextColumn();
                    ImGui.Text(l.ForceY.ToString());
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text("Moment");
                    ImGui.TableNextColumn();
                    ImGui.Text(l.Moment.ToString());

                    ImGui.EndTable();

                }
            }
            else
            {
                ImGui.Text("Node has no loads!");
            }
        }
        else if (HoveredElement != -1)
        {
            Element e = Structure.GetElement(HoveredElement);
            Material mat = Structure.GetMaterial(e.MaterialID);
            Section sect = Structure.GetSection(e.SectionID);
            
            ImGui.SeparatorText("Material");
            ImGui.Text(mat.Description);
            if (ImGui.BeginTable("Material Properties", 2, ImGuiTableFlags.Borders))
            {
                ImGui.TableNextColumn();
                ImGui.Text("E");
                ImGui.TableNextColumn();
                ImGui.Text(mat.E.ToString());
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text("Yield");
                ImGui.TableNextColumn();
                ImGui.Text(mat.Yield.ToString());
                
                ImGui.EndTable();

            }
            ImGui.NewLine();
            ImGui.SeparatorText("Section");
            ImGui.Text(sect.Description);
            if (ImGui.BeginTable("Section Properties", 2, ImGuiTableFlags.Borders))
            {
                ImGui.TableNextColumn();
                ImGui.Text("A");
                ImGui.TableNextColumn();
                ImGui.Text(sect.A.ToString());
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text("I");
                ImGui.TableNextColumn();
                ImGui.Text(sect.I.ToString());
                
                ImGui.EndTable();

            }
        }
    }
    public void MaterialSelectComboBox()
    {
        if (ImGui.BeginCombo("Material", Structure.GetMaterial(CurrentMaterialID).Description, ImGuiComboFlags.WidthFitPreview))
        {
            foreach (int i in Structure.GetMaterialIndexesSorted())
            {
                if (ImGui.Selectable(Structure.GetMaterial(i).Description, CurrentMaterialID == i))
                {
                    CurrentMaterialID = i;
                }

                if (CurrentMaterialID == i)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }
    }
    public void SectionSelectComboBox()
    {
        if (ImGui.BeginCombo("Section", Structure.GetSection(CurrentSectionID).Description, ImGuiComboFlags.WidthFitPreview))
        {
            foreach (int i in Structure.GetSectionIndexesSorted())
            {
                if (ImGui.Selectable(Structure.GetSection(i).Description, CurrentSectionID == i))
                {
                    CurrentSectionID = i;
                }

                if (CurrentSectionID == i)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }
    }
    
    //imgui popups
    //Load Editor
    private float loadX, loadY, moment;
    //Boundary Condition Editor
    private bool fixedX, fixedY, fixedMoment;
    //---------
    //Flags
    private bool openBCEditor, openLoadEditor;
    public bool OpenAddMaterialModal, OpenAddSectionModal;
    public void HandlePopups()
    {
        DefineAddSectionlModal();
        DefineAddMaterialModal();
        DefineLoadEditorModal();
        DefineBoundaryConditionEditorModal();
        DefineSelectedNodePopup();

        
        if (openBCEditor)
        {
            ImGui.OpenPopup("Boundary Condition Editor");
            openBCEditor = false;
        }

        if (openLoadEditor)
        {
            ImGui.OpenPopup("Load Editor");
            openLoadEditor = false;
        }

        if (OpenAddSectionModal)
        {
            ImGui.OpenPopup("Add Section");
            OpenAddSectionModal = false;
        }

        if (OpenAddMaterialModal)
        {
            ImGui.OpenPopup("Add Material");
            OpenAddMaterialModal = false;
        }
    }

    public void DefineSelectedNodePopup()
    {
        if (ImGui.BeginPopup("SelectedNodePopup"))
        {
            if (ImGui.Selectable("Delete node(s)"))
            {
                DeleteSelectedNodes();
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.Selectable("Edit node load(s)"))
            {
                ImGui.CloseCurrentPopup();
                openLoadEditor = true;
            }

            if (ImGui.Selectable("Edit node boundary condition(s)"))
            {
                ImGui.CloseCurrentPopup();
                openBCEditor = true;
            }
            ImGui.EndPopup();
        }
    }
    
    public void DefineLoadEditorModal()
    {
        if (ImGui.BeginPopupModal("Load Editor", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text($"Editing load for {SelectedNodeCount} node(s)");
            ImGui.InputFloat("Load in X", ref loadX);
            ImGui.InputFloat("Load in Y", ref loadY);
            ImGui.InputFloat("Moment", ref moment);
            if (ImGui.Button("Reset values to default"))
            {
                loadX = 0;
                loadY = 0;
                moment = 0;
            }
            ImGui.Separator();
            if (ImGui.Button("Add Load(s)"))
            {
                AddLoadToSelectedNodes(new Load(loadX, loadY, moment));
                loadX = 0;
                loadY = 0;
                moment = 0;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
        
    }

    public void DefineBoundaryConditionEditorModal()
    {
        
        if (ImGui.BeginPopupModal("Boundary Condition Editor", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text($"Editing boundary condition for {SelectedNodeCount} node(s)");
            ImGui.Checkbox("Fixed X", ref fixedX);
            ImGui.Checkbox("Fixed Y", ref fixedY);
            ImGui.Checkbox("Fixed Moment", ref fixedMoment);

            if (ImGui.Button("Reset to default"))
            {
                fixedX = false;
                fixedY = false;
                fixedMoment = false;
            }
            ImGui.Separator();
            if (ImGui.Button("Add BC(s)"))
            {
                AddBoundaryConditionToSelectedNodes(new BoundaryCondition(fixedX, fixedY, fixedMoment));
                fixedX = false;
                fixedY = false;
                fixedMoment = false;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
    }

    private const int DescriptionMaxLength = 160;
    private string addMaterialDescription = String.Empty;
    private float addMaterialE = 0;
    private float addMaterialYield = 0;
    public void DefineAddMaterialModal()
    {
        if (ImGui.BeginPopupModal("Add Material", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.InputText("Description", ref addMaterialDescription, DescriptionMaxLength);
            ImGui.InputFloat("E", ref addMaterialE);
            ImGui.InputFloat("Yield", ref addMaterialYield);
            ImGui.Separator();
            if (ImGui.Button("Add material"))
            {
                Structure.AddMaterial(new Material(addMaterialDescription, addMaterialE, addMaterialYield));
                addMaterialDescription = String.Empty;
                addMaterialE = 0;
                addMaterialYield = 0;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Close"))
            {
                addMaterialDescription = String.Empty;
                addMaterialE = 0;
                addMaterialYield = 0;
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
    }

    private string addSectionDescription = "";
    private float addSectionI = 0;
    private float addSectionA = 0;
    
    public void DefineAddSectionlModal()
    {
        if (ImGui.BeginPopupModal("Add Section", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.InputText("Description", ref addSectionDescription, DescriptionMaxLength);
            ImGui.InputFloat("I", ref addSectionI);
            ImGui.InputFloat("A", ref addSectionA);
            ImGui.Separator();
            if (ImGui.Button("Add section"))
            {
                Structure.AddSection(new Section(addSectionDescription, addSectionI, addSectionA));
                addSectionDescription = String.Empty;
                addSectionI = 0;
                addSectionA = 0;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Close"))
            {
                addSectionDescription = String.Empty;
                addSectionI = 0;
                addSectionA = 0;
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
    }
    
    
    //scene editor helpers
    private void ResetHovered()
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
        LivePos = GetRealCoordinates(position);
    }
    private void AddHoveredToSelected()
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
        }

        CurrentTool = newTool;
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
        Structure.AddElement(new Element(node1ID, node2ID, CurrentMaterialID, CurrentSectionID));

    }
    
    //input handling
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
        }
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
    public void HandleMouseKeyUpEvent()
    {
        switch (CurrentTool)
        {
            case Tool.Select_Elements:
            case Tool.Select_Nodes:
                FinaliseMultiInput();
                break;
            case Tool.Add_Element:
                FinaliseMultiInput();
                AddElementBetweenPositions();
                break;
        }
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
    public Queue<ISceneObject> GetSceneObjects(DrawSettings drawSettings)
    {
        Queue<ISceneObject> renderQueue = new Queue<ISceneObject>();
        //background
        renderQueue.Enqueue(new BackgroundObject(Color.White));
        
        //grid
        //todo base this off of somethign else? it feels uncomfortable for it to be based on scenepixelgridspacing
        renderQueue.Enqueue(new GridObject(SceneRenderer.SceneGridSlices, SceneRenderer.ScenePixelGridSpacing));
        
        //Elements
        QueueElementSceneObjects(ref renderQueue, drawSettings);
        
        //Nodes
        QueueNodeSceneObjects(ref renderQueue, drawSettings);
        
        //Tool based extra rendering
        switch (CurrentTool)
        {
            case Tool.Add_Node:
                renderQueue.Enqueue(new SphereObject(LivePos.RoundToNearest(Structure.GetStructureSettings().gridSpacing), drawSettings.selectedNodeColor, drawSettings.nodeRadius));
                break;
            case Tool.Add_Element:
                if (!MultiInputStarted) break;
                float gridSpacing = Structure.GetStructureSettings().gridSpacing;
                renderQueue.Enqueue(new LineObject(
                    GetSceneCoordinates(MultiSelectLockedPos.RoundToNearest(gridSpacing)), 
                    GetSceneCoordinates(LivePos.RoundToNearest(gridSpacing)), 
                    drawSettings.selectedElementColor,
                    drawSettings.elementThickness
                    ));
                break;
            case Tool.Select_Elements:
            case Tool.Select_Nodes:
                if (MultiInputStarted)
                {
                    renderQueue.Enqueue(new SelectionBoxObject(GetSceneCoordinates(MultiSelectLockedPos), GetSceneCoordinates(LivePos), drawSettings.selectionBoxColor));
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
            Vector2 position1 = GetSceneCoordinates(Structure.GetNode(e.Node1ID).Pos);
            Vector2 position2 = GetSceneCoordinates(Structure.GetNode(e.Node2ID).Pos);
            
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
            Vector2 pos1 = GetSceneCoordinates(Structure.GetNode(hoveredElement.Node1ID).Pos);
            Vector2 pos2 = GetSceneCoordinates(Structure.GetNode(hoveredElement.Node2ID).Pos);
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
            Vector2 position = GetSceneCoordinates(Structure.GetNode(i).Pos);
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
            Vector2 pos = GetSceneCoordinates(Structure.GetNode(HoveredNode).Pos);
            renderQueue.Enqueue(new SphereObject(pos, drawSettings.hoveredNodeColor, drawSettings.nodeRadius));
        }
        renderQueue.Enqueue(selectedNodesObject);
        renderQueue.Enqueue(nodesObject);
    }
}