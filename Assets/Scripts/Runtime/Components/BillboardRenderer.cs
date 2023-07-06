using System.Collections.Generic;
using System.Linq;
using Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime
{
    
    [ExecuteInEditMode,RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
    public class BillboardRenderer : ARuntimeRendererBase
    {
        public float m_Width = 1;
        public float m_Height = 1;
        [Range(0,360f)] public float m_Rotate = 0;
        public EBillboardType m_PositionMatching = EBillboardType.Position;
        private ValueChecker<Matrix4x4> m_CameraTRChecker = new ValueChecker<Matrix4x4>();

        private void OnValidate() => m_CameraTRChecker.Set(Matrix4x4.identity);
        private void Update()
        {
            if (!Camera.current) return;
            var currentTransform = Camera.current.transform.localToWorldMatrix;
            if(m_CameraTRChecker.Check(currentTransform))
                PopulateMesh();
        }

        private static int kInstanceID = 0;
        protected override string GetInstanceName() => $"Billboard - {kInstanceID++}";

        protected override void PopulateMesh(Mesh _mesh)
        {
            if (!Camera.current) return;
            var trackingTransform = Camera.current.transform;
            m_CameraTRChecker.Set(trackingTransform.localToWorldMatrix);
            var U = trackingTransform.up;
            var R = trackingTransform.right;
            switch (m_PositionMatching)
            {
                case EBillboardType.Position:
                {
                    var Z = (trackingTransform.position - transform.position).normalized;
                    U = math.cross(R,Z);
                    R = math.cross(Z,U);
                }
                    break;
                case EBillboardType.YConstrained:
                {
                    var Z = (trackingTransform.position - transform.position).normalized;
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
    }

}
public enum EBillboardType
{
    Cheapest,
    Position,
    YConstrained,
}
