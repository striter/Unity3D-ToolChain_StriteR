using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.DataStructure;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Gizmos = UnityEngine.Gizmos;

namespace Examples.Rendering.Shadows
{
    public class IbvhHelperSphereCapsule : IBVHHelper<GSphere, GCapsule>
    {
        public void SortElements(int _median, GSphere _boundary, IList<int> _elementIndexes,IList<GCapsule> _elements)
        {
            PCA.Evaluate(_elements.Select(p=>p.Origin),out var center,out var right,out var up,out var forward);
            _elementIndexes.Divide(_median,
                // .Sort(
                // ESortType.Bubble,
                (_a, _b) =>
                {
                    var aDot = math.dot(_elements[_a].Origin - center, right);
                    var bDot = math.dot(_elements[_b].Origin - center, right);
                    return aDot >= bDot ? 1 : -1;
                }
            );
        }

        public GSphere CalculateBoundary(IList<GCapsule> _elements) => UGeometry.GetBoundingSphere(_elements.Select(p=>p.GetBoundingSphere()));
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
        private BoundingVolumeHierarchy<GSphere, GCapsule,IbvhHelperSphereCapsule> m_BVH = new(4,4);
        
        List<GCapsule> capsules = new List<GCapsule>();
        public void Update()
        {
            if (SDFRenderer.sRenderers.Count <= 0)
                return;

            capsules.Clear();
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
            
            m_BVH.Construct(capsules);

            int volumeIndex = 0;
            int elementIndex = 0;
            m_VolumeIndexes.Clear();
            foreach (var volume in m_BVH.GetLeafs())
            {
                int start = elementIndex;
                
                var count = volume.elementsIndex.Count;
                for (int i = 0; i < count; i++)
                {
                    var shapeWS = volume.Index(capsules,i,out var index);   
                    var parameter1 = shapeWS.origin.to4(shapeWS.radius);
                    var parameter2 = shapeWS.normal.to4(shapeWS.height);
                    m_ShapeParameters1[start+i] = (parameter1);
                    m_ShapeParameters2[start+i] = (parameter2);
                }

                elementIndex += count;
                
                m_VolumeIndexes.Add(count);
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
            foreach (var volume in m_BVH.GetLeafs())
            {
                Gizmos.color = UColor.IndexToColor(index++);
                volume.boundary.DrawGizmos();
                foreach (var element in volume.elementsIndex)
                    capsules[element].DrawGizmos();
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