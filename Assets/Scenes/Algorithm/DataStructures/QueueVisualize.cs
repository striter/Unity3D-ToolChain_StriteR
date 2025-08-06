using System.Linq.Extensions;
using Runtime.DataStructure.Core;
using UnityEngine;

namespace Examples.Algorithm.DataStructures
{
    public class QueueVisualize : MonoBehaviour
    {
        [Readonly]
        public Queue<int> m_Queue = new Queue<int>();
        
        [InspectorButton(true)]
        public void Enqueue(int _element) => m_Queue.Enqueue(_element);
        
        [InspectorButton(true)]
        public int Dequeue() => m_Queue.Dequeue();
        
        [InspectorButton(true)]
        public void Clear() => m_Queue.Clear();
        
        [InspectorButton(true)]
        public void TrimExcess() => m_Queue.TrimExcess();

        void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.white.SetA(.1f);
            foreach (var (i,value) in m_Queue.WithIndex())
            {
                var position = new Vector3(i, 0, 0);
                Gizmos.DrawWireCube(position, Vector3.one);
                UGizmos.DrawString(value.ToString(), position);
            }
        }
    }
}