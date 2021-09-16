using System;
using System.Collections;
using System.Collections.Generic;
using Geometry.Extend;
using LinqExtentions;
using TPoolStatic;
using UnityEngine;

namespace ConvexGrid
{
    public class ModuleBakeMesh : MonoBehaviour
    {
        public BoolQube m_Relation;

        public OrientedModuleMesh CollectModuleMesh()
        {
            var vertices = TSPoolList<Vector3>.Spawn();
            var indexes = TSPoolList<int>.Spawn();
            var uvs = TSPoolList<Vector2>.Spawn();
            var normals = TSPoolList<Vector3>.Spawn();

            foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
            {
                var mesh = meshFilter.sharedMesh;

                int indexOffset = vertices.Count;
                foreach (var vertex in mesh.vertices)
                {
                    var positionWS = meshFilter.transform.localToWorldMatrix.MultiplyPoint(vertex);
                    var positionOS = transform.worldToLocalMatrix.MultiplyPoint(positionWS);
                    vertices.Add(UModule.ObjectToModuleVertex(positionOS));
                }

                foreach (var index in mesh.GetIndices(0))
                    indexes.Add(indexOffset+index);    
                
                uvs.AddRange(mesh.uv);
                normals.AddRange(mesh.normals);
            }
            
            var moduleMesh=new OrientedModuleMesh
            {
                m_Vertices = vertices.ToArray(),
                m_UVs=uvs.ToArray(),
                m_Indexes = indexes.ToArray(),
                m_Normals = normals.ToArray(),
            };
            
            TSPoolList<Vector3>.Recycle(vertices);
            TSPoolList<int>.Recycle(indexes);
            TSPoolList<Vector2>.Recycle(uvs);
            TSPoolList<Vector3>.Recycle(normals);
            return moduleMesh;
        }
        
        public void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.up*.5f,Vector3.one);
            for (int i = 0; i < 8; i++)
            {
                Gizmos.color = m_Relation[i] ? Color.green : Color.red.SetAlpha(.5f);
                Gizmos.DrawWireSphere(UModule.unitQube[i],.1f);
            }
        }
    }

}