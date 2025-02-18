using Raylib_cs;

namespace SimpleFEM.SceneObjects;

public class BackgroundObject : ISceneObject
{
    private Color backgroundColor;
    public BackgroundObject(Color backgroundColor)
    {
        this.backgroundColor = backgroundColor;
    }

    public void Render()
    {
        Raylib.ClearBackground(backgroundColor);
    }
}