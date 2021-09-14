using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using TPool;
using TPoolStatic;
using UnityEngine;

namespace  ConvexGrid
{
    public class ModuleContainer : PoolBehaviour<PileID>
    {
        private IModuleCollector m_Collector;
        private Mesh m_Mesh;
        public override void OnPoolInit(Action<PileID> _DoRecycle)
        {
            base.OnPoolInit(_DoRecycle);
            m_Mesh = new Mesh() {hideFlags = HideFlags.HideAndDontSave};
            m_Mesh.MarkDynamic();
            GetComponent<MeshFilter>().sharedMesh = m_Mesh;
        }

        public ModuleContainer Init(IModuleCollector _collector)
        {
            m_Collector = _collector;
            transform.SetPositionAndRotation(_collector.m_ModuleTransform.position, _collector.m_ModuleTransform.rotation);
            return this;
        }

        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_Collector = null;
        }
        public void ModuleValidate(ConvexMeshData _data)
        {
            byte moduleByte = m_Collector.m_ModuleByte;
            ref var data =ref _data.m_ModuleData[moduleByte];

            var vertices = TSPoolList<Vector3>.Spawn();
            var indexes = TSPoolList<int>.Spawn();
            var uvs = TSPoolList<Vector2>.Spawn();
            var normals = TSPoolList<Vector3>.Spawn();
            for (int i = 0; i < 8; i++)
            {
                if(data[i]<0)
                    continue;
                ref var moduleMesh=ref _data.m_ModuleMeshes[i];
                ref var localToModuleMatrix=ref UModule.LocalToModuleMatrix[i];

                var moduleDirection = localToModuleMatrix.MultiplyPoint(Vector3.forward);

                int indexOffset = vertices.Count;
                indexes.AddRange(moduleMesh.m_Indexes.Select(p=>p+=indexOffset));

                for (int j = 0; j <  moduleMesh.m_Vertices.Length; i++)
                {
                    Vector3 srcVector = moduleMesh.m_Vertices[j];
                    Vector3 dstVector = srcVector;
                    vertices.Add(dstVector);
                }
                
                normals.AddRange(moduleMesh.m_Normals);
                uvs.AddRange(moduleMesh.m_UVs);
            }
            
            m_Mesh.Clear();
            m_Mesh.SetVertices(vertices);
            m_Mesh.SetIndices(indexes,MeshTopology.Quads,0);
            m_Mesh.SetNormals(normals);
            m_Mesh.SetUVs(0,uvs);
            
            TSPoolList<Vector3>.Recycle(vertices);
            TSPoolList<int>.Recycle(indexes);
            TSPoolList<Vector2>.Recycle(uvs);
            TSPoolList<Vector3>.Recycle(normals);
        }
    }   
}
