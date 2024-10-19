using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public class FBillboardRendererCore : ARuntimeRendererBase,IRuntimeRendererBillboard
    {
        public float m_Width = 1;
        public float m_Height = 1;
        [Range(0,360f)] public float m_Rotate = 0;
        public EBillboardType m_PositionMatching = EBillboardType.Position;

        protected override void PopulateMesh(Mesh _mesh,Transform _transform,Transform _viewTransform)
        {
            var U = _viewTransform.up;
            var R = _viewTransform.right;
            switch (m_PositionMatching)
            {
                case EBillboardType.Position:
                {
                    var Z = (_viewTransform.position - _transform.position).normalized;
                    U = math.cross(R,Z);
                    R = math.cross(Z,U);
                }
                    break;
                case EBillboardType.YConstrained:
                {
                    var Z = (_viewTransform.position - _transform.position).normalized;
                    U = Vector3.up;
                    R = math.cross(Z,U).normalize();
                }
                    break;
            }

            var w = m_Width / 2f;
            var h = m_Height / 2f;
            math.sincos(kmath.kDeg2Rad * m_Rotate,out var s0,out var c0);
            
            var X = w * c0 * R + w * s0 * U;
            var Y = -h * s0 * R + h * c0 * U;
            var billboard = new Quad<Vector3>(-X - Y,-X + Y, X + Y,X - Y);
            List<Vector3> vertices = new List<Vector3>(billboard);
            List<int> indices = new List<int>(PQuad.kDefault.GetTriangleIndexes());
            List<Vector2> uvs = new List<Vector2>(G2Quad.kDefaultUV.Select(p=>(Vector2)p));
            _mesh.SetVertices(vertices);
            _mesh.SetIndices(indices,MeshTopology.Triangles,0);
            _mesh.SetUVs(0,uvs);
        }

        public bool Billboard => true;
    }
    
    public class BillboardRenderer : ARuntimeRendererMonoBehaviour<FBillboardRendererCore>
    {
    }
}
public enum EBillboardType
{
    Cheapest,
    Position,
    YConstrained,
}
