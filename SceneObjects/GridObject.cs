using Raylib_cs;
using SimpleFEM.Interfaces;

namespace SimpleFEM.SceneObjects;

public class GridObject : ISceneObject
{
    private readonly int _gridSlices;
    private readonly float _gridSpacing;
    public GridObject(int gridSlices, float gridSpacing)
    {
        _gridSlices = gridSlices;
        _gridSpacing = gridSpacing;
    }

    public void Render()
    {
        Rlgl.PushMatrix();
        Rlgl.Rotatef(90, 1, 0, 0);
        Raylib.DrawGrid(_gridSlices,_gridSpacing);
        Rlgl.Rotatef(-180,1,0,0);            
        Raylib.DrawGrid(_gridSlices,_gridSpacing);
        Rlgl.PopMatrix();
    }
}