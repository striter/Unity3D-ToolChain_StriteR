using System.Collections;
using System.Collections.Generic;
using System.Linq.Extensions;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Dome
{
    public class FDomeGrid : ADomeController,IGraphPathFinding<FDomeCell>
    {
        [ScriptableObjectEdit] public FDomeGridData m_Data;
        public float4[] initialTechPoints { get; private set; }
        private GSphere m_Bounds;
        
        public List<FDomeCell> m_Vertices { get; private set; }
        public override void OnInitialized()
        {
            var output = m_Data.Output();
            m_Vertices = output.vertices;
            m_Bounds = output.bounds;
            initialTechPoints = m_Data.initialTechPoints;
        }

        public override void Tick(float _deltaTime)
        {
        }

        public override void Dispose()
        {
        }

        public int Count => m_Vertices.Count;
        public float Heuristic(FDomeCell _src, FDomeCell _dst) => 1;
        public float Cost(FDomeCell _src, FDomeCell _dst)=> math.lengthsq(_src.positions.Origin - _dst.positions.Origin);
        public IEnumerable<FDomeCell> GetAdjacentNodes(FDomeCell _src)
        {
            foreach (var connection in _src.connections)
            {
                if(connection < 0)
                    continue;
                yield return m_Vertices[connection];
            }
        }


        public FDomeCell Validate(float3 _position)
        {
            var nearestCell = m_Vertices.MinElement(p =>
                math.lengthsq(p.available ? p.positions.Origin - _position : float.MaxValue));
            
            return nearestCell;
        }

        public FDomeCell Random()
        {
            return m_Vertices.RandomElement();
        }

        public float3 ConstrainPosition(float3 _src)
        {
            if (!m_Bounds.Contains(_src))
                return m_Bounds.GetSupportPoint(_src);
            return _src;
        }
        
        #if UNITY_EDITOR
            public bool m_DrawGizmos;
            private void OnDrawGizmosSelected()
            {
                if (!m_DrawGizmos)
                    return;
                
                Gizmos.matrix = transform.localToWorldMatrix;
                m_Data?.DrawGizmos();
            }


            [InspectorButton]
            void NewDomeData() => UnityEditor.Extensions.UEAsset.CreateScriptableInstanceAtCurrentRoot<FDomeGridData>("DomeGridData");
#endif
        public IEnumerator<FDomeCell> GetEnumerator() => m_Vertices.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public bool PositionToNode(float3 _position, out FDomeCell _node)
        {
            _node = m_Vertices.MinElement(p => (p.positions.Origin - _position).magnitude());
            return true;
        }

        public bool NodeToPosition(FDomeCell _node, out float3 _position)
        {
            _position = _node.positions.Origin;
            return true;
        }
    }
}
