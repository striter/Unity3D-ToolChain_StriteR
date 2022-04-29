using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Geometry;
using Geometry.Voxel;
using Procedural;
using TPool;
using TPoolStatic;
using UnityEditor;
using UnityEngine;

namespace PolyGrid.Module
{
    public class ModuleVoxel : PoolBehaviour<PolyID>
    {
        public IVoxel m_Voxel { get; private set; }
        private Mesh m_Mesh;
        private Qube<EModuleType> m_CornerTypes=KEnumQube<EModuleType>.Invalid;
        private Qube<ECornerStatus> m_CornerStatus = KEnumQube<ECornerStatus>.Invalid;
        private Qube<byte> m_CornerBytes = default;
        public override void OnPoolCreate(Action<PolyID> _DoRecycle)
        {
            base.OnPoolCreate(_DoRecycle);
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
            m_CornerTypes = KEnumQube<EModuleType>.Invalid;
            m_CornerStatus = KEnumQube<ECornerStatus>.Invalid;
            m_CornerBytes = default;
            m_Voxel = null;
            m_Mesh.Clear();
        }
        
        public void ModuleValidate(List<ModuleRuntimeData> _data,Dictionary<PolyID,ModuleCorner> _corners)
        {
            //Collect Data
            for (int i = 0; i < 8; i++)
            {
                var type = EModuleType.Invalid;
                var status = ECornerStatus.Invalid;
                var cornerID = m_Voxel.QubeCorners[i];
                if (_corners.ContainsKey(cornerID))
                {
                    var corner = _corners[cornerID];
                    type = corner.m_Type;
                    status = DModule.GetModuleStatus(i, corner.m_Status);
                }
                m_CornerTypes[i] = type;
                m_CornerStatus[i] = status;
            }
            
            //Populate Mesh
            var vertices = TSPoolList<Vector3>.Spawn();
            var uvs = TSPoolList<Vector2>.Spawn();
            var normals = TSPoolList<Vector3>.Spawn();
            var indexes = TSPoolList<int>.Spawn();
            var colors = TSPoolList<Color>.Spawn();
            m_CornerBytes = default;
            for (int i = 0; i < 8; i++)
            {
                if(m_CornerTypes[i]== EModuleType.Invalid)
                    continue;
                
                var cornerByte = m_CornerTypes.GetCornerBytes(i);
                m_CornerBytes[i] = cornerByte;
                if(!UModuleByte.IsValidByte(cornerByte))
                    continue;

                var cornerData = _data[(int)m_CornerTypes[i]];
                var cornerStatus = m_CornerStatus[i];
                cornerData.GetOrientedIndex(cornerByte ,m_CornerStatus[i],out var moduleIndex,out var moduleOrientation);
                if (moduleIndex == -1)
                {
                    Debug.LogError($"Invalid Byte:{cornerByte} {cornerStatus}");
                    continue;
                }
                ref var moduleMesh=ref cornerData[cornerStatus][moduleIndex];
                ref var orientedRotation = ref UMath.m_Rotate3DCW[moduleOrientation];
                int indexOffset = vertices.Count;
                indexes.AddRange(moduleMesh.m_Indexes.Select(p=> p + indexOffset));

                var vertexCount = moduleMesh.m_Vertices.Length;
                for (int j = 0; j < vertexCount; j++)
                {
                    var dstVertex = UModule.ModuleToObjectVertex(i,moduleOrientation, moduleMesh.m_Vertices[j],m_Voxel.CornerShapeLS,KPolyGrid.tileHeightHalf);
                    var dstNormal =orientedRotation* moduleMesh.m_Normals[j];
                    var dstColor = moduleMesh.m_Colors[j];
                    vertices.Add(dstVertex);
                    normals.Add(dstNormal);
                    colors.Add(dstColor);
                }
                uvs.AddRange(moduleMesh.m_UVs);
            }
            
            m_Mesh.Clear();
            m_Mesh.SetVertices(vertices);
            m_Mesh.SetIndices(indexes,MeshTopology.Triangles,0);
            m_Mesh.SetNormals(normals);
            m_Mesh.SetUVs(0,uvs);
            m_Mesh.SetColors(colors);
            
            TSPoolList<Vector3>.Recycle(vertices);
            TSPoolList<int>.Recycle(indexes);
            TSPoolList<Vector2>.Recycle(uvs);
            TSPoolList<Vector3>.Recycle(normals);
            TSPoolList<Color>.Recycle(colors);
        }

        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (m_Voxel == null)
                return;
            if (Selection.activeObject != this.gameObject)
                return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(Vector3.zero,.1f);
            for (int i = 0; i < 8; i++)
            {
                if (m_CornerTypes[i] == EModuleType.Invalid)
                    continue;

                var qubeCenterLS = UModule.ModuleToObjectVertex(i, 0, Vector3.one * .5f, m_Voxel.CornerShapeLS,
                    KPolyGrid.tileHeightHalf);
                Gizmos.color = m_CornerTypes[i].ToColor();
                Gizmos.DrawLine(Vector3.zero,qubeCenterLS);
                Gizmos.DrawWireSphere(qubeCenterLS,.1f);
                
                Gizmos_Extend.DrawString(qubeCenterLS,$"{m_CornerTypes[i]} {m_CornerStatus[i]} { UModuleByte.GetByte(m_CornerBytes[i],m_CornerStatus[i])}");
            }
        }
        #endif
    }   
}
