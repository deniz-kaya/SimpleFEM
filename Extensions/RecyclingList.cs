using System;
using System.Collections;
using System.Collections.Generic;

namespace SimpleFEM.Extensions;

public class RecyclingList<T> : IEnumerable<T>
    {
        private T[] _elements;
        private Stack<int> _freeSpots;

        private bool[] _occupied;

        private int _elementCount;
        
        public int LastAddedIndex { get; private set; }

        public bool Exists(T item, out int index)
        {
            index = -1;
            foreach (int i in GetIndexes())
            {
                if (EqualityComparer<T>.Default.Equals(_elements[i], item))
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
                if (EqualityComparer<T>.Default.Equals(_elements[i], item))
                {
                    return true;
                }
            }
            return false;
        }
        
        public int Count {
            get { return _elementCount; }
        }

        public bool ExistsAt(int index)
        {
            return _occupied[index];
        }
        public RecyclingList(int initialCapacity = 100)
        {
            _elements = new T[initialCapacity];
            _occupied = new bool[initialCapacity];
            _elementCount = 0;
            _freeSpots = new Stack<int>();
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
                    return _elements[index];
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
                    _elements[index] = value;
                }
            }
        }

        public bool ValidIndex(int index)
        {
            bool indexTooSmall = index < 0;
            bool isntOccupied = !_occupied[index];
            return !(indexTooSmall || isntOccupied);
        }
        public bool RemoveAt(int index)
        {
            if (ValidIndex(index))
            {
                _occupied[index] = false;
                _elementCount--;
                _freeSpots.Push(index);
                return true;
            }

            return false;
        }
        
        public bool Remove(T item)
        {
            for (int i = 0; i < _elements.Length; i++)
            {
                if (_occupied[i])
                {
                    if (EqualityComparer<T>.Default.Equals(_elements[i], item))
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
            int index = _freeSpots.Count != 0 ? _freeSpots.Pop() : _elementCount;
            _elements[index] = item;
            _occupied[index] = true;
            LastAddedIndex = index;
            _elementCount++;
        }

        public List<int> GetIndexes()
        {
            List<int> indexes = new List<int>();
            for (int i = 0; i < _elements.Length; i++)
            {
                if (_occupied[i])
                {
                    indexes.Add(i);
                }
            }
            return indexes;
        }
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _elements.Length; i++)
            {
                if (_occupied[i])
                {
                    yield return _elements[i];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }