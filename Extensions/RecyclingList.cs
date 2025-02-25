using System.Collections;

namespace SimpleFEM.Extensions;

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