using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using SimpleFEM.Interfaces;
using SimpleFEM.Extensions;
using SimpleFEM.Types;
using SimpleFEM.Types.Settings;

namespace SimpleFEM.Base;

public class SceneRenderer
{

    private Queue<ISceneObject> _sceneObjects;
    protected Vector2 CurrentTextureSize;
    protected Camera2D Camera;
    private RenderTexture2D _texture;
    protected SceneRendererSettings Settings;
    
    static readonly Vector2 DefaultTextureSize = new Vector2(100f, 100f);
    public SceneRenderer(SceneRendererSettings settings)
    {
        //initialise all objects to their default
        Settings = settings;
        CurrentTextureSize = DefaultTextureSize;
        _texture = RaylibExtensions.LoadRenderTextureV(CurrentTextureSize);
        //set offset to the middle of the texture, and to target (0,0)
        Camera = new Camera2D(CurrentTextureSize / 2, Vector2.Zero, 0f, 1f);
        _sceneObjects = new Queue<ISceneObject>();
    }

    public void OperateCamera(CameraOperation operation)
    {
        //self explanatory
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
    public void ClearQueue()
    {
        //remove all elements of the queue
        _sceneObjects.Clear();
    }
    protected void ProcessTextureSizeChanges(Vector2 newSize)
    {
        if (CurrentTextureSize != newSize)
        {
            //unload current texture, load the new texture, move camera offset to match the middle 
            Raylib.UnloadRenderTexture(_texture);
            _texture = RaylibExtensions.LoadRenderTextureV(newSize);
            Camera.Offset = Vector2.Divide(newSize, 2);
            CurrentTextureSize = newSize;
        }
    }
    
    public void SetRenderQueue(Queue<ISceneObject> queue)
    {
        _sceneObjects = queue;
    }
    public RenderTexture2D GetSceneTexture(Vector2 textureSize)
    {
        if (_sceneObjects.Count == 0) throw new InvalidOperationException("Render queue is empty");
        ProcessTextureSizeChanges(textureSize);
        
        Raylib.BeginTextureMode(_texture);
        Raylib.BeginMode2D(Camera);
        //using lower-level rlgl api to transform coordinates
        Rlgl.PushMatrix();
        //scale and backface culling thing is required to convert coordinate system into traditional cartesian coordinates
        //scale reflects all coordiantes along the x-z plane, changing the y values
        //this is so that y increases as we move the mouse up on the scene, the coordiante system that people are more used to
        //however, this also means that we have to modify out logic slightly when doing certain things
        //all instances where this happens are commented
        Rlgl.Scalef(1f,-1f,1f);
        Rlgl.DisableBackfaceCulling();
        
        //dequeue all elements in the scene objects queue for rendering
        while (_sceneObjects.Count > 0)
        {
            _sceneObjects.Dequeue().Render();
        }
        Rlgl.PopMatrix();
        
        Raylib.EndMode2D();
        Raylib.EndTextureMode();
        
        return _texture;
    }
}