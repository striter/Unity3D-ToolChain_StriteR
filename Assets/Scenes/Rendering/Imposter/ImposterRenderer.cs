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
        public bool m_Interpolate;
        [MFoldout(nameof(m_Interpolate),true),Range(0,1)] public float m_Parallax;
        public override bool isBillboard() => true;
        private MeshRenderer m_Renderer;
        private static int kInstanceID = 0;
        protected override string GetInstanceName() => $"Imposter - {kInstanceID++}";

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
            var viewDirectionWS = (_viewTransform.position - center).normalized;
            var viewDirectionOS = _transform.worldToLocalMatrix.rotation *viewDirectionWS;
            
            var weights = float4.zero;
            var size = m_Data.m_BoundingSphere.radius;
            var block = new MaterialPropertyBlock();
            if (!m_Interpolate)
            {
                var corner = m_Data.m_Input.GetImposterViewsNormalized().MaxElement(p => math.dot(p.direction, viewDirectionOS));
                var axis = GAxis.ForwardBillboard(0,-corner.direction);
                math.sincos(0,out var s0,out var c0);
                var X = size * c0 * axis.right + size * s0 * axis.up;
                var Y = -size * s0 * axis.right + size * c0 * axis.up;
                var billboard = new GQuad(-X - Y,-X + Y, X + Y,X - Y) + m_Data.m_BoundingSphere.center;
                weights[0] = 1;
                
                _mesh.SetVertices(billboard.Select(p=>(Vector3)p).FillList(kVertices));
                _mesh.SetIndices(PQuad.kDefault.GetTriangleIndexes().FillList(kIndices),MeshTopology.Triangles,0);
                _mesh.SetUVs(0,G2Quad.kDefaultUV.Select(p=> (Vector2)URender.TransformTex(p  ,corner.rect.ToTexelSize())).FillList(kUVs));
            }
            else
            {
                var F = -viewDirectionOS;    
                var billboardRotation = Quaternion.LookRotation(F, Vector3.up);
            
                var U = math.mul(billboardRotation, kfloat3.up);
                var R = math.mul(billboardRotation, kfloat3.right);
                U = math.cross(R,viewDirectionOS).normalize();
                R = math.cross(viewDirectionOS,U).normalize();

                math.sincos(0,out var s0,out var c0);
                var X = size * c0 * R + size * s0 * U;
                var Y = -size * s0 * R + size * c0 * U;
                var billboard = new GQuad(-X - Y,-X + Y, X + Y,X - Y) + m_Data.m_BoundingSphere.center;

                _mesh.SetVertices(billboard.Select(p=>(Vector3)p).FillList(kVertices));
                _mesh.SetIndices(PQuad.kDefault.GetTriangleIndexes().FillList(kIndices),MeshTopology.Triangles,0);
                
                var texelIndex = 0;
                foreach (var (imposterViewData,weight) in m_Data.m_Input.GetImposterViews(viewDirectionOS))
                {
                    if(weight == 0)
                        continue;
                
                    var forwardOS = imposterViewData.direction;
                    var rightOS = math.normalize(math. cross( forwardOS, kfloat3.up) );
                    var upOS = math.normalize(math.cross( rightOS, forwardOS ));
                    var axis = new GAxis(0,rightOS,upOS);
                    var parallax = axis.GetUV(viewDirectionOS) * m_Parallax * 2 * (1-weight);
                    var texelST = imposterViewData.rect.ToTexelSize();
                    _mesh.SetUVs(texelIndex,G2Quad.kDefaultUV.Select(p=> (Vector2)(URender.TransformTex(p  ,texelST)+ parallax * m_Data.m_Input.cellSizeNormalized)).FillList(kUVs));
                    weights[texelIndex] = weight;
                    texelIndex += 1;
                }
            }
            
            block.SetVector(ImposterDefine.kWeights,weights);
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

            var center = _transform.TransformPoint(m_Data.m_BoundingSphere.center);
            
            Gizmos.matrix = _transform.localToWorldMatrix * Matrix4x4.TRS(center,Quaternion.identity,m_Data.m_BoundingSphere.radius  * Vector3.one);
            var viewDirection = _transform.worldToLocalMatrix.rotation * (_viewTransform.position - center).normalized;
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(viewDirection, Vector3.one * 0.01f);
            
            if(m_DrawInput)
                m_Data.m_Input.DrawGizmos();

            var index = 0;
            foreach (var (corner, weight) in m_Data.m_Input.GetImposterViews(viewDirection))
            {
                Gizmos.color =  UColor.IndexToColor(index++).SetA(weight);
                Gizmos.DrawCube(corner.direction, Vector3.one * 0.02f);
            }
        }
    }
    
    public class ImposterRenderer : ARuntimeRendererMonoBehaviour<ImposterRendererCore>
    {
    }
}