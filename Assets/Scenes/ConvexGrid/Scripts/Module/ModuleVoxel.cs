using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Extend;
using Geometry.Pixel;
using Geometry.Voxel;
using Procedural;
using TPool;
using TPoolStatic;
using UnityEditor;
using UnityEngine;

namespace  ConvexGrid
{
    public class ModuleVoxel : PoolBehaviour<PileID>
    {
        public IVoxel m_Voxel { get; private set; }
        private Mesh m_Mesh;
        private EnumQube<EModuleType> m_CornerTypes=EnumQube<EModuleType>.Invalid;
        private ByteQube m_CornerBytes = default;
        public override void OnPoolInit(Action<PileID> _DoRecycle)
        {
            base.OnPoolInit(_DoRecycle);
            m_Mesh = new Mesh() {hideFlags = HideFlags.HideAndDontSave};
            m_Mesh.MarkDynamic();
            GetComponent<MeshFilter>().sharedMesh = m_Mesh;
        }

        public ModuleVoxel Init(IVoxel _collector)
        {
            m_Voxel = _collector;
            transform.SetPositionAndRotation(_collector.Transform.position, _collector.Transform.rotation);
            return this;
        }

        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_CornerTypes = EnumQube<EModuleType>.Invalid;
            m_CornerBytes = default;
            m_Voxel = null;
            m_Mesh.Clear();
        }
        public void ModuleValidate(ModuleRuntimeData _data,Dictionary<PileID,ModuleCorner> _corners)
        {
            for (int i = 0; i < 8; i++)
            {
                var type = EModuleType.Invalid;
                var corner = m_Voxel.QubeCorners[i];
                if (_corners.ContainsKey(corner))
                    type = _corners[corner].m_Type;
                m_CornerTypes.SetCorner(i,type);
            }
            
            IntQube moduleIndexes=IntQube.NegOne;
            IntQube moduleOrientations = default;
            for (int i = 0; i < 8; i++)
            {
                if(m_CornerTypes[i]== EModuleType.Invalid)
                    continue;
                
                m_CornerBytes[i] = m_CornerTypes.GetCornerByte(i);
                UModule.GetCornerModule(m_CornerBytes[i] ,out var index,out var orientation);
                moduleIndexes[i] = index;
                moduleOrientations[i] = orientation;
            }

            var vertices = TSPoolList<Vector3>.Spawn();
            var uvs = TSPoolList<Vector2>.Spawn();
            var normals = TSPoolList<Vector3>.Spawn();
            var indexes = TSPoolList<int>.Spawn();
            for (int i = 0; i < 8; i++)
            {
                var moduleIndex = moduleIndexes[i];
                if(moduleIndex<0)
                    continue;
                var moduleOrientation = moduleOrientations[i];
                ref var moduleMesh=ref _data.m_OrientedMeshes[moduleIndex];
                ref var orientedRotation = ref UMath.m_Rotate3DCW[moduleOrientation];
                int indexOffset = vertices.Count;
                indexes.AddRange(moduleMesh.m_Indexes.Select(p=> p + indexOffset));

                var vertexCount = moduleMesh.m_Vertices.Length;
                for (int j = 0; j < vertexCount; j++)
                {
                    var dstVertex = UModule.ModuleToObjectVertex(i,moduleOrientation, moduleMesh.m_Vertices[j],m_Voxel.CornerShapeLS,KConvexGrid.tileHeightHalf);
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

        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (m_Voxel == null)
                return;
            Gizmos.matrix = transform.localToWorldMatrix;
            for (int i = 0; i < 8; i++)
            {
                if (m_CornerTypes[i] == EModuleType.Invalid)
                    continue;

                var qubeCenterLS = UModule.ModuleToObjectVertex(i, 0, Vector3.one * .5f, m_Voxel.CornerShapeLS,
                    KConvexGrid.tileHeightHalf);
                Gizmos.color = m_CornerTypes[i].ToColor();
                Gizmos.DrawWireSphere(qubeCenterLS,.1f);
                Gizmos_Extend.DrawString(qubeCenterLS,$"{m_CornerTypes[i]} {m_CornerBytes[i]}");
            }
        }
        #endif
    }   
}
