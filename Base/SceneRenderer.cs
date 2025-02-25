using System.ComponentModel.Design;
using System.Numerics;
using Raylib_cs;
using SimpleFEM.Interfaces;
using SimpleFEM.Extensions;

namespace SimpleFEM.Base;

public class SceneRenderer
{
    private Queue<ISceneObject> SceneObjects;
    protected Vector2 currentTextureSize;
    protected Camera2D camera;
    private RenderTexture2D texture;
    readonly static Vector2 DefaultTextureSize = new Vector2(100f, 100f);
    public SceneRenderer(Vector2 textureSize)
    {
        currentTextureSize = textureSize;
        texture = RaylibExtensions.LoadRenderTextureV(currentTextureSize);
        camera = new Camera2D(currentTextureSize / 2, Vector2.Zero, 0f, 1f);
        SceneObjects = new Queue<ISceneObject>();
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