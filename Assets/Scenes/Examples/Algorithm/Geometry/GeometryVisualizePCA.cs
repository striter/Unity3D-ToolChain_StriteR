using Geometry.PointSet;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Algorithm.Geometry
{
    public class GeometryVisualizePCA : MonoBehaviour
    {
        public float3[] randomPoints;
        [Readonly] public float3 centre;
        [Readonly] public float3 right;
        [Readonly] public float3 forward;
        [Readonly] public float3 up;

        private void OnValidate()
        {
            UPrincipleComponentAnalysis.Evaluate(randomPoints,out centre,out right,out up,out forward);
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.color = Color.white;
            foreach (var point in randomPoints)
                Gizmos.DrawSphere(point,.01f);
            
            Gizmos.matrix = transform.localToWorldMatrix*Matrix4x4.Translate(centre);
            Gizmos.DrawCube(Vector3.zero,.1f*Vector3.one);
            Gizmos.color = Color.red;
            Gizmos_Extend.DrawArrow(Vector3.zero,right,1f,.05f);
            Gizmos.color = Color.green;
            Gizmos_Extend.DrawArrow(Vector3.zero,up,1f,.05f);
            Gizmos.color = Color.blue;
            Gizmos_Extend.DrawArrow(Vector3.zero,forward,1f,.05f);
        }
        
        [Button]
        void RandomPoints()
        {
            for (int i = 0; i < randomPoints.Length; i++)
                randomPoints[i] = URandom.RandomSphere();
        }
    }
}