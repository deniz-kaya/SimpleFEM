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

    private int _currentMaterialID;
    private int _currentSectionID;
    
    private StructureEditorSettings _settings;
    
    private int _hoveredNode;
    private int _hoveredElement;
    public UIStructureEditor(IStructure structure, StructureEditorSettings settings) : base(structure)
    {   
        ResetHovered();
        ResetSelection();

        //todo maybe find a better way to do this rather than getting the whole list of elements
        _currentMaterialID = structure.GetMaterialIndexesSorted().First();
        _currentSectionID = structure.GetSectionIndexesSorted().First();
        _settings = settings;
        CurrentTool = Tool.MouseSelect;
    }

    //imgui windows
    public Vector2 GetSceneCoordinates(Vector2 realPosition)
    {
        return (realPosition * SceneRenderer.ScenePixelGridSpacing) / Structure.GetStructureSettings().GridSpacing;
    }
    public Vector2 GetRealCoordinates(Vector2 screenPosition)
    {
        return (screenPosition / SceneRenderer.ScenePixelGridSpacing) * Structure.GetStructureSettings().GridSpacing;
    }

    public void DrawOperationWindow()
    {
        MaterialSelectComboBox();
        
        SectionSelectComboBox();
    }
    public void DrawHoveredPropertiesViewer()
    {
        if (_hoveredElement == -1 && _hoveredNode == -1)
        {
            ImGui.Text("Nothing hovered!");
            return;
        }

        if (_hoveredNode != -1)
        {
            Vector2 pos = Structure.GetNode(_hoveredNode).Pos;
            BoundaryCondition bc = Structure.GetBoundaryCondition(_hoveredNode);
            Load l = Structure.GetLoad(_hoveredNode);
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
        else if (_hoveredElement != -1)
        {
            Element e = Structure.GetElement(_hoveredElement);
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
        if (ImGui.BeginCombo("Material", Structure.GetMaterial(_currentMaterialID).Description, ImGuiComboFlags.WidthFitPreview))
        {
            foreach (int i in Structure.GetMaterialIndexesSorted())
            {
                if (ImGui.Selectable(Structure.GetMaterial(i).Description, _currentMaterialID == i))
                {
                    _currentMaterialID = i;
                }

                if (_currentMaterialID == i)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }
    }
    public void SectionSelectComboBox()
    {
        if (ImGui.BeginCombo("Section", Structure.GetSection(_currentSectionID).Description, ImGuiComboFlags.WidthFitPreview))
        {
            foreach (int i in Structure.GetSectionIndexesSorted())
            {
                if (ImGui.Selectable(Structure.GetSection(i).Description, _currentSectionID == i))
                {
                    _currentSectionID = i;
                }

                if (_currentSectionID == i)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }
    }
    
    //imgui popups
    //todo naming
    //Load Editor
    private float _loadX, _loadY, _moment;
    //Boundary Condition Editor
    private bool _fixedX, _fixedY, _fixedMoment;
    //---------
    //Flags
    private bool _openBcEditor, _openLoadEditor;
    public bool OpenAddMaterialModal, OpenAddSectionModal;
    public void HandlePopups()
    {
        DefineAddSectionModal();
        DefineAddMaterialModal();
        DefineLoadEditorModal();
        DefineBoundaryConditionEditorModal();
        DefineSelectedNodePopup();

        
        if (_openBcEditor)
        {
            ImGui.OpenPopup("Boundary Condition Editor");
            _openBcEditor = false;
        }

        if (_openLoadEditor)
        {
            ImGui.OpenPopup("Load Editor");
            _openLoadEditor = false;
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

    private void DefineSelectedNodePopup()
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
                _openLoadEditor = true;
            }

            if (ImGui.Selectable("Edit node boundary condition(s)"))
            {
                ImGui.CloseCurrentPopup();
                _openBcEditor = true;
            }
            ImGui.EndPopup();
        }
    }

    private void DefineLoadEditorModal()
    {
        if (ImGui.BeginPopupModal("Load Editor", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text($"Editing load for {SelectedNodeCount} node(s)");
            ImGui.InputFloat("Load in X", ref _loadX);
            ImGui.InputFloat("Load in Y", ref _loadY);
            ImGui.InputFloat("Moment", ref _moment);
            if (ImGui.Button("Reset values to default"))
            {
                _loadX = 0;
                _loadY = 0;
                _moment = 0;
            }
            ImGui.Separator();
            if (ImGui.Button("Add Load(s)"))
            {
                AddLoadToSelectedNodes(new Load(_loadX, _loadY, _moment));
                _loadX = 0;
                _loadY = 0;
                _moment = 0;
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

    private void DefineBoundaryConditionEditorModal()
    {
        
        if (ImGui.BeginPopupModal("Boundary Condition Editor", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text($"Editing boundary condition for {SelectedNodeCount} node(s)");
            ImGui.Checkbox("Fixed X", ref _fixedX);
            ImGui.Checkbox("Fixed Y", ref _fixedY);
            ImGui.Checkbox("Fixed Moment", ref _fixedMoment);

            if (ImGui.Button("Reset to default"))
            {
                _fixedX = false;
                _fixedY = false;
                _fixedMoment = false;
            }
            ImGui.Separator();
            if (ImGui.Button("Add BC(s)"))
            {
                AddBoundaryConditionToSelectedNodes(new BoundaryCondition(_fixedX, _fixedY, _fixedMoment));
                _fixedX = false;
                _fixedY = false;
                _fixedMoment = false;
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
    private string _addMaterialModalDescription = string.Empty;
    private float _addMaterialModalE;
    private float _addMaterialModalYield;

    private void DefineAddMaterialModal()
    {
        if (ImGui.BeginPopupModal("Add Material", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.InputText("Description", ref _addMaterialModalDescription, DescriptionMaxLength);
            ImGui.InputFloat("E", ref _addMaterialModalE);
            ImGui.InputFloat("Yield", ref _addMaterialModalYield);
            ImGui.Separator();
            if (ImGui.Button("Add material"))
            {
                Structure.AddMaterial(new Material(_addMaterialModalDescription, _addMaterialModalE, _addMaterialModalYield));
                _addMaterialModalDescription = String.Empty;
                _addMaterialModalE = 0;
                _addMaterialModalYield = 0;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Close"))
            {
                _addMaterialModalDescription = String.Empty;
                _addMaterialModalE = 0;
                _addMaterialModalYield = 0;
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
    }

    private string _addSectionModalDescription = "";
    private float _addSectionModalI;
    private float _addSectionModalA;
    
    private void DefineAddSectionModal()
    {
        if (ImGui.BeginPopupModal("Add Section", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.InputText("Description", ref _addSectionModalDescription, DescriptionMaxLength);
            ImGui.InputFloat("I", ref _addSectionModalI);
            ImGui.InputFloat("A", ref _addSectionModalA);
            ImGui.Separator();
            if (ImGui.Button("Add section"))
            {
                Structure.AddSection(new Section(_addSectionModalDescription, _addSectionModalI, _addSectionModalA));
                _addSectionModalDescription = string.Empty;
                _addSectionModalI = 0;
                _addSectionModalA = 0;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Close"))
            {
                _addSectionModalDescription = string.Empty;
                _addSectionModalI = 0;
                _addSectionModalA = 0;
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
    }
    
    
    //scene editor helpers
    private void ResetHovered()
    {
        _hoveredNode = -1;
        _hoveredElement = -1;
    }
    public bool UpdateHoveredItems()
    {
        ResetHovered();
        float treshold = (_settings.HoveringDistanceTreshold / SceneRenderer.ScenePixelGridSpacing) * Structure.GetStructureSettings().GridSpacing;
        _hoveredNode = CheckForNodesCloseToPos(LivePos, treshold);
        if (_hoveredNode == -1)
        {
            _hoveredElement = CheckForElementsCloseToPos(LivePos, treshold);
            if (_hoveredElement == -1)
            {
                return false;
            }
        }

        return true;
    }
    public void SetLivePos(Vector2 position)
    {
        LivePos = GetRealCoordinates(position);
    }
    private void AddHoveredToSelected()
    {
        if (_hoveredNode != -1)
        {
            if (SelectedNodes.Contains(_hoveredNode))
            {
                DeselectNode(_hoveredNode);
            }
            else
            {
                SelectNode(_hoveredNode);
            }
        }

        if (_hoveredElement != -1)
        {
            if (SelectedElements.Contains(_hoveredElement))
            {
                DeselectElement(_hoveredElement);
            }
            else
            {
                SelectElement(_hoveredElement);
            }
        }
    }
    public void SwitchTool(Tool newTool)
    {
        switch (newTool)
        {
            //TODO clean up
            case Tool.AddNode:
                ResetSelection();
                break;
            case Tool.AddElement:
                ResetSelection();
                break;
        }

        CurrentTool = newTool;
    }
    private void AddElementBetweenPositions()
    {
        Vector2 position1 = MultiSelectLockedPos.RoundToNearest(Structure.GetStructureSettings().GridSpacing);
        Vector2 position2 = LivePos.RoundToNearest(Structure.GetStructureSettings().GridSpacing);
        
        //adds node, otherwise returns node index of node at position
        Structure.AddNode(position1, out int node1ID);
        Structure.AddNode(position2, out int node2ID);

        if (node1ID == -1 || node2ID == -1)
        {
            throw new Exception("Something went seriously wrong, did you forget to implement AddNode properly?");
        }
        Structure.AddElement(new Element(node1ID, node2ID, _currentMaterialID, _currentSectionID));

    }
    
    //input handling
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
                Vector2 pos = LivePos.RoundToNearest(Structure.GetStructureSettings().GridSpacing);
                Structure.AddNode(pos);
                break;
            case Tool.MouseSelect:
                AddHoveredToSelected();
                break;
        }
    }
    public void HandleMouseKeyUpEvent()
    {
        switch (CurrentTool)
        {
            case Tool.SelectElements:
            case Tool.SelectNodes:
                FinaliseMultiInput();
                break;
            case Tool.AddElement:
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
            ResetSelection(); 
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
            case Tool.AddNode:
                renderQueue.Enqueue(new SphereObject(LivePos.RoundToNearest(Structure.GetStructureSettings().GridSpacing), drawSettings.SelectedNodeColor, drawSettings.NodeRadius));
                break;
            case Tool.AddElement:
                if (!MultiInputStarted) break;
                float gridSpacing = Structure.GetStructureSettings().GridSpacing;
                renderQueue.Enqueue(new LineObject(
                    GetSceneCoordinates(MultiSelectLockedPos.RoundToNearest(gridSpacing)), 
                    GetSceneCoordinates(LivePos.RoundToNearest(gridSpacing)), 
                    drawSettings.SelectedElementColor,
                    drawSettings.ElementThickness
                    ));
                break;
            case Tool.SelectElements:
            case Tool.SelectNodes:
                if (MultiInputStarted)
                {
                    renderQueue.Enqueue(new SelectionBoxObject(GetSceneCoordinates(MultiSelectLockedPos), GetSceneCoordinates(LivePos), drawSettings.SelectionBoxColor));
                }

                break;
        }

        return renderQueue;
    }
    private void QueueElementSceneObjects(ref Queue<ISceneObject> renderQueue, DrawSettings drawSettings)
    {
        LinesObject selectedElementsObject = new LinesObject(drawSettings.SelectedElementColor, drawSettings.ElementThickness);
        LinesObject elementsObject = new LinesObject(drawSettings.ElementColor, drawSettings.ElementThickness);
        foreach (int i in Structure.GetElementIndexesSorted())
        {
            if (i == _hoveredElement) continue;
            
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

        if (_hoveredElement != -1 && Structure.ValidElementID(_hoveredElement))
        {
            Element hoveredElement = Structure.GetElement(_hoveredElement);
            Vector2 pos1 = GetSceneCoordinates(Structure.GetNode(hoveredElement.Node1ID).Pos);
            Vector2 pos2 = GetSceneCoordinates(Structure.GetNode(hoveredElement.Node2ID).Pos);
            renderQueue.Enqueue(new LineObject(pos1, pos2, drawSettings.HoveredElementColor,
                drawSettings.ElementThickness));
        }

        renderQueue.Enqueue(selectedElementsObject);
        renderQueue.Enqueue(elementsObject);
    }
    private void QueueNodeSceneObjects(ref Queue<ISceneObject> renderQueue, DrawSettings drawSettings)
    {
        SpheresObject selectedNodesObject = new SpheresObject(drawSettings.SelectedNodeColor, drawSettings.NodeRadius);
        SpheresObject nodesObject= new SpheresObject(drawSettings.NodeColor, drawSettings.NodeRadius);

        foreach (int i in Structure.GetNodeIndexesSorted())
        {
            if (i == _hoveredNode) continue;
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

        if (_hoveredNode != -1 && Structure.ValidNodeID(_hoveredNode))
        {
            Vector2 pos = GetSceneCoordinates(Structure.GetNode(_hoveredNode).Pos);
            renderQueue.Enqueue(new SphereObject(pos, drawSettings.HoveredNodeColor, drawSettings.NodeRadius));
        }
        renderQueue.Enqueue(selectedNodesObject);
        renderQueue.Enqueue(nodesObject);
    }
}