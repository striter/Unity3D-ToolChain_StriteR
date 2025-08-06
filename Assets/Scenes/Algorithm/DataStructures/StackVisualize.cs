using System.Linq.Extensions;
using UnityEngine;
using Runtime.DataStructure.Core;

namespace Examples.Algorithm.DataStructures
{
    public class StackVisualize : MonoBehaviour
    {
        [Readonly]
        public Stack<int> m_Stack = new();
        
        [InspectorButton(true)]
        public void Push(int _element) => m_Stack.Push(_element);
        [InspectorButton(true)]
        public int Pop() => m_Stack.Pop();
        [InspectorButton(true)]
        public void Clear() => m_Stack.Clear();
        [InspectorButton(true)]
        public void TrimExcess() => m_Stack.TrimExcess();
        private void OnDrawGizmos()
        {
            if (m_Stack.Count == 0)
                return;
            Gizmos.color = Color.white.SetA(.1f);
            var count = m_Stack.Count;
            foreach (var (index,element) in m_Stack.WithIndex())
            {
                var pos = transform.position + Vector3.up * (count - index);
                Gizmos.DrawWireCube(pos, Vector3.one);
                UGizmos.DrawString(element.ToString(), pos);
            }
            
        }
    }
}
