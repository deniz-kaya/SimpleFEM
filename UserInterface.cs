using System.Diagnostics;
using ImGuiNET;
using SimpleFEM;

namespace SimpleFEM;

public class UserInterface
{
    private Data.Structure structure;
    public UserInterface(Data.Structure structure)
    {
        this.structure = structure;
    }
    
    //Nodes
    private float nodeX, nodeY;
    
    //Elements
    private int node1ID, node2ID;
    private Data.Material material;
    
    //Boundary Conditions
    private int boundaryConditionNodeID;
    private bool fixedY, fixedX, fixedMoment;
    
    //Loads
    private int loadNodeID;
    private float loadX, loadY, loadMoment;
    
    
    public void ShowSimpleEditGUI()
    {
        Console.WriteLine("reached simpleeditygui");
        ImGui.Begin("Simple Edit GUI");
        Console.WriteLine("ImGui.Begin executed!");
        ImGui.Text("Test text");
        if (ImGui.BeginTabBar("Property Select Tab Bar"))
        {
            if (ImGui.BeginTabItem("Nodes"))
            {
                Console.WriteLine("reached to nodes bit");
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
                    ImGui.TableSetupColumn("");
                    ImGui.TableSetupColumn("");
                    
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
                // ShowNodesTab();
                // ImGui.SameLine();
                // ShowElementsTab();
                //
                // ShowBoundaryConditionsTab();
                // ImGui.SameLine();
                // ShowLoadsTab();
                
                ImGui.EndTabItem();
            }
        }
        ImGui.End();
    }
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
                this.structure.AddElement(node1ID, node2ID, Data.Structure.TestMaterial);
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
                ImGui.Text(structure.Elements[i].Node1Id.ToString());
                ImGui.TableNextColumn();
                ImGui.Text(structure.Elements[i].Node2Id.ToString());
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
                this.structure.AddNode(nodeX, nodeY);
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
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(i.ToString());
                ImGui.TableNextColumn();
                ImGui.Text(structure.Nodes[i].X.ToString());
                ImGui.TableNextColumn();
                ImGui.Text(structure.Nodes[i].Y.ToString());
                ImGui.TableNextColumn();
                if (ImGui.Button("Remove"))
                {
                    structure.Nodes.RemoveAt(i);
                }
                
            }
            ImGui.EndTable();

        }
    }
}