using Raylib_cs;
using SimpleFEM.Interfaces;

namespace SimpleFEM.SceneObjects;

public class BackgroundObject : ISceneObject
{
    private readonly Color _backgroundColor;
    public BackgroundObject(Color backgroundColor)
    {
        _backgroundColor = backgroundColor;
    }

    public void Render()
    {
        Raylib.ClearBackground(_backgroundColor);
    }
}