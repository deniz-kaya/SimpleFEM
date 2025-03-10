using System.ComponentModel.Design;
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
    private Queue<ISceneObject> SceneObjects;
    protected Vector2 currentTextureSize;
    protected Camera2D camera;
    private RenderTexture2D texture;
    protected SceneRendererSettings settings;
    
    readonly static Vector2 DefaultTextureSize = new Vector2(100f, 100f);
    public SceneRenderer(SceneRendererSettings settings)
    {
        this.settings = settings;
        currentTextureSize = DefaultTextureSize;
        texture = RaylibExtensions.LoadRenderTextureV(currentTextureSize);
        camera = new Camera2D(currentTextureSize / 2, Vector2.Zero, 0f, 1f);
        SceneObjects = new Queue<ISceneObject>();
    }

    public void OperateCamera(CameraOperation operation)
    {
        switch (operation)
        {
            case CameraOperation.Left:
                camera.Target -= new Vector2(settings.CameraSpeed / camera.Zoom, 0);
                break;
            case CameraOperation.Right:
                camera.Target += new Vector2(settings.CameraSpeed / camera.Zoom, 0);
                break;
            case CameraOperation.Up:
                camera.Target -= new Vector2(0, settings.CameraSpeed / camera.Zoom);
                break;
            case CameraOperation.Down:
                camera.Target += new Vector2(0, settings.CameraSpeed / camera.Zoom);
                break;
            case CameraOperation.ZoomIn:
                camera.Zoom += camera.Zoom + settings.ZoomIncrement > settings.MaxZoom ? 0f : settings.ZoomIncrement;
                break;
            case CameraOperation.ZoomOut:
                camera.Zoom -= camera.Zoom - settings.ZoomIncrement < settings.MinZoom ? 0f : settings.ZoomIncrement;
                break;
        }
    }
    public void PushObject(ISceneObject sceneObject)
    {
        SceneObjects.Enqueue(sceneObject);
    }

    public void ClearQueue()
    {
        SceneObjects.Clear();
    }
    protected void ProcessTextureSizeChanges(Vector2 newSize)
    {
        if (currentTextureSize != newSize)
        {
            Raylib.UnloadRenderTexture(texture);
            texture = RaylibExtensions.LoadRenderTextureV(newSize);
            camera.Offset = Vector2.Divide(newSize, 2);
            currentTextureSize = newSize;
        }
    }

    public void MoveTarget(Vector2 delta)
    {
        camera.Target += delta;
    }
    public void SetRenderQueue(Queue<ISceneObject> queue)
    {
        this.SceneObjects = queue;
    }
    public RenderTexture2D GetSceneTexture(Vector2 textureSize)
    {
        if (SceneObjects.Count == 0) throw new InvalidOperationException("Render queue is empty");
        ProcessTextureSizeChanges(textureSize);
        
        Raylib.BeginTextureMode(texture);
        Raylib.BeginMode2D(camera);
        Rlgl.PushMatrix();
        //scale and backface culling thing is required to convert coordinate system into traditional cartesian coordinates
        //not explicity necessary for program function, but it is good to have it this way as people are more used to it
        Rlgl.Scalef(1f,-1f,1f);
        Rlgl.DisableBackfaceCulling();
        while (SceneObjects.Count > 0)
        {
            SceneObjects.Dequeue().Render();
        }
        Rlgl.PopMatrix();
        Raylib.EndMode2D();
        Raylib.EndTextureMode();
        
        //clear queue just in case
        ClearQueue();
        return texture;
    }
}