using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.Geometry;
using Runtime.Geometry.Validation;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public class FLineSegmentRenderer : ALineRendererBase
    {
        [Header("Input")]
        public float3[] m_LinePositions = GTriangle.kDefault.IterateToArray();
        public bool m_ManualInput = false;
        [MFoldout(nameof(m_ManualInput), false)] public float3 m_Normal = kfloat3.up;
        [MFoldout(nameof(m_ManualInput),true)] public float3[] m_Normals =  new [] {GTriangle.kDefault.normal,GTriangle.kDefault.normal,GTriangle.kDefault.normal} ;
        
        private static int kInstanceID = 0;
        protected override string GetInstanceName() => $"Line - ({kInstanceID++})";

        //I should put all these stuffs into shaders ?
        protected override void PopulatePositions(Transform _transform, List<Vector3> _vertices, List<Vector3> _tangents)
        {
            if (m_LinePositions == null) return;

            var length = m_LinePositions.Length;
            if (length <= 1) return;

            for (int i = 0; i < length; i++)
            {
                _vertices.Add(m_LinePositions[i]);
                var normal = m_ManualInput ? m_Normals[i] : m_Normal;
                var tangent = i == length - 1
                    ? _tangents[^1]
                    : Vector3.Cross(normal,(m_LinePositions[i + 1] - m_LinePositions[i]).normalize());
                _tangents.Add(tangent);
            }
        }

        public override void DrawGizmos(Transform _transform)
        {
            base.DrawGizmos(_transform);
            UGizmos.DrawLines(m_LinePositions,p=>p);
        }
    }

    public class LineSegmentRenderer : ARuntimeRendererMonoBehaviour<FLineSegmentRenderer>
    {
        public void SetPositions(float3[] _positions)
        {
            meshConstructor.m_LinePositions = _positions;
            meshConstructor.m_Normal = kfloat3.up;
            meshConstructor.m_ManualInput = false;
            if(_positions.Length > 3)
                PrincipleComponentAnalysis.Evaluate(_positions,out var _center,out var right,out var forward,out meshConstructor.m_Normal);
            PopulateMesh();
        }
        public void SetPositions(float3[] _positions,float3[] _normals)
        {
            meshConstructor.m_LinePositions = _positions.ToArray();
            meshConstructor.m_Normals = _normals.ToArray();
            meshConstructor.m_ManualInput = true;
            PopulateMesh();
        }
    }

}
