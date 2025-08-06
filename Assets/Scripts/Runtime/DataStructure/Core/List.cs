using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Runtime.DataStructure.Core
{
    [Serializable]
    public class List<Element> : IEnumerable<Element>
    {
        [field: SerializeField] public Element[] m_Elements { get; private set; }
        [field: SerializeField] public int m_Size { get; private set; }

        public List(int _capacity = 0)
        {
            if (_capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(_capacity), _capacity, "Non-negative number required.");
            if (_capacity == 0)
                m_Elements = Array.Empty<Element>();
            else
                m_Elements = new Element[_capacity];
        }
        
        public int Count => m_Size;

        public Element this[int _index]
        {
            get
            {
                if (_index < 0 || _index >= m_Size)
                    throw new ArgumentOutOfRangeException(nameof(_index), _index, "Index was out of range. Must be non-negative and less than the size of the collection.");
                return m_Elements[_index];
            }
        }

        public void Add(Element _element)
        {
            if (m_Size >= m_Elements.Length)
                EnsureCapacity(m_Size + 1);
            m_Elements[m_Size++] = _element;
        }

        public void Clear()
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<Element>())
                Array.Clear(m_Elements, 0, m_Elements.Length);
            m_Size = 0;
        }

        public void Remove(Element _element)
        {
            var index = Array.IndexOf(m_Elements, _element, 0, m_Size);
            if (index < 0)
                return;
            RemoveAt(index);
        }
        
        public void RemoveAt(int _index)
        {
            if (_index < 0 || _index >= m_Size)
                throw new ArgumentOutOfRangeException(nameof(_index), _index, "Index was out of range. Must be non-negative and less than the size of the collection.");
            --m_Size;
            if (_index < m_Size)
                Array.Copy(m_Elements, _index + 1, m_Elements, _index, m_Size - _index);
            if (RuntimeHelpers.IsReferenceOrContainsReferences<Element>())
                m_Elements[m_Size] = default;
        }
        
        public void TrimExcess()
        {
            if (m_Size >= m_Elements.Length * 0.9f)
                return;
            EnsureCapacity(m_Size); 
        }
        
        void EnsureCapacity(int _minimumCapacity)
        {
            if (m_Elements.Length >= _minimumCapacity)
                return;

            var num = m_Elements.Length == 0 ? 4 : m_Elements.Length * 2;
            if ((uint) num > 2146435071U)
                num = 2146435071;
            if (num < _minimumCapacity)
                num = _minimumCapacity;
            
            var oldArray = m_Elements;
            m_Elements = new Element[num];
            Array.Copy(oldArray, m_Elements, m_Size);
        }

        public IEnumerator<Element> GetEnumerator()
        {
            for (var i = 0; i < m_Size; i++)
                yield return m_Elements[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}