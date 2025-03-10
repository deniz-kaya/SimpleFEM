namespace SimpleFEM.Types.Settings;

public struct SceneRendererSettings
{
    public float CameraSpeed;
    public float MaxZoom;
    public float MinZoom;
    public float ZoomIncrement;


    public SceneRendererSettings(float cameraSpeed, float maxZoom, float minZoom, float zoomIncrement)
    {
        CameraSpeed = cameraSpeed;
        MaxZoom = maxZoom;
        MinZoom = minZoom;
        ZoomIncrement = zoomIncrement;
    }
    public static SceneRendererSettings Default => new SceneRendererSettings(10f, 20f,0.25f, 0.25f);
}