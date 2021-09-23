using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Procedural;
using TPool;
using TPoolStatic;
using UnityEngine;

namespace  ConvexGrid
{
    public class ModuleContainer : PoolBehaviour<PileID>
    {
        public IModuleCollector m_Collector { get; private set; }
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
            m_Mesh.Clear();
        }
        public void ModuleValidate(ModuleRuntimeData _data)
        {
            byte moduleByte = m_Collector.m_ModuleByte;
            ref var moduleData =ref _data.m_ModuleData[moduleByte];
            int totalVerticies=0;
            int totalIndexes=0;
            for (int i = 0; i < 8; i++)
            {
                if (moduleData.modules[i] < 0)
                    continue;
                
                ref var moduleMesh=ref _data.m_OrientedMeshes[moduleData.modules[i]];
                totalVerticies += moduleMesh.m_Vertices.Length;
                totalIndexes += moduleMesh.m_Indexes.Length;
            }

            var vertices = TSPoolList<Vector3>.Spawn(totalVerticies);
            var uvs = TSPoolList<Vector2>.Spawn(totalVerticies);
            var normals = TSPoolList<Vector3>.Spawn(totalVerticies);
            var indexes = TSPoolList<int>.Spawn(totalIndexes);
            for (int i = 0; i < 8; i++)
            {
                if(moduleData.modules[i]<0)
                    continue;
                ref var moduleMesh=ref _data.m_OrientedMeshes[moduleData.modules[i]];
                var moduleOrientation = moduleData.orientations[i];
                ref var orientedRotation = ref UMath.m_Rotate3DCW[moduleData.orientations[i]];
                int indexOffset = vertices.Count;
                indexes.AddRange(moduleMesh.m_Indexes.Select(p=> p + indexOffset));

                var vertexCount = moduleMesh.m_Vertices.Length;
                for (int j = 0; j < vertexCount; j++)
                {
                    var dstVertex = UModule.ModuleToObjectVertex(i,moduleOrientation, moduleMesh.m_Vertices[j],m_Collector.m_ModuleShapeLS,KConvexGrid.tileHeightHalf);
                    var dstNormal =orientedRotation* moduleMesh.m_Normals[j];
                    vertices.Add(dstVertex);
                    normals.Add(dstNormal);
                }
                uvs.AddRange(moduleMesh.m_UVs);
            }
            
            m_Mesh.Clear();
            m_Mesh.SetVertices(vertices);
            m_Mesh.SetIndices(indexes,MeshTopology.Triangles,0);
            m_Mesh.SetNormals(normals);
            m_Mesh.SetUVs(0,uvs);
            
            TSPoolList<Vector3>.Recycle(vertices);
            TSPoolList<int>.Recycle(indexes);
            TSPoolList<Vector2>.Recycle(uvs);
            TSPoolList<Vector3>.Recycle(normals);
        }
    }   
}
