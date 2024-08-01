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
        [ScriptableObjectEdit(true)] public ImposterData m_Data;
        public override bool isBillboard() => true;
        private MeshRenderer m_Renderer;
        private static int kInstanceID = 0;
        protected override string GetInstanceName() => $"Imposter - {kInstanceID++}";

        public override Mesh Initialize(Transform _transform)
        {
            m_Renderer = _transform.GetComponent<MeshRenderer>();
            return base.Initialize(_transform);
        }

        public override void OnValidate()
        {
            base.OnValidate();
            if(m_Renderer!=null && m_Data != null)
                m_Renderer.sharedMaterial = m_Data.m_Material;
        }

        private static List<Vector3> kVertices = new List<Vector3>();
        private static List<int> kIndices = new List<int>();
        private static List<Vector4> kUVs = new List<Vector4>();
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
            var axis = GAxis.kDefault;
            if (!m_Data.m_Interpolate)
            {
                var corner = m_Data.m_Input.GetImposterViewsNormalized().MaxElement(p => math.dot(p.direction, viewDirectionOS));
                axis = GAxis.ForwardBillboard(0,-corner.direction);
                math.sincos(0,out var s0,out var c0);
                var X = size * c0 * axis.right + size * s0 * axis.up;
                var Y = -size * s0 * axis.right + size * c0 * axis.up;
                var billboard = new GQuad(-X - Y,-X + Y, X + Y,X - Y) + m_Data.m_BoundingSphere.center;
                weights[0] = 1;

                _mesh.SetVertices(billboard.Select(p=>(Vector3)p).FillList(kVertices));
                _mesh.SetIndices(PQuad.kDefault.GetTriangleIndexes().FillList(kIndices),MeshTopology.Triangles,0);
                var texelSize = corner.rect.ToTexelSize();
                _mesh.SetUVs(0,G2Quad.kDefaultUV.Select(p=> (Vector4)(URender.TransformTex(p ,texelSize)).to4()).FillList(kUVs));
            }
            else
            {
                var output = m_Data.m_Input.GetImposterViews(viewDirectionOS);
                var centerOS = output.centroid;
                axis = GAxis.ForwardBillboard(0,-centerOS);
                math.sincos(0,out var s0,out var c0);
                var X = size * c0 * axis.right + size * s0 * axis.up;
                var Y = -size * s0 * axis.right + size * c0 * axis.up;
                var billboard = new GQuad(-X - Y,-X + Y, X + Y,X - Y) + m_Data.m_BoundingSphere.center;

                _mesh.SetVertices(billboard.Select(p=>(Vector3)p).FillList(kVertices));
                _mesh.SetIndices(PQuad.kDefault.GetTriangleIndexes().FillList(kIndices),MeshTopology.Triangles,0);

                for (var texelIndex = 0; texelIndex < 3; texelIndex++)
                {
                    var weight = output.weights[texelIndex];
                    if ( weight == 0 )
                        continue;
                
                    var corner = output.corners[texelIndex];
                    var forwardOS = corner.direction;
                    var parallax = -axis.GetUV(forwardOS) * m_Data.m_Parallax * 2;
                    var texelST = corner.rect.ToTexelSize();
                    _mesh.SetUVs(texelIndex,G2Quad.kDefaultUV.Select(p=>(Vector4)(URender.TransformTex(p  ,texelST)+ parallax * m_Data.m_Input.cellSizeNormalized).to4()).FillList(kUVs));
                    weights[texelIndex] = weight;
                }
            }
            
            block.SetVector(ImposterDefine.kWeights,weights);
            block.SetVector(ImposterDefine.kBounding,(float4)m_Data.m_BoundingSphere);
            block.SetVector(ImposterDefine.kRotation,((quaternion)_transform.rotation).value);
            block.SetVector("_ImposterViewDirection",axis.forward.to4());
            
            m_Renderer.SetPropertyBlock(block);
        }

        public bool m_DrawInput;
        public override void DrawGizmos(Transform _transform,Transform _viewTransform)
        {
            base.DrawGizmos(_transform,_viewTransform);
            if (m_Data == null)
                return;

            var center = _transform.TransformPoint(m_Data.m_BoundingSphere.center);

            Gizmos.matrix = Matrix4x4.TRS(center,Quaternion.identity,_transform.lossyScale * m_Data.m_BoundingSphere.radius);
            if(m_DrawInput)
                m_Data.m_Input.DrawGizmos();
            var viewDirection = _transform.worldToLocalMatrix.rotation * (_viewTransform.position - center).normalized;
            

            var output = m_Data.m_Input.GetImposterViews(viewDirection);
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(output.centroid, Vector3.one * 0.01f);
            for(var i = 0 ; i < 3 ; i++)
            {
                var weight = output.weights[i];
                var corner = output.corners[i];
                Gizmos.color =  UColor.IndexToColor(i).SetA(weight);
                Gizmos.DrawCube(corner.direction, Vector3.one * 0.02f);
            }
        }
    }
    
    public class ImposterRenderer : ARuntimeRendererMonoBehaviour<ImposterRendererCore>
    {
    }
}