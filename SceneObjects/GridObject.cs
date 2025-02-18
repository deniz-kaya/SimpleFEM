using Raylib_cs;

namespace SimpleFEM.SceneObjects;

public class GridObject : ISceneObject
{
    private int gridSlices;
    private float gridSpacing;
    public GridObject(int gridSlices, float gridSpacing)
    {
        this.gridSlices = gridSlices;
        this.gridSpacing = gridSpacing;
    }

    public void Render()
    {
        Rlgl.PushMatrix();
        Rlgl.Rotatef(90, 1, 0, 0);
        Raylib.DrawGrid(gridSlices,gridSpacing);
        Rlgl.Rotatef(-180,1,0,0);            
        Raylib.DrawGrid(gridSlices,gridSpacing);
        Rlgl.PopMatrix();
    }
}