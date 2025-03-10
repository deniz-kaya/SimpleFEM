using System.Numerics;
using Raylib_cs;

namespace SimpleFEM.Extensions;

public static class RaylibExtensions
{
    public static RenderTexture2D LoadRenderTextureV(Vector2 size)
    {
        (int, int) processedSize = size.Floor().ToInteger();
        return Raylib.LoadRenderTexture(processedSize.Item1, processedSize.Item2);
    }

    public static Color Vector4ToColor(this Vector4 v)
    {
        return new Color(
            (byte)(v.X * 255),
            (byte)(v.Y * 255),
            (byte)(v.Z * 255),
            (byte)(v.W * 255)
            );

    }
}