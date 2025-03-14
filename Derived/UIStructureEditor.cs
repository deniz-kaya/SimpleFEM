using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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


public class UIStructureEditor : StructureEditor
{
    public Tool CurrentTool { get; private set; }
    

    private int _currentMaterialID;
    private int _currentSectionID;
    
    private StructureEditorSettings _settings;
    
    private int _hoveredNode;
    private int _hoveredElement;
    public UIStructureEditor(IStructure structure, StructureEditorSettings settings) : base(structure)
    {   
        //reset hovered and selection, initialise fields to their default
        ResetHovered();
        ResetSelection();

        _currentMaterialID = structure.GetMaterialIndexesSorted().First();
        _currentSectionID = structure.GetSectionIndexesSorted().First();
        _settings = settings;
        CurrentTool = Tool.MouseSelect;
    }

    //imgui windows
    public Vector2 GetSceneCoordinates(Vector2 structurePosition)
    {
        //convert structure coordinates to scene coordinates
        return (structurePosition * UISceneRenderer.ScenePixelGridSpacing) / Structure.GetStructureSettings().GridSpacing;
    }
    public Vector2 GetStructureCoordinates(Vector2 screenPosition)
    {
        //convert scene coordinates to structure coordinates
        return (screenPosition / UISceneRenderer.ScenePixelGridSpacing) * Structure.GetStructureSettings().GridSpacing;
    }

    public void DrawOperationWindow()
    {
        MaterialSelectComboBox();
        SectionSelectComboBox();
    }
    public void DrawHoveredPropertiesViewer()
    {
        //exit early if there are no hovered items
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
            //display position of node in the structure coordinate system
            ImGui.Text($"Pos: {pos.ToString()}");
            ImGui.NewLine();
            ImGui.SeparatorText("Boundary Condition");
            //show the boundary conditions on the node in a table if the load isnt default (no bc's), otherwise show that there are no bc's

            if (!bc.IsEmpty)
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
            //show the loads on the node in a table if the load isnt default (no load), otherwise show that there are no loads
            if (!l.IsEmpty)
            {
                if (ImGui.BeginTable("Load", 2, ImGuiTableFlags.Borders))
                {
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
        //this means that there is an element being hovered, else if because if a node is hoevered, we do not want to display hovered element properties
        else if (_hoveredElement != -1)
        {
            Element e = Structure.GetElement(_hoveredElement);
            Material mat = Structure.GetMaterial(e.MaterialID);
            Section sect = Structure.GetSection(e.SectionID);
            
            ImGui.SeparatorText("Material");
            ImGui.Text(mat.Description);
            //table shows material properties
            if (ImGui.BeginTable("Material Properties", 2, ImGuiTableFlags.Borders))
            {
                ImGui.TableNextColumn();
                ImGui.Text("E");
                ImGui.TableNextColumn();
                ImGui.Text(mat.E.ToString());
                
                ImGui.EndTable();

            }
            ImGui.NewLine();
            
            ImGui.SeparatorText("Section");
            ImGui.Text(sect.Description);
            //table shows section properties
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
            //iterates through all materials and displays their description in a combo box list
            foreach (int i in Structure.GetMaterialIndexesSorted())
            {
                if (ImGui.Selectable(Structure.GetMaterial(i).Description, _currentMaterialID == i))
                {
                    //if a material is selected, we want to change the current section ID to the selected one
                    _currentMaterialID = i;
                }

                if (_currentMaterialID == i)
                {
                    //set the display value of the combo box to the selected material
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
            //iterates through all sections and displays their description in a combo box list
            foreach (int i in Structure.GetSectionIndexesSorted())
            {
                if (ImGui.Selectable(Structure.GetSection(i).Description, _currentSectionID == i))
                {
                    //if a section is selected, we want to change the current section ID to the selected one
                    _currentSectionID = i;
                }

                if (_currentSectionID == i)
                {
                    //set the display value of the combo box to the selected section
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }
    }
    
    //Popups
    
    //Load Editor
    private float _loadEditorModalLoadX, _loadEditorModalLoadY, _loadEditorModalMoment;
    //Boundary Condition Editor
    private bool _bcEditorModalFixedX, _bcEditorModalFixedY, _bcEditorModalFixedRotation;
    //Add section
    private string _addMaterialModalDescription = string.Empty;
    private float _addMaterialModalE;
    private string _addMaterialModalErrorMessage = string.Empty;
    //Add Material
    private string _addSectionModalDescription = string.Empty;
    private float _addSectionModalI;
    private float _addSectionModalA;
    private string _addSectionModalErrorMessage = string.Empty;
    //Flags
    private bool _openBcEditor, _openLoadEditor;
    public bool OpenAddMaterialModal, OpenAddSectionModal;
    public void HandlePopups()
    {
        //all popups must be defined each frame, as otherwise they will not be displayed when set to "open" by imgui
        DefineAddSectionModal();
        DefineAddMaterialModal();
        DefineLoadEditorModal();
        DefineBoundaryConditionEditorModal();
        DefineSelectedNodePopup();
        
        //change popup states using the flags set within other parts of the program
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
                ResetHovered();
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
        //added to avoid having to rewrite code
        void ResetValues()
        {
            _loadEditorModalLoadX = 0;
            _loadEditorModalLoadY = 0;
            _loadEditorModalMoment = 0;
        }
        if (ImGui.BeginPopupModal("Load Editor", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text($"Editing load for {SelectedNodeCount} node(s)");
            ImGui.InputFloat("Load in X", ref _loadEditorModalLoadX);
            ImGui.InputFloat("Load in Y", ref _loadEditorModalLoadY);
            ImGui.InputFloat("Moment", ref _loadEditorModalMoment);
            if (ImGui.Button("Reset values to default"))
            {
                ResetValues();
            }
            ImGui.Separator();
            if (ImGui.Button("Add Load(s)"))
            {
                //the - for the Y axis load is because we flipped the raylib scene earlier, this means that negative loads would push the structure 'up' rather than 'down' without this change
                AddLoadToSelectedNodes(new Load(_loadEditorModalLoadX, -_loadEditorModalLoadY, _loadEditorModalMoment));
                ResetValues();
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
        void ResetValues()
        {
            _bcEditorModalFixedX = false;
            _bcEditorModalFixedY = false;
            _bcEditorModalFixedRotation = false;
        }
        if (ImGui.BeginPopupModal("Boundary Condition Editor", ImGuiWindowFlags.AlwaysAutoResize))
        {
            //imgui input fields
            ImGui.Text($"Editing boundary condition for {SelectedNodeCount} node(s)");
            ImGui.Checkbox("Fixed X", ref _bcEditorModalFixedX);
            ImGui.Checkbox("Fixed Y", ref _bcEditorModalFixedY);
            ImGui.Checkbox("Fixed Rotation", ref _bcEditorModalFixedRotation);
                
            //buttons actions are self explanatory
            if (ImGui.Button("Reset values to default"))
            {
                ResetValues();
            }
            ImGui.Separator();
            if (ImGui.Button("Add BC(s)"))
            {
                AddBoundaryConditionToSelectedNodes(new BoundaryCondition(_bcEditorModalFixedX, _bcEditorModalFixedY, _bcEditorModalFixedRotation));
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                ResetValues();
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
    }

    //maximum length for the material and section descriptions
    private const int DescriptionMaxLength = 160;
    
    private void DefineAddMaterialModal()
    {
        //written to avoid repeating code
        void CloseAction()
        {
            _addMaterialModalDescription = string.Empty;
            _addMaterialModalE = 0;
            _addMaterialModalErrorMessage = string.Empty;
            ImGui.CloseCurrentPopup();
        }
        if (ImGui.BeginPopupModal("Add Material", ImGuiWindowFlags.AlwaysAutoResize))
        {
            //show fields for material that can be edited
            ImGui.InputText("Description", ref _addMaterialModalDescription, DescriptionMaxLength);
            ImGui.InputFloat("E", ref _addMaterialModalE);
            ImGui.Separator();

            //buttons
            if (ImGui.Button("Add material"))
            {
                if (_addMaterialModalE > 0 && _addMaterialModalDescription.Length > 0)
                {
                    Structure.AddMaterial(new Material(_addMaterialModalDescription, _addMaterialModalE));
                    CloseAction();
                }
                else
                {
                    _addMaterialModalErrorMessage = "Material values cannot be blank/zero!";
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Close"))
            {
                CloseAction();
            }

            ImGui.TextWrapped(_addMaterialModalErrorMessage);
            ImGui.EndPopup();
        }
    }



    private void DefineAddSectionModal()
    {
        //written to avoid rewriting code
        void CloseAction()
        {
            _addSectionModalDescription = string.Empty;
            _addSectionModalI = 0;
            _addSectionModalA = 0;
            _addSectionModalErrorMessage = string.Empty;
            ImGui.CloseCurrentPopup();
        }
        if (ImGui.BeginPopupModal("Add Section", ImGuiWindowFlags.AlwaysAutoResize))
        {
            //buttons
            ImGui.InputText("Description", ref _addSectionModalDescription, DescriptionMaxLength);
            ImGui.InputFloat("I", ref _addSectionModalI);
            ImGui.InputFloat("A", ref _addSectionModalA);
            ImGui.Separator();
            if (ImGui.Button("Add section"))
            {
                if (_addSectionModalI > 0 && _addSectionModalA > 0)
                {
                    Structure.AddSection(new Section(_addSectionModalDescription, _addSectionModalI,
                        _addSectionModalA));
                    CloseAction();
                }
                else
                {
                    _addSectionModalErrorMessage = "Section values cannot be blank/zero!";
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Close"))
            {
                CloseAction();
            }
            ImGui.TextWrapped(_addSectionModalErrorMessage);
            ImGui.EndPopup();
        }
    }
    
    
    //scene editor helpers
    private void ResetHovered()
    {
        _hoveredNode = -1;
        _hoveredElement = -1;
    }
    //returns true if there are hovered items
    public bool UpdateHoveredItems()
    {
        ResetHovered();
        //calculate the treshold distance
        float treshold = (_settings.HoveringDistanceTreshold / UISceneRenderer.ScenePixelGridSpacing) * Structure.GetStructureSettings().GridSpacing;
        _hoveredNode = CheckForNodesCloseToPos(LivePos, treshold);
        if (_hoveredNode == -1)
        {
            //this means that no nodes are hovered, we should check for elements instead
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
        LivePos = GetStructureCoordinates(position);
    }
    // adds/removes hovered items from selected items
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
        //handling operations which have to be done when switching to different tools

        switch (newTool)
        {
            case Tool.AddNode:
                ResetSelection();
                break;
            case Tool.AddElement:
                ResetSelection();
                break;
        }

        CurrentTool = newTool;
    }
    
    public void DeleteSelected()
    {
        //reset hovered while deleting selected, in case items being deleted are hovered at the moment
        ResetHovered();
        DeleteSelectedElements();
        DeleteSelectedNodes();
    }
    private void AddElementBetweenPositions()
    {
        Vector2 position1 = MultiSelectLockedPos.RoundToNearest(Structure.GetStructureSettings().GridSpacing);
        Vector2 position2 = LivePos.RoundToNearest(Structure.GetStructureSettings().GridSpacing);
        
        //adds node, otherwise returns node index of node already present at position
        Structure.AddNode(position1, out int node1ID);
        Structure.AddNode(position2, out int node2ID);

        //add element between nodes with currently seleted material and section
        Structure.AddElement(new Element(node1ID, node2ID, _currentMaterialID, _currentSectionID));

    }
    
    //input handling
    public void HandleMouseKeyDownEvent()
    {
        //handles actions according to the current tool when mouse key is down
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
        //handles action according to the current tool when the mouse key is pressed
        switch (CurrentTool)
        {
            case Tool.AddNode:
                //add the grid snapped node to the structure
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
        //handles action according to the current tool when mouse key is lifted
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
        //if multi input isnt started, we want to start it
        if (!MultiInputStarted)
        {
            //reset the selection as a new one is starting
            ResetSelection();
            //set the locked pos to the current live position
            MultiSelectLockedPos = LivePos;
            //change multi input status
            MultiInputStarted = true;
        }
    }
    
    //Rendering logic stuff
    public Queue<ISceneObject> GetSceneObjects(DrawSettings drawSettings)
    {
        //initialise render queue
        Queue<ISceneObject> renderQueue = new Queue<ISceneObject>();
        //background
        renderQueue.Enqueue(new BackgroundObject(Color.White));
        
        //grid
        renderQueue.Enqueue(new GridObject(UISceneRenderer.SceneGridSlices, UISceneRenderer.ScenePixelGridSpacing));
        
        //Elements
        QueueElementSceneObjects(ref renderQueue, drawSettings);
        
        //Nodes
        QueueNodeSceneObjects(ref renderQueue, drawSettings);
        
        //Tool based extra rendering
        switch (CurrentTool)
        {
            case Tool.AddNode:
                //queue drawing the circle for the candidate node snapped to the grid
                Vector2 newNodeScenePosition = GetSceneCoordinates(LivePos.RoundToNearest(Structure.GetStructureSettings().GridSpacing));
                renderQueue.Enqueue(new CircleObject(newNodeScenePosition, drawSettings.SelectedNodeColor, drawSettings.NodeRadius));
                break;
            case Tool.AddElement:
                float gridSpacing = Structure.GetStructureSettings().GridSpacing;
                //queue drawing the circle for the first node of the element snapped to the grid
                renderQueue.Enqueue(new CircleObject(GetSceneCoordinates(LivePos.RoundToNearest(gridSpacing)),
                    drawSettings.SelectedNodeColor, drawSettings.NodeRadius));
                if (!MultiInputStarted) break;
                //multi input has started, therefore we should draw the second candidate node and tehe candidate element
                renderQueue.Enqueue(new LineObject(
                    GetSceneCoordinates(MultiSelectLockedPos.RoundToNearest(gridSpacing)), 
                    GetSceneCoordinates(LivePos.RoundToNearest(gridSpacing)), 
                    drawSettings.SelectedElementColor,
                    drawSettings.ElementThickness
                    ));
                //queue drawing the circle for the second node of the element, node snapped to the grid
                renderQueue.Enqueue(new CircleObject(GetSceneCoordinates(MultiSelectLockedPos.RoundToNearest(gridSpacing)), drawSettings.SelectedNodeColor, drawSettings.NodeRadius));

                break;
            case Tool.SelectElements:
            case Tool.SelectNodes:
                //if multi input has started, draw the selection box to show the area being selected
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
        //create different linesobject with respective colours for selected and normal elemeents
        LinesObject selectedElementsObject = new LinesObject(drawSettings.SelectedElementColor, drawSettings.ElementThickness);
        LinesObject elementsObject = new LinesObject(drawSettings.ElementColor, drawSettings.ElementThickness);
        foreach (int i in Structure.GetElementIndexesSorted())
        {
            //do not draw the element if it is hovered, as this is handled later
            if (i == _hoveredElement) continue;
            
            //get element node scene positions
            Element e = Structure.GetElement(i);
            Vector2 position1 = GetSceneCoordinates(Structure.GetNode(e.Node1ID).Pos);
            Vector2 position2 = GetSceneCoordinates(Structure.GetNode(e.Node2ID).Pos);
            
            //if the element is one that is selected, add it to the selected elements object, otherwise add it to the normal one
            if (SelectedElements.Contains(i))
            {
                selectedElementsObject.AddLine(position1, position2);
            }
            else
            {
                elementsObject.AddLine(position1, position2);
            }
        }
        //if there is a hovered element, and the hovered element exists (extra check to ensure progran doesnt crash)
        if (_hoveredElement != -1 && Structure.ValidElementID(_hoveredElement))
        {
            //get the node positions, and draw teh hovered element on the scene
            Element hoveredElement = Structure.GetElement(_hoveredElement);
            Vector2 pos1 = GetSceneCoordinates(Structure.GetNode(hoveredElement.Node1ID).Pos);
            Vector2 pos2 = GetSceneCoordinates(Structure.GetNode(hoveredElement.Node2ID).Pos);
            renderQueue.Enqueue(new LineObject(pos1, pos2, drawSettings.HoveredElementColor,
                drawSettings.ElementThickness));
        }
        //queue the elements
        renderQueue.Enqueue(selectedElementsObject);
        renderQueue.Enqueue(elementsObject);
    }
    private void QueueNodeSceneObjects(ref Queue<ISceneObject> renderQueue, DrawSettings drawSettings)
    {
        //create two different circle objects for selected and normal nodes
        CirclesObject selectedNodesObject = new CirclesObject(drawSettings.SelectedNodeColor, drawSettings.NodeRadius);
        CirclesObject nodesObject= new CirclesObject(drawSettings.NodeColor, drawSettings.NodeRadius);

        foreach (int i in Structure.GetNodeIndexesSorted())
        {
            //if current node is hovered, do not draw as it is handled later
            if (i == _hoveredNode) continue;
            //get position, and add position to respective list depenbding on node selection status
            Vector2 position = GetSceneCoordinates(Structure.GetNode(i).Pos);
            if (SelectedNodes.Contains(i))
            {
                selectedNodesObject.AddCircle(position);
            }
            else
            {
                nodesObject.AddCircle(position);
            }
        }
        //if a node is hovered and it is a valid node, draw it on the scene
        if (_hoveredNode != -1 && Structure.ValidNodeID(_hoveredNode))
        {
            Vector2 pos = GetSceneCoordinates(Structure.GetNode(_hoveredNode).Pos);
            renderQueue.Enqueue(new CircleObject(pos, drawSettings.HoveredNodeColor, drawSettings.NodeRadius));
        }
        //queue the nodes
        renderQueue.Enqueue(selectedNodesObject);
        renderQueue.Enqueue(nodesObject);
    }
}