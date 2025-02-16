using System.Collections;
using System.Numerics;
using System.Threading.Tasks.Dataflow;
using Raylib_cs;

namespace SimpleFEM;

public static class DebugHelpers
{
    public static void PrintList(List<int> list)
    {
        string s = "[";
        foreach (int i in list)
        {
            s += ($"{i.ToString()}, ");
        }

        s += "]";
        Console.WriteLine(s);
    }
}
public static class RectangleExtensions
{
    public static Rectangle GetRectangleFromPoints(Vector2 point1, Vector2 point2)
    {
        float posX = MathF.Min(point1.X, point2.X);
        float posY = MathF.Min(point1.Y, point2.Y);
        float width = MathF.Abs(point1.X - point2.X);
        float height = MathF.Abs(point1.Y - point2.Y);
        return new Rectangle(posX, posY, width, height);
    }
}
public static class Vector2Extensions
{
    public static bool LiesWithinRect(this Vector2 vector, Vector2 pos1, Vector2 pos2)
    {
        if (pos1 == pos2)
        {
            return false;
        }
        float maxX = Math.Max(pos1.X, pos2.X);
        float minX = Math.Min(pos1.X, pos2.X);
        float maxY = Math.Max(pos1.Y, pos2.Y);
        float minY = Math.Min(pos1.Y, pos2.Y);
        
        return vector.X >= minX && vector.X <= maxX && vector.Y >= minY && vector.Y <= maxY;
    }
    public static Vector2 Round(this Vector2 vector)
    {
        return new(
            MathF.Round(vector.X),
            MathF.Round(vector.Y)
        );
    }

    public static Vector2 RoundToNearest(this Vector2 vector, float value)
    {
        // this is doing: 
        // Round(vector / value) * value
        return Vector2.Multiply(Round(Vector2.Divide(vector, value)), new Vector2(value));
    }
    public static Vector2 Floor(this Vector2 vector)
    {
        return new Vector2(
            MathF.Floor(vector.X),
            MathF.Floor(vector.Y)
            );
    }

    public static (int, int) ToInteger(this Vector2 vector)
    {
        Vector2 roundedVec = Round(vector);
        return new((int)roundedVec.X, (int)roundedVec.Y);
    }
}

public static class RaylibExtensions
{
    public static RenderTexture2D LoadRenderTextureV(Vector2 size)
    {
        (int, int) processedSize = size.Floor().ToInteger();
        return Raylib.LoadRenderTexture(processedSize.Item1, processedSize.Item2);
    }
        
}

public class RecyclingList<T> : IEnumerable<T>
    {
        private T[] elements;
        private Stack<int> freeSpots;

        private bool[] occupied;

        private int elementCount;

        public int LastAddedIndex { get; private set; }

        public bool Exists(T item, out int index)
        {
            index = -1;
            foreach (int i in GetIndexes())
            {
                if (EqualityComparer<T>.Default.Equals(elements[i], item))
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }

        public bool Exists(T item)
        {
            foreach (int i in GetIndexes())
            {
                if (EqualityComparer<T>.Default.Equals(elements[i], item))
                {
                    return true;
                }
            }
            return false;
        }
        
        public int Count {
            get { return elementCount; }
            private set
            {
                elementCount = value;
            }
        }

        public bool ExistsAt(int index)
        {
            return occupied[index];
        }
        public RecyclingList(int initialCapacity = 100)
        {
            elements = new T[initialCapacity];
            occupied = new bool[initialCapacity];
            elementCount = 0;
            freeSpots = new Stack<int>();
        }

        public T this[int index]
        {
            get
            {
                if (!ValidIndex(index))
                {
                    throw new IndexOutOfRangeException("Invalid index.");
                }
                else
                {
                    return elements[index];
                }
            }
            set
            {
                if (!ValidIndex(index))
                {
                    throw new IndexOutOfRangeException("Invalid index.");
                }
                else
                {
                    elements[index] = value;
                }
            }
        }

        public bool ValidIndex(int index)
        {
            bool indexTooSmall = index < 0;
            bool isntOccupied = !occupied[index];
            return !(indexTooSmall || isntOccupied);
        }
        public bool RemoveAt(int index)
        {
            if (ValidIndex(index))
            {
                occupied[index] = false;
                elementCount--;
                freeSpots.Push(index);
                return true;
            }

            return false;
        }
        
        public bool Remove(T item)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                if (occupied[i])
                {
                    if (EqualityComparer<T>.Default.Equals(elements[i], item))
                    {
                        RemoveAt(i);
                        return true;
                    }
                }
            }

            return false;
        }
        
        public void Add(T item)
        {
            int index;
            if (freeSpots.Count != 0)
            {
                index = freeSpots.Pop();
            }
            else
            {
                index = elementCount;
            }
            elements[index] = item;
            occupied[index] = true;
            LastAddedIndex = index;
            elementCount++;
        }

        public List<int> GetIndexes()
        {
            List<int> indexes = new List<int>();
            for (int i = 0; i < elements.Length; i++)
            {
                if (occupied[i])
                {
                    indexes.Add(i);
                }
            }
            return indexes;
        }
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < elements.Length; i++)
            {
                if (occupied[i])
                {
                    yield return elements[i];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }