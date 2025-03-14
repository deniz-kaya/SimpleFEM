using System.Numerics;
using Raylib_cs;

namespace SimpleFEM.Extensions;

public static class RaylibExtensions
{
    public static RenderTexture2D LoadRenderTextureV(Vector2 size)
    {
        //convert the vector2 size to the integer tuple size
        (int, int) processedSize = size.Floor().ToIntegerTuple();
        return Raylib.LoadRenderTexture(processedSize.Item1, processedSize.Item2);
    }
    
}