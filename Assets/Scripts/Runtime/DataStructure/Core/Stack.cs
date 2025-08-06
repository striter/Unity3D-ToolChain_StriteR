using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Runtime.DataStructure.Core
{
    [Serializable]
    public class Stack<Element> : IEnumerable<Element>
    {
        public int Count => m_Size;

        [SerializeField] private Element[] m_Elements;
        public int m_Size;
        public Stack(int _initialCapacity = 10)
        {
            if (_initialCapacity < 10)
                _initialCapacity = 10;
            m_Elements = new Element[_initialCapacity];
            m_Size = 0;
        }

        public void Push(Element _element)
        {
            m_Elements[m_Size++] = _element;
            if (m_Size == m_Elements.Length)
                Array.Resize(ref m_Elements, m_Elements.Length * 2);
        }

        public Element Pop()
        {
            if (m_Size == 0)
                throw new InvalidOperationException();
            var element = m_Elements[--m_Size];
            m_Elements[m_Size] = default;
            return element;
        }

        public void Clear()
        {
            m_Size = 0;
            if (RuntimeHelpers.IsReferenceOrContainsReferences<Element>())
                Array.Clear(m_Elements, 0, m_Elements.Length);
        }
        
        public void TrimExcess()
        {
            if (m_Size >= m_Elements.Length * 0.9f)
                return;
            Array.Resize(ref m_Elements, m_Size);
        }

        public Element Peek() => m_Elements[m_Size - 1];
        public IEnumerator<Element> GetEnumerator()
        {
            for (var i = m_Size - 1; i >= 0; i--)
                yield return m_Elements[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}