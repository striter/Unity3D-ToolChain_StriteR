using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Extensions;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Examples.Algorithm.DataStructures
{
    public class ListVisualization : MonoBehaviour
    {
        [Readonly]
        public List<int> m_List = new();
        
        [InspectorButton(true)]
        public void Add(int _element) => m_List.Add(_element);
        
        [InspectorButton(true)]
        public void Remove(int _element) => m_List.Remove(_element);
        
        [InspectorButton(true)]
        public void Clear() => m_List.Clear();
        
        [InspectorButton(true)]
        public void TrimExcess() => m_List.TrimExcess();

        private void OnDrawGizmos()
        {
            if (m_List.Count == 0)
                return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.white.SetA(.1f);
            foreach (var (index,element) in m_List.WithIndex())
            {
                var pos = Vector3.right * index;
                Gizmos.DrawWireCube(pos, Vector3.one);
                UGizmos.DrawString(element.ToString(), pos);
            }
        }
    }
    
}