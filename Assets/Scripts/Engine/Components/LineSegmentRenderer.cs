using System.Collections.Generic;
using System.Linq;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime
{
    public class LineSegmentRenderer : ALineRendererBase
    {
        
        [Header("Input")]
        public bool m_LocalSpace = false;
        public float3[] m_LinePositions = GTriangle.kDefault.IterateToArray();
        public bool m_ManualInput = false;
        [MFoldout(nameof(m_ManualInput), false)] public float3 m_Normal = kfloat3.up;
        [MFoldout(nameof(m_ManualInput),true)] public float3[] m_Normals =  new [] {GTriangle.kDefault.normal,GTriangle.kDefault.normal,GTriangle.kDefault.normal} ;
        private List<float3> m_FinalVertices = new List<float3>();
        //I should put all these stuffs into shaders ?
        protected override void PopulatePositions( List<Vector3> _vertices, List<Vector3> _tangents)
        {
            if (m_LinePositions == null) return;

            var length = m_LinePositions.Length;
            if (length <= 1) return;
            
            m_FinalVertices.Clear();
            m_FinalVertices.AddRange(m_LocalSpace?m_LinePositions.Select(p=>(float3)transform.localToWorldMatrix.MultiplyPoint(p)):m_LinePositions);

            for (var i = 0; i < length; i++)
            {
                _vertices.Add(m_FinalVertices[i]); 
                var normal = m_ManualInput ? m_Normals[i] : m_Normal;
                var tangent = i == length - 1
                    ? _tangents[^1]
                    : Vector3.Cross(normal,(m_FinalVertices[i + 1] - m_FinalVertices[i]).normalize());
                _tangents.Add(tangent);
            }
        }

        protected override void OnInitialize()
        {
            
        }

        protected override void OnDispose()
        {
        }

        protected override void Validate()
        {
        }

        public override void DrawGizmos(Transform _viewTransform)
        {
            base.DrawGizmos(_viewTransform);
            UGizmos.DrawLines(m_FinalVertices,p=>p);
        }
        
        public void SetPositions(float3[] _positions)
        {
            m_LinePositions = _positions;
            m_Normal = kfloat3.up;
            m_ManualInput = false;
            if(_positions.Length > 3)
                PCA.Evaluate(_positions,out var _center,out var right,out var forward,out m_Normal);
            SetDirty();
        }
        public void SetPositions(float3[] _positions,float3[] _normals)
        {
            m_LinePositions = _positions.ToArray();
            m_Normals = _normals.ToArray();
            m_ManualInput = true;
            SetDirty();
        }
    }

}
