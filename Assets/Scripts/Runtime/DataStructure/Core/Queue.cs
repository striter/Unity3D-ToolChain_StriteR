using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Runtime.DataStructure.Core
{
    [Serializable]
    public class Queue<Element> : IEnumerable<Element>
    {
        [SerializeField]private Element[] m_Elements;
        public int m_Size = 0;
        public int m_Head;
        public int m_Tail;
        public int Count => m_Size;

        public Queue() => m_Elements = Array.Empty<Element>();
        public Queue(int _capacity) => m_Elements = new Element[_capacity];

        public void Clear()
        {
            if (m_Size != 0)
            {
                if (RuntimeHelpers.IsReferenceOrContainsReferences<Element>())
                {
                    if (m_Head < m_Tail)
                    {
                        Array.Clear(m_Elements,m_Head,m_Size);
                    }
                    else
                    {
                        Array.Clear(m_Elements,m_Head,m_Elements.Length - m_Head);
                        Array.Clear(m_Elements,0,m_Tail);
                    }
                }
            }
            m_Size = 0;
            m_Head = 0;
            m_Tail = 0;
        }
        
        public Element Peek()
        {
            if (m_Size == 0)
                throw new InvalidOperationException("Queue empty.");
            return m_Elements[m_Head];
        }

        public void Enqueue(Element _element)
        {
            if (m_Size == m_Elements.Length)
            {
                var capacity = m_Elements.Length *2;
                if (capacity < m_Elements.Length + 4)
                    capacity = m_Elements.Length + 4;
                SetCapacity(capacity);
            }
            m_Elements[m_Tail] = _element;
            MoveNext(ref m_Tail);
            ++m_Size;
        }
        
        public Element Dequeue()
        {
            if (m_Size == 0)
                throw new InvalidOperationException("Queue empty.");
            var element = m_Elements[m_Head];
            m_Elements[m_Head] = default;
            MoveNext(ref m_Head);
            --m_Size;
            return element;
        }
        
        void SetCapacity(int _capacity)
        {
            var elements = new Element[_capacity];
            if (m_Size > 0)
            {
                if (m_Head < m_Tail)
                {
                    Array.Copy(m_Elements, m_Head, elements, 0, m_Size);
                }
                else
                {
                    Array.Copy(m_Elements, m_Head, elements, 0, m_Elements.Length - m_Head);
                    Array.Copy(m_Elements, 0, elements, m_Elements.Length - m_Head, m_Tail);
                }
            }
            m_Elements = elements;
            m_Head = 0;
            m_Tail = m_Size == _capacity ? 0 : m_Size;
        }
        
        private void MoveNext(ref int index)
        {
            int num = index + 1;
            if (num == this.m_Elements.Length)
                num = 0;
            index = num;
        }

        public void TrimExcess()
        {
            if (m_Size >= (int) (m_Elements.Length * 0.9))
                return;
            SetCapacity(m_Size);
        }

        public IEnumerator<Element> GetEnumerator()
        {
            var index = m_Head;
            for (var i = 0; i < m_Size; i++)
            {
                yield return m_Elements[index];
                MoveNext(ref index);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}