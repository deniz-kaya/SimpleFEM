namespace SimpleFEM.Types.Settings;

public struct CameraSettings
{
    public float CameraSpeed;
    public float MaxZoom;
    public float MinZoom;
    public float ZoomIncrement;


    public CameraSettings(float cameraSpeed, float maxZoom, float minZoom, float zoomIncrement)
    {
        CameraSpeed = cameraSpeed;
        MaxZoom = maxZoom;
        MinZoom = minZoom;
        ZoomIncrement = zoomIncrement;
    }
    public static CameraSettings Default => new CameraSettings(10f, 20f,0.25f, 0.25f);
}