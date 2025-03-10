using System.Numerics;
using Raylib_cs;
using SimpleFEM.Interfaces;
using SimpleFEM.Extensions;
using SimpleFEM.Types;
using SimpleFEM.Types.Settings;

namespace SimpleFEM.Base;

public class SceneRenderer
{
    public const int ScenePixelGridSpacing = 50;
    public const int SceneGridSlices = 200;
    private Queue<ISceneObject> _sceneObjects;
    protected Vector2 CurrentTextureSize;
    protected Camera2D Camera;
    private RenderTexture2D _texture;
    protected SceneRendererSettings Settings;
    
    static readonly Vector2 DefaultTextureSize = new Vector2(100f, 100f);
    public SceneRenderer(SceneRendererSettings settings)
    {
        Settings = settings;
        CurrentTextureSize = DefaultTextureSize;
        _texture = RaylibExtensions.LoadRenderTextureV(CurrentTextureSize);
        Camera = new Camera2D(CurrentTextureSize / 2, Vector2.Zero, 0f, 1f);
        _sceneObjects = new Queue<ISceneObject>();
    }

    public void OperateCamera(CameraOperation operation)
    {
        switch (operation)
        {
            case CameraOperation.Left:
                Camera.Target -= new Vector2(Settings.CameraSpeed / Camera.Zoom, 0);
                break;
            case CameraOperation.Right:
                Camera.Target += new Vector2(Settings.CameraSpeed / Camera.Zoom, 0);
                break;
            case CameraOperation.Up:
                Camera.Target -= new Vector2(0, Settings.CameraSpeed / Camera.Zoom);
                break;
            case CameraOperation.Down:
                Camera.Target += new Vector2(0, Settings.CameraSpeed / Camera.Zoom);
                break;
            case CameraOperation.ZoomIn:
                Camera.Zoom += Camera.Zoom + Settings.ZoomIncrement > Settings.MaxZoom ? 0f : Settings.ZoomIncrement;
                break;
            case CameraOperation.ZoomOut:
                Camera.Zoom -= Camera.Zoom - Settings.ZoomIncrement < Settings.MinZoom ? 0f : Settings.ZoomIncrement;
                break;
        }
    }
    public void PushObject(ISceneObject sceneObject)
    {
        _sceneObjects.Enqueue(sceneObject);
    }

    public void ClearQueue()
    {
        _sceneObjects.Clear();
    }
    protected void ProcessTextureSizeChanges(Vector2 newSize)
    {
        if (CurrentTextureSize != newSize)
        {
            Raylib.UnloadRenderTexture(_texture);
            _texture = RaylibExtensions.LoadRenderTextureV(newSize);
            Camera.Offset = Vector2.Divide(newSize, 2);
            CurrentTextureSize = newSize;
        }
    }

    public void MoveTarget(Vector2 delta)
    {
        Camera.Target += delta;
    }
    public void SetRenderQueue(Queue<ISceneObject> queue)
    {
        this._sceneObjects = queue;
    }
    public RenderTexture2D GetSceneTexture(Vector2 textureSize)
    {
        if (_sceneObjects.Count == 0) throw new InvalidOperationException("Render queue is empty");
        ProcessTextureSizeChanges(textureSize);
        
        Raylib.BeginTextureMode(_texture);
        Raylib.BeginMode2D(Camera);
        Rlgl.PushMatrix();
        //scale and backface culling thing is required to convert coordinate system into traditional cartesian coordinates
        //not explicity necessary for program function, but it is good to have it this way as people are more used to it
        Rlgl.Scalef(1f,-1f,1f);
        Rlgl.DisableBackfaceCulling();
        while (_sceneObjects.Count > 0)
        {
            _sceneObjects.Dequeue().Render();
        }
        Rlgl.PopMatrix();
        Raylib.EndMode2D();
        Raylib.EndTextureMode();
        
        //clear queue just in case
        ClearQueue();
        return _texture;
    }
}