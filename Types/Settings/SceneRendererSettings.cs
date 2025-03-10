namespace SimpleFEM.Types.Settings;

public struct SceneRendererSettings
{
    public float CameraSpeed;
    public float MaxZoom;
    public float MinZoom;
    public float ZoomIncrement;


    public SceneRendererSettings(float CameraSpeed, float MaxZoom, float MinZoom, float ZoomIncrement)
    {
        this.CameraSpeed = CameraSpeed;
        this.MaxZoom = MaxZoom;
        this.MinZoom = MinZoom;
        this.ZoomIncrement = ZoomIncrement;
    }
    public static SceneRendererSettings Default => new SceneRendererSettings(10f, 20f,0.25f, 0.25f);
}