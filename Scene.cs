using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks.Dataflow;
using Raylib_cs;
using ImGuiNET;
using rlImGui_cs;

namespace SimpleFEM;

public class Scene
{
    public Camera2D camera;
    private Vector2 SceneWindowPosition;
    private Vector2 SceneWindowSize;
    private Structure structure;
    private StructureToolbox toolbox;
    
    //FLAGS
    public bool SceneWindowHovered { get; private set; }

    
    private bool FirstRender;
    public Tool SelectedTool
    {
        get => toolbox.CurrentTool;
        set => toolbox.SoftSwitchState(value);
    }
    //settings
    private int gridSpacing = 50;
    public int cameraSpeed = 20;

    private Vector2 worldPosition
    {
        get => GetScreenToScenePos(ImGui.GetMousePos());
    }
    
    public Vector2? nullablePosition
    {
        get => SceneWindowHovered ? GetScreenToScenePos(ImGui.GetMousePos()) : null;
    }
    
    public float CameraZoom
    {
        get => camera.Zoom;
        set => camera.Zoom = value;
    }
    
    public Scene(Structure structure)
    {
        this.toolbox = new StructureToolbox(structure);
        this.structure = structure;
        FirstRender = true;
    }

    public Vector2 GetScreenToScenePos(Vector2 screenPos)
    {
        return Raylib.GetScreenToWorld2D((screenPos - SceneWindowPosition), camera);
        
    }

    private Vector2 SnapToGrid(Vector2 pos)
    {
        return pos.RoundToNearest(gridSpacing);
    }
    
    public void ShowSceneWindow()
    {
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar;
        
        //optional
        // ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0f,0f));        
        // ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);
        // ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
        ImGui.Begin("Scene Window", flags);
        
        SceneWindowHovered = ImGui.IsWindowHovered();
        
        if (FirstRender)
        {
            SceneWindowPosition = ImGui.GetCursorScreenPos();
            SceneWindowSize = ImGui.GetContentRegionAvail();
            
            camera = new Camera2D(Vector2.Divide(SceneWindowSize,2), Vector2.Zero, 0f, 1f);

            viewTexture = RaylibExtensions.LoadRenderTextureV(SceneWindowSize);

            FirstRender = false;
        }
        else
        {
            SceneWindowPosition = ImGui.GetCursorScreenPos();
            ProcessWindowSizeChanges(ImGui.GetContentRegionAvail());
        }
        RenderSceneToTexture();
        rlImGui.ImageRenderTexture(viewTexture);

        ProcessInputs();
        DefinePopups();
        ImGui.PopStyleVar(10);


        ImGui.End();
    }
    
    private void ShowPopup()
    {
        if (toolbox.selectedNodes.Count == 1)
        {
            ImGui.OpenPopup("Select_Node_Popup");
        }
    }

    //INPUT HANDLING
    private bool DoIdleSelection = true;
    public void ProcessInputs()
    {
        if (!SceneWindowHovered) return;
        if (DoIdleSelection)
        {
            if (!toolbox.SelectNearbyNode(worldPosition))
            {
                
                toolbox.SelectNearbyElement(worldPosition);
            }
        }

        if (ImGui.IsKeyPressed(ImGuiKey.MouseRight))
        {
            ShowPopup();
        }

        SwitchTools();
        switch (toolbox.CurrentTool)
        {
            case Tool.None:
                DoIdleSelection = true;
                HandleCameraMovementInput();
                
                break;
            case Tool.AddNode:
                DoIdleSelection = true;
                if (ImGui.IsKeyPressed(ImGuiKey.MouseLeft))
                {
                    toolbox.SetFirstSelectPos(SnapToGrid(worldPosition));
                    toolbox.AddNode();
                    toolbox.SwitchState(Tool.AddNode);
                }

                break;
            case Tool.AddElement:
                DoIdleSelection = true;
                HandleMultiplePositionInput(true);
                if (ImGui.IsKeyReleased(ImGuiKey.MouseLeft))
                {
                    toolbox.AddElement();
                    toolbox.SwitchState(Tool.AddElement);
                }

                break;
            case Tool.SelectNodes:
                HandleMultiplePositionInput();
                if (toolbox.MultiInputStarted)
                {
                    DoIdleSelection = false;
                    toolbox.SelectNodesWithinArea();
                }
                if (ImGui.IsKeyReleased(ImGuiKey.MouseLeft))
                {
                    if (toolbox.EmptySelection)
                    {
                        DoIdleSelection = true;
                    }
                    toolbox.SoftSwitchState(Tool.SelectNodes);
                }
                break;
            case Tool.SelectElements:
                HandleMultiplePositionInput();
                if (toolbox.MultiInputStarted)
                {
                    DoIdleSelection = false;
                    toolbox.SelectElementsWithinArea();
                }
                if (ImGui.IsKeyReleased(ImGuiKey.MouseLeft))
                {
                    if (toolbox.EmptySelection)
                    {
                        DoIdleSelection = true;
                    }
                    toolbox.SoftSwitchState(Tool.SelectElements);
                }
                
                break;
        }
        Console.WriteLine(toolbox.CurrentTool.ToString());

        if (ImGui.IsKeyPressed(ImGuiKey.Delete))
        {
            if (!toolbox.EmptySelection)
            {
                ImGui.OpenPopup("ConfirmDeleteModal");
            }
        }
    }
    private void HandleCameraMovementInput()
    {
        if (ImGui.IsKeyDown(ImGuiKey.RightArrow))
            camera.Target += new Vector2(cameraSpeed / camera.Zoom, 0);
        if (ImGui.IsKeyDown(ImGuiKey.LeftArrow))
            camera.Target -= new Vector2(cameraSpeed / camera.Zoom, 0);
        if (ImGui.IsKeyDown(ImGuiKey.DownArrow))
            camera.Target += new Vector2(0, cameraSpeed / camera.Zoom);
        if (ImGui.IsKeyDown(ImGuiKey.UpArrow))
            camera.Target -= new Vector2(0, cameraSpeed / camera.Zoom);
        if (ImGui.IsKeyPressed(ImGuiKey.PageUp))
            camera.Zoom += camera.Zoom + 0.25f > 6 ? 0f : 0.25f;
        if (ImGui.IsKeyPressed(ImGuiKey.PageDown))
            camera.Zoom -= camera.Zoom - 0.25f < 0.1f ? 0f : 0.25f;
    }
    public void HandleMultiplePositionInput(bool gridSnap = false)
    {
        Vector2 pos = gridSnap ? SnapToGrid(worldPosition) : worldPosition;
        if (ImGui.IsKeyDown(ImGuiKey.MouseLeft))
        {
            if (!toolbox.MultiInputStarted)
            {
                DoIdleSelection = false;
                toolbox.SetFirstSelectPos(pos);
            }
            toolbox.SetSecondSelectPos(pos);
        }
    }

    public void SwitchTools()
    {
        if (ImGui.IsKeyPressed(ImGuiKey.M))
        {
            Console.WriteLine("key pressed");

            SelectedTool = Tool.None;
        }
        if (ImGui.IsKeyPressed(ImGuiKey.A))
        {
            Console.WriteLine("key pressed");
            SelectedTool = Tool.AddNode;
        }
        if (ImGui.IsKeyPressed(ImGuiKey.E))
        {
            SelectedTool = Tool.AddElement;
            Console.WriteLine(toolbox.CurrentTool.ToString());
        }
        if (ImGui.IsKeyPressed(ImGuiKey.Z))
        {
            SelectedTool = Tool.SelectNodes;            Console.WriteLine(toolbox.CurrentTool.ToString());

        }
        if (ImGui.IsKeyPressed(ImGuiKey.D))            
        {
            SelectedTool = Tool.SelectElements;            Console.WriteLine(toolbox.CurrentTool.ToString());

        }
        
    }   
    
    //DRAWING TO TEXTURE
    private RenderTexture2D viewTexture;
    private void RenderSceneToTexture()
    {
        Raylib.BeginTextureMode(viewTexture);
        Raylib.BeginMode2D(camera);
        
        Vector2 pos = GetScreenToScenePos(ImGui.GetMousePos());

        Raylib.ClearBackground(Color.RayWhite);
            
        DrawGrid(); 
    
        DrawElements();

        DrawNodes();
        
        Raylib.DrawCircleV(pos, 2, Color.Black);

        if (toolbox.CurrentTool == Tool.SelectElements || toolbox.CurrentTool == Tool.SelectNodes)
        {
            Raylib.DrawRectangleRec(
                RectangleExtensions.GetRectangleFromPoints(toolbox.selectPos1, toolbox.selectPos2), 
                new Color(199, 199, 199, 59));
        }
        else if (toolbox.CurrentTool == Tool.AddElement)
        {
            Raylib.DrawLineEx(toolbox.selectPos1, toolbox.selectPos2, 2, Color.Green);
        }
        Raylib.EndMode2D();
        Raylib.EndTextureMode();

    }
    private void DrawElements()
    {
        foreach (int i in structure.Elements.GetIndexes())
        {
            Element e = structure.Elements[i];
            Vector2 pos1 = structure.Nodes[e.Node1Id].pos;
            Vector2 pos2 = structure.Nodes[e.Node2Id].pos;
            Color c = Color.Green;
            if (toolbox.selectedElements.Contains(i))
            {
                c = Color.Orange;
            }
            Raylib.DrawLineEx(pos1, pos2, 2, c);
        }
    }
    private void DrawNodes()
    {
        foreach (int i in structure.Nodes.GetIndexes())
        {
            Color c = Color.Red;
            if (toolbox.selectedNodes.Contains(i))
            {
                c = Color.Orange;
            }
            Raylib.DrawCircleV(structure.Nodes[i].pos, 3, c);
        }
    }
    private void DrawGrid()
    {
        Rlgl.PushMatrix();
        Rlgl.Rotatef(90, 1, 0, 0);
        Raylib.DrawGrid(200,gridSpacing);
        Rlgl.Rotatef(-180,1,0,0);            
        Raylib.DrawGrid(200,gridSpacing);
        Rlgl.PopMatrix();
    }
    private void ProcessWindowSizeChanges(Vector2 newWindowSize)
        {
            if (SceneWindowSize != newWindowSize)
            {
                Raylib.UnloadRenderTexture(viewTexture);
                viewTexture = RaylibExtensions.LoadRenderTextureV(newWindowSize);
                camera.Offset = Vector2.Divide(newWindowSize, 2);
                SceneWindowSize = newWindowSize;
            }
        }
    
    //POPUPS
    private void DefinePopups()
    {
        SelectNodePopupDefinition();
        ConfirmDeleteModalDefinition();
    }

    private void ConfirmDeleteModalDefinition()
    {
        if (ImGui.BeginPopupModal("ConfirmDeleteModal"))
        {
            int elementCount = toolbox.selectedElements.Count;
            int nodeCount = toolbox.selectedNodes.Count;
            
            ImGui.Text("Are you sure you want to delete?");
            ImGui.Text($"{elementCount} element(s)");
            ImGui.Text($"{nodeCount} node(s)");
            if (ImGui.Button("Cancel") || ImGui.IsKeyPressed(ImGuiKey.Escape))
            {
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            
            if (ImGui.Button("Delete") || ImGui.IsKeyPressed(ImGuiKey.Enter))
            {
                toolbox.DeleteSelectedElements();
                toolbox.DeleteSelectedNodes();
                ImGui.CloseCurrentPopup();
            }
            ImGui.SetItemDefaultFocus();
            ImGui.EndPopup();
        }
    }
    private void SelectNodePopupDefinition()
    {
        if (ImGui.BeginPopup("Select_Node_Popup"))
        {
            Console.WriteLine("Showed popup");
            if (ImGui.Selectable("Show Properties"))
            {
                Console.WriteLine("Show Properties Reached");
            }

            ImGui.EndPopup();
        }

    }
}

