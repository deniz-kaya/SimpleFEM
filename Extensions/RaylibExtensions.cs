using System.Numerics;
using Raylib_cs;

namespace SimpleFEM.Extensions;

public static class RaylibExtensions
{
    public static RenderTexture2D LoadRenderTextureV(System.Numerics.Vector2 size)
    {
        (int, int) processedSize = size.Floor().ToInteger();
        return Raylib_cs.Raylib.LoadRenderTexture(processedSize.Item1, processedSize.Item2);
    }
        
}