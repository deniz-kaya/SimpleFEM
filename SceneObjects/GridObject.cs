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
        //we have to use the lower level api rlgl here to rotate the coordinate system as the grid is by default drawn on the x-z plane
        //this is a problem as our 2D camera views the x-y plane
        Rlgl.PushMatrix();
        //we have to rotate by 90 degrees about x to transform x-z plane to x-y
        Rlgl.Rotatef(90, 1, 0, 0);
        //draw top half of grid
        Raylib.DrawGrid(_gridSlices,_gridSpacing);
        
        //undo last rotation and rotate the other direction to draw the other half of the grid
        Rlgl.Rotatef(-180,1,0,0);            
        Raylib.DrawGrid(_gridSlices,_gridSpacing);
        Rlgl.PopMatrix();
    }
}