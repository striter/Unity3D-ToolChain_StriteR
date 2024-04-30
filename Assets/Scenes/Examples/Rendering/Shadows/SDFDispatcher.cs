using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AlgorithmExtension;
using Runtime.DataStructure;
using Runtime.Geometry;
using Runtime.Geometry.Validation;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace Examples.Rendering.Shadows
{
    public struct BVHNode_Sphere_Capsule : IBVHNode<GSphere, GCapsule>
    {
        public IList<GCapsule> elements { get; set; }
        public GSphere boundary { get; set; }
        public int iteration { get; set; }
        
        public void SortElements(int _median, IList<GCapsule> _elements)
        {
            PrincipleComponentAnalysis.Evaluate(_elements.Select(p=>p.Center),out var center,out var right,out var up,out var forward);
             _elements.Divide(_median,
                 // .Sort(
                // ESortType.Bubble,
                (_a, _b) =>
                {
                    float aDot = math.dot(_a.Center - center, right);
                    float bDot = math.dot(_b.Center - center, right);
                    return aDot >= bDot ? 1 : -1;
                }
            );
        }

        public bool Contains(GSphere _bounds, GCapsule _element) => throw new NotImplementedException();
        public GSphere CalculateBounds(IEnumerable<GCapsule> _elements) => UGeometry.GetBoundingSphere(_elements.Select(p=>p.GetBoundingSphere()));
    }
    
    [ExecuteInEditMode]
    public class SDFDispatcher : MonoBehaviour
    {
        private const int kMaxSDFVolumeCount = 32;
        private List<float> m_VolumeIndexes = new List<float>();
        private Vector4[] m_VolumeShapes = new Vector4[kMaxSDFVolumeCount];
        
        private const int kMaxSDFCount = 1024;
        private Vector4[] m_ShapeParameters1 = new Vector4[kMaxSDFCount];
        private Vector4[] m_ShapeParameters2 = new Vector4[kMaxSDFCount];
        private int kSDFVolumeCount = Shader.PropertyToID("_SDFVolumeCount");
        private int kSDFVolumeIndexes = Shader.PropertyToID("_SDFVolumeIndexes");
        private int kSDFVolumeShapes = Shader.PropertyToID("_SDFVolumeShapes");
        
        private int kSDFParameters1 = Shader.PropertyToID("_SDFParameters1");
        private int kSDFParameters2 = Shader.PropertyToID("_SDFParameters2");
        public int m_VolumeCapacity = 4;
        public int m_MaxIteration = 4;
        private BoundingVolumeHierarchy<BVHNode_Sphere_Capsule, GSphere, GCapsule> m_BVH = new();
        
        public void Update()
        {
            if (SDFRenderer.sRenderers.Count <= 0)
                return;

            List<GCapsule> capsules = new List<GCapsule>();
            foreach (var renderer in SDFRenderer.sRenderers)
            {
                var localToWorld = renderer.transform.localToWorldMatrix;
                if (renderer.m_ShapesOS == null)
                    continue;
                foreach (var shapeOS in renderer.m_ShapesOS)
                {
                    var shapeWS = localToWorld * shapeOS;
                    capsules.Add(shapeWS);
                }
            }
            
            m_BVH.Construct(capsules,m_MaxIteration,m_VolumeCapacity);

            int volumeIndex = 0;
            int elementIndex = 0;
            m_VolumeIndexes.Clear();
            foreach (var volume in m_BVH.m_Volumes)
            {
                int start = elementIndex;
                
                for (int i = 0; i < volume.elements.Count; i++)
                {
                    var shapeWS = volume.elements[i];   
                    var parameter1 = shapeWS.origin.to4(shapeWS.radius);
                    var parameter2 = shapeWS.normal.to4(shapeWS.height);
                    m_ShapeParameters1[start+i] = (parameter1);
                    m_ShapeParameters2[start+i] = (parameter2);
                }

                elementIndex += volume.elements.Count;
                
                m_VolumeIndexes.Add(volume.elements.Count);
                m_VolumeShapes[volumeIndex] = (float4)volume.boundary;
                volumeIndex++;
            }
            
            Shader.SetGlobalInt(kSDFVolumeCount,volumeIndex);
            Shader.SetGlobalFloatArray(kSDFVolumeIndexes,m_VolumeIndexes);
            Shader.SetGlobalVectorArray(kSDFVolumeShapes,m_VolumeShapes);

            Shader.SetGlobalVectorArray(kSDFParameters1,m_ShapeParameters1);
            Shader.SetGlobalVectorArray(kSDFParameters2,m_ShapeParameters2);
        }

        public bool m_DrawGizmos;
        private void OnDrawGizmos()
        {
            if (!m_DrawGizmos)
                return;
            
            int index = 0;
            foreach (var volume in m_BVH.m_Volumes)
            {
                Gizmos.color = UColor.IndexToColor(index++);
                volume.boundary.DrawGizmos();
                foreach (var element in volume.elements)
                    element.DrawGizmos();
            }
            // foreach (var renderer in SDFRenderer.sRenderers)
            // {
            //     Gizmos.matrix = renderer.transform.localToWorldMatrix;
            //     foreach (var shapeOS in renderer.m_ShapesOS)
            //         shapeOS.DrawGizmos();
            // }
        }
    }
}