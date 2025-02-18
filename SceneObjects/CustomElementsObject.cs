using System.Numerics;
using Raylib_cs;

namespace SimpleFEM.SceneObjects;
//most likely never going to use this, but its here
public class CustomElementsObject : ISceneObject
{

    private Queue<(bool custom, Vector2 position1, Vector2 position2)> elements;
    private Queue<(Color color, float thickness)> elementProperties;
    private Color defaultColor;
    private float defaultThickness;
    
    public CustomElementsObject(Color defaultColor, float defaultThickness)
    {
        elements = new();
        elementProperties = new();
        this.defaultColor = defaultColor;
        this.defaultThickness = defaultThickness;
    }

    public void AddElement(Vector2 position1, Vector2 position2, Color? color = null, float? thickness = null)
    {
        if (color is not null || thickness is not null)
        {
            elements.Enqueue((true, position1, position2));
            Color validColor = color ?? defaultColor;
            float validThickness = thickness ?? defaultThickness;
            elementProperties.Enqueue((validColor, validThickness));
        }
        else
        {
            elements.Enqueue((false, position1, position2));
        }
    }
    public void Render()
    {
        while (elements.Count > 0){
            (bool custom, Vector2 position1, Vector2 position2) e = elements.Dequeue();
            if (e.custom)
            {
                (Color color, float thickness) properties = elementProperties.Dequeue();
                Raylib.DrawLineEx(e.position1, e.position2, properties.thickness, properties.color);
            }
            else
            {
                Raylib.DrawLineEx(e.position1, e.position2, defaultThickness, defaultColor);
            }
        }
    }
}