using System;
using System.Collections;
using System.Collections.Generic;

namespace SimpleFEM.Extensions;

public class RecyclingList<T> : IEnumerable<T>
    {
        public int Count => _elementCount;
        private T[] _elements;
        private Stack<int> _freeSpots;

        private bool[] _occupied;

        private int _elementCount;
        
        public int LastAddedIndex { get; private set; }
        
        public RecyclingList(int initialCapacity = 100)
        {
            //initialise everything
            _elements = new T[initialCapacity];
            _occupied = new bool[initialCapacity];
            _elementCount = 0;
            _freeSpots = new Stack<int>();
        }

        //get item at the requested spot if the index is valid
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
            //if an item exists at index, remove the item and return true, otherwise return false
            if (ValidIndex(index))
            {
                _occupied[index] = false;
                _elementCount--;
                //add the removed index to the stack of available indexes
                _freeSpots.Push(index);
                return true;
            }

            return false;
        }
        
        
        public void Add(T item)
        {
            //resize arrays if they are at full capacity
            if (_elementCount == _elements.Length)
            {
                Array.Resize(ref _elements, _elements.Length + 100);
                Array.Resize(ref _occupied, _occupied.Length + 100);
            }
            //determine the index according to the status of the available indexes stack
            //if there are available indexes, use that index, otherwise append item at the end of teh array
            int index = _freeSpots.Count != 0 ? _freeSpots.Pop() : _elementCount;
            _elements[index] = item;
            _occupied[index] = true;
            LastAddedIndex = index;
            _elementCount++;
        }
        public List<int> GetIndexes()
        {
            List<int> indexes = new List<int>();
            //get all valid indexes of items in the list
            for (int i = 0; i < _elements.Length; i++)
            {
                if (_occupied[i])
                {
                    indexes.Add(i);
                }
            }
            return indexes;
        }
        //methods for the IEnumerable interface, this is so that we can loop through the items using a foreach loop
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