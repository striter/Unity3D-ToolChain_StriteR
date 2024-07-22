using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Rendering.Imposter
{
    [Serializable]
    public class ImposterRendererCore : ARuntimeRendererBase
    {
        [ScriptableObjectEdit] public ImposterData m_Data;
        public float m_Rotate;
        public override bool isBillboard() => true;
        private MeshRenderer m_Renderer;
        public override Mesh Initialize(Transform _transform)
        {
            m_Renderer = _transform.GetComponent<MeshRenderer>();
            return base.Initialize(_transform);
        }

        private static List<Vector3> kVertices = new List<Vector3>();
        private static List<int> kIndices = new List<int>();
        private static List<Vector2> kUVs = new List<Vector2>();
        protected override void PopulateMesh(Mesh _mesh, Transform _transform, Transform _viewTransform)
        {
            if (!m_Data)
                return;

            var center = _transform.TransformPoint(m_Data.m_BoundingSphere.center);
            var Z = _transform.worldToLocalMatrix.MultiplyVector((center - _viewTransform.position).normalized);
            var (cellRect,viewDirection) = m_Data.m_Input.GetImposterViewsNormalized().MinElement(p => math.dot(p.direction, Z));
            var viewRotation = Quaternion.LookRotation(-viewDirection, Vector3.up);

            var U = math.mul(viewRotation, kfloat3.up);
            var R = math.mul(viewRotation, kfloat3.right);
            U = math.cross(R,viewDirection).normalize();
            R = math.cross(viewDirection,U).normalize();

            var size = m_Data.m_BoundingSphere.radius;
    
            math.sincos(kmath.kDeg2Rad * m_Rotate,out var s0,out var c0);
    
            var X = size * c0 * R + size * s0 * U;
            var Y = -size * s0 * R + size * c0 * U;
            var billboard = new GQuad(-X - Y,-X + Y, X + Y,X - Y) + m_Data.m_BoundingSphere.center;
            _mesh.SetVertices(billboard.Select(p=>(Vector3)p).FillList(kVertices));
            _mesh.SetIndices(PQuad.kDefault.GetTriangleIndexes().FillList(kIndices),MeshTopology.Triangles,0);
            _mesh.SetUVs(0,G2Quad.kDefaultUV.Select(p=>(Vector2)p).FillList(kUVs));
            
            var block = new MaterialPropertyBlock();
            block.SetVector( "_MainTex_ST", m_Data.m_Input.UVToUVRect(cellRect.center).ToTexelSize());
            block.SetVector(ImposterDefine.kBounding,(float4)m_Data.m_BoundingSphere);
            block.SetVector(ImposterDefine.kRotation,((quaternion)_transform.rotation).value);
            m_Renderer.SetPropertyBlock(block);
        }

        public bool m_DrawInput;
        public override void DrawGizmos(Transform _transform,Transform _viewTransform)
        {
            base.DrawGizmos(_transform,_viewTransform);
            if (m_Data == null)
                return;
            
            Gizmos.matrix = _transform.localToWorldMatrix * Matrix4x4.TRS(Vector3.zero,Quaternion.identity,m_Data.m_BoundingSphere.radius  * Vector3.one);
            if(m_DrawInput)
                m_Data.m_Input.DrawGizmos();
            Gizmos.color = Color.blue;
            var Z = _transform.worldToLocalMatrix.MultiplyVector((_transform.position - _viewTransform.position).normalized);
            var (_,direction) = m_Data.m_Input.GetImposterViewsNormalized().MinElement(p => math.dot(p.direction, Z));
            Gizmos.DrawSphere(direction,.02f);            
        }
    }
    
    public class ImposterRenderer : ARuntimeRendererMonoBehaviour<ImposterRendererCore>
    {
    }
}