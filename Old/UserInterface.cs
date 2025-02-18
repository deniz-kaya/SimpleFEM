using System.Diagnostics;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using ImGuiNET;
using rlImGui_cs;
using Raylib_cs;
using SimpleFEM;

namespace SimpleFEM;

public class UserInterface
{
    public SceneRenderer SceneRenderer;
    private Structure structure;
    
    private float footerHeight = 20f;
    private float mainMenuBarHeight;
    
    public UserInterface(Structure structure)
    {
        this.structure = structure;
        SceneRenderer = new SceneRenderer(structure);
    }
    public void DrawMainDockSpace()
    {
        (Vector2 pos, Vector2 size) usableArea = GetUsableArea();
        ImGui.SetNextWindowPos(usableArea.pos);
        ImGui.SetNextWindowSize(usableArea.size);
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar 
                                 | ImGuiWindowFlags.NoCollapse 
                                 | ImGuiWindowFlags.NoResize 
                                 | ImGuiWindowFlags.NoMove
                                 | ImGuiWindowFlags.NoScrollbar
                                 | ImGuiWindowFlags.NoNav
                                 | ImGuiWindowFlags.NoBackground
                                 | ImGuiWindowFlags.NoInputs
                                 | ImGuiWindowFlags.NoMouseInputs
                                 | ImGuiWindowFlags.NoScrollWithMouse
                                 | ImGuiWindowFlags.NoDecoration
                                 | ImGuiWindowFlags.NoBackground 
                                 | ImGuiWindowFlags.NoDocking;
        
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        
        ImGui.Begin("Dockspace", flags);
        ImGui.DockSpace(ImGui.GetID("MainDockSpace"), Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);
        ImGui.End();
        ImGui.PopStyleVar(5);
    }
    public void ShowMainMenuBar()
    {
        ImGui.BeginMainMenuBar();
        {
            mainMenuBarHeight = ImGui.GetWindowHeight();
            if (ImGui.BeginMenu("File"))
            {
                ImGui.MenuItem("File");
                ImGui.SeparatorText("Test");
                ImGui.Text(DateTime.Now.ToString());
            }
        }
    }
    
    public (Vector2 pos, Vector2 size) GetUsableArea()
    {
        Vector2 viewportSize = ImGui.GetMainViewport().Size;
        return (new Vector2(0,mainMenuBarHeight), 
            Vector2.Subtract(viewportSize, new Vector2(0,mainMenuBarHeight + footerHeight)));
    }
    public void ShowFooter()
    {
        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoTitleBar 
            | ImGuiWindowFlags.NoCollapse
            | ImGuiWindowFlags.NoDocking
            | ImGuiWindowFlags.NoScrollbar
            | ImGuiWindowFlags.NoNav;
        
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(5,3));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        
        Vector2 size = ImGui.GetMainViewport().Size;
        ImGui.SetNextWindowSize(new Vector2(size.X, footerHeight));
        ImGui.SetNextWindowPos(new Vector2(0, size.Y-footerHeight));
        
        ImGui.Begin("Footer", windowFlags);

        float width = ImGui.GetContentRegionAvail().X;
        
        //Left of the footer
        ImGui.Text($"Selected Tool: {SceneRenderer.SelectedTool.ToString()}");
        //Right of the footer
        string mousePosition = SceneRenderer.nullablePosition.ToString();
        
        ImGui.SameLine(width - ImGui.CalcTextSize(mousePosition).X);
        ImGui.Text(mousePosition);
        
        ImGui.End();
        
        ImGui.PopStyleVar(10);
    }

    private Tool selectedTool = Tool.None;
    public void ShowToolBox()
    {
        ImGui.Begin("Toolbox");
        if (ImGui.BeginCombo("Tool", Tool.None.ToString()))
        {

            foreach (Tool t in Enum.GetValues(typeof(Tool)))
            {
                if (ImGui.Selectable(t.ToString(), selectedTool == t))
                {
                    selectedTool = t;
                }

                if (selectedTool == t)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }
        ImGui.End();
        if (selectedTool != SceneRenderer.SelectedTool)
        {
            SceneRenderer.SelectedTool = selectedTool;
        }
    }
        
    public void ShowSimpleEditGUI()
    {
        ImGui.Begin("Simple Edit GUI");
        if (ImGui.BeginTabBar("Property Select Tab Bar"))
        {
            if (ImGui.BeginTabItem("Nodes"))
            {
                ShowNodesTab();
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Elements"))
            {
                ShowElementsTab();
                ImGui.EndTabItem();
            }
        
            if (ImGui.BeginTabItem("Boundary Conditions"))
            {
                ShowBoundaryConditionsTab();
                ImGui.EndTabItem();
            }
        
            if (ImGui.BeginTabItem("Loads"))
            {
                ShowLoadsTab();
                ImGui.EndTabItem();
            }
        
            if (ImGui.BeginTabItem("All"))
            {
                
                // nodes - elements
                // bc's  - loads
                if (ImGui.BeginTable("AllQuadView", 2, ImGuiTableFlags.Borders))
                {

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ShowNodesTab();
                    ImGui.TableNextColumn();
                    ShowElementsTab();
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ShowBoundaryConditionsTab();
                    ImGui.TableNextColumn();
                    ShowLoadsTab();
                    ImGui.EndTable();
                }

                ImGui.EndTabItem();
            }
        }
        ImGui.End();
    }
    
    private int loadNodeID = 0;
    private float loadX = 0f, loadY = 0f, loadMoment = 0f;
    public void ShowLoadsTab()
    {
        if (ImGui.Button("Add Load"))
        {
            ImGui.OpenPopup("AddLoad");
        }

        if (ImGui.BeginPopupModal("AddLoad", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.InputInt("Node ID", ref loadNodeID);
            ImGui.InputFloat("Force X", ref loadX);
            ImGui.InputFloat("Force Y", ref loadY);
            ImGui.InputFloat("Moment", ref loadMoment);

            if (ImGui.Button("Add Load"))
            {
                this.structure.ModifyLoad(loadNodeID, loadX, loadY, loadMoment);
            }

            ImGui.SameLine();
            if (ImGui.Button("Close"))
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        if (ImGui.BeginTable("LoadsTable", 6, ImGuiTableFlags.Borders))
        {
            ImGui.TableSetupColumn("ID");
            ImGui.TableSetupColumn("Node ID");
            ImGui.TableSetupColumn("Force X");
            ImGui.TableSetupColumn("Force Y");
            ImGui.TableSetupColumn("Moment");
            ImGui.TableSetupColumn("");

            ImGui.TableHeadersRow();

            
            foreach (int i in structure.Loads.GetIndexes())
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(i.ToString());
                ImGui.TableNextColumn();
                ImGui.Text(structure.Loads[i].NodeID.ToString());
                ImGui.TableNextColumn();
                ImGui.Text(structure.Loads[i].ForceX.ToString());
                ImGui.TableNextColumn();
                ImGui.Text(structure.Loads[i].ForceY.ToString());
                ImGui.TableNextColumn();
                ImGui.Text(structure.Loads[i].Moment.ToString());
                ImGui.TableNextColumn();

                if (ImGui.Button("Remove"))
                {
                    structure.Loads.RemoveAt(i);
                }

            }
            ImGui.EndTable();

        }
    }
    
    private int node1ID = 0, node2ID = 0;
    private Material material = new Material();
    public void ShowElementsTab()
    {   
        if (ImGui.Button("Add Element"))
        {
            ImGui.OpenPopup("AddElement");
        }

        if (ImGui.BeginPopupModal("AddElement", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.InputInt("Node 1", ref node1ID);
            ImGui.InputInt("Node 2", ref node2ID);
            if (ImGui.Button("Add Element"))
            {
                this.structure.AddElement(node1ID, node2ID, Structure.TestMaterial);
            }

            ImGui.SameLine();
            if (ImGui.Button("Close"))
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        if (ImGui.BeginTable("ElementsTable", 4, ImGuiTableFlags.Borders))
        {
            ImGui.TableSetupColumn("ID");
            ImGui.TableSetupColumn("Node 1 ID");
            ImGui.TableSetupColumn("Node 2 ID");
            ImGui.TableSetupColumn("");

            ImGui.TableHeadersRow();

            foreach (int i in structure.Elements.GetIndexes())
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(i.ToString());
                ImGui.TableNextColumn();
                ImGui.Text(structure.Elements[i].Node1ID.ToString());
                ImGui.TableNextColumn();
                ImGui.Text(structure.Elements[i].Node2ID.ToString());
                ImGui.TableNextColumn();
                // TODO IMPLEMENT VIEWING MATERIAL
                if (ImGui.Button("Remove"))
                {
                    structure.Elements.RemoveAt(i);
                }

            }
            ImGui.EndTable();

        }
    }
    
    private int boundaryConditionNodeID = 0;
    private bool fixedY = false, fixedX = false, fixedMoment = false;
    public void ShowBoundaryConditionsTab()
    {
        if (ImGui.Button("Modify Boundary Condition"))
        {
            ImGui.OpenPopup("ModifyBoundaryCondition");
        }

        if (ImGui.BeginPopupModal("ModifyBoundaryCondition", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.InputInt("Node ID", ref boundaryConditionNodeID);
            ImGui.Checkbox("Fixed X", ref fixedX);
            ImGui.Checkbox("Fixed Y", ref fixedY);
            ImGui.Checkbox("Fixed Moment", ref fixedMoment);
            if (ImGui.Button("Add Condition"))
            {
                this.structure.ModifyBoundaryCondition(boundaryConditionNodeID, fixedX, fixedY, fixedMoment);
            }
            ImGui.SameLine();
            if (ImGui.Button("Close"))
            {
                ImGui.CloseCurrentPopup();
            }
            
            ImGui.EndPopup();
        }

        if (ImGui.BeginTable("BoundaryConditionsTable", 6, ImGuiTableFlags.Borders))
        {
            ImGui.TableSetupColumn("ID");
            ImGui.TableSetupColumn("Node ID");
            ImGui.TableSetupColumn("Fixed X");
            ImGui.TableSetupColumn("Fixed Y");
            ImGui.TableSetupColumn("Fixed Moment");
            ImGui.TableSetupColumn("");

            ImGui.TableHeadersRow();

            
            foreach (int i in structure.BoundaryConditions.GetIndexes())
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(i.ToString());
                ImGui.TableNextColumn();
                ImGui.Text(structure.BoundaryConditions[i].NodeID.ToString());
                ImGui.TableNextColumn();
                ImGui.Text(structure.BoundaryConditions[i].FixedX.ToString());
                ImGui.TableNextColumn();
                ImGui.Text(structure.BoundaryConditions[i].FixedY.ToString());
                ImGui.TableNextColumn();
                ImGui.Text(structure.BoundaryConditions[i].FixedMoment.ToString());
                ImGui.TableNextColumn();
                if (ImGui.Button("Remove"))
                {
                    structure.Nodes.RemoveAt(i);
                }
                
            }
            ImGui.EndTable();

        }
    }
    
    private float nodeX = 0f, nodeY = 0f;
    public void ShowNodesTab()
    {
        if (ImGui.Button("Add Node"))
        {
            ImGui.OpenPopup("AddNode");
        }

        if (ImGui.BeginPopupModal("AddNode", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.InputFloat("X", ref nodeX);
            ImGui.InputFloat("Y", ref nodeY);
            if (ImGui.Button("Add Node"))
            {
                this.structure.AddNode(new Node(new Vector2(nodeX, nodeY)));
            }
            ImGui.SameLine();
            if (ImGui.Button("Close"))
            {
                ImGui.CloseCurrentPopup();
            }
            
            ImGui.EndPopup();
        }

        if (ImGui.BeginTable("NodesTable", 4, ImGuiTableFlags.Borders))
        {
            ImGui.TableSetupColumn("ID");
            ImGui.TableSetupColumn("X");
            ImGui.TableSetupColumn("Y");
            ImGui.TableSetupColumn("");

            ImGui.TableHeadersRow();

            
            foreach (int i in structure.Nodes.GetIndexes())
            {
                Vector2 nodePos = structure.Nodes[i].Pos;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(i.ToString());
                ImGui.TableNextColumn();
                ImGui.Text(nodePos.X.ToString());
                ImGui.TableNextColumn();
                ImGui.Text(nodePos.Y.ToString());
                ImGui.TableNextColumn();
                if (ImGui.Button("Remove"))
                {
                    structure.RemoveNode(i);
                }
                
            }
            ImGui.EndTable();
        }
    }
}
