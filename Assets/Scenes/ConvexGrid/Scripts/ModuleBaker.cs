using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Voxel;
using TPoolStatic;
using Unity.Mathematics;
using UnityEngine;

namespace ConvexGrid
{
    using UnityEditor;
    public class ModuleBaker : MonoBehaviour
    {
        public ConvexMeshData m_Data;
    
        public void Bake()
        {
            if (m_Data == null)
                throw new Exception("Invalid Module Data Set!");

            List<ModuleMesh> totalModuleMeshes = new List<ModuleMesh>();
            
            var vertices = TSPoolList<Vector3>.Spawn();
            var indexes = TSPoolList<int>.Spawn();
            var uvs = TSPoolList<Vector2>.Spawn();
            var normals = TSPoolList<Vector3>.Spawn();
            UModule.halfQubeFwd.FillFacingQuad(ECubeFacing.T,vertices,indexes,uvs,normals);
            UModule.halfQubeFwd.FillFacingQuad(ECubeFacing.D,vertices,indexes,uvs,normals);
            UModule.halfQubeFwd.FillFacingQuad(ECubeFacing.BL,vertices,indexes,uvs,normals);
            UModule.halfQubeFwd.FillFacingQuad(ECubeFacing.LF,vertices,indexes,uvs,normals);
            UModule.halfQubeFwd.FillFacingQuad(ECubeFacing.FR,vertices,indexes,uvs,normals);
            UModule.halfQubeFwd.FillFacingQuad(ECubeFacing.RB,vertices,indexes,uvs,normals);
            
            totalModuleMeshes.Add(new ModuleMesh
            {
                m_Vertices = vertices.ToArray(),
                m_UVs=uvs.ToArray(),
                m_Indexes = indexes.ToArray(),
                m_Normals = normals.ToArray(),
            });
            
            TSPoolList<Vector3>.Recycle(vertices);
            TSPoolList<int>.Recycle(indexes);
            TSPoolList<Vector2>.Recycle(uvs);
            TSPoolList<Vector3>.Recycle(normals);
            
            List<ModuleData> totalModules = new List<ModuleData>();
            
            for (int i = 0; i < 256; i++)
            {
                var possibility = (byte) i;
                ModuleData data = default;
                data.identity = possibility;
                for (int j = 0; j < 8; j++)
                    data.SetCorner(j,UByte.PosValid(possibility,j)?0:-1);
                totalModules.Add(data);
            }

            m_Data.m_ModuleData = totalModules.ToArray();
            m_Data.m_ModuleMeshes = totalModuleMeshes.ToArray();
            EditorUtility.SetDirty(m_Data);
        }

        public bool m_Gizmos;
        private void OnDrawGizmos()
        {
            if (!m_Gizmos||!m_Data)
                return;
            
            int width = -8;
            int height = -8;
            var shrinkQube = UModule.unitQube.Shrink<GQube,Vector3>(.8f);
            foreach (var moduleData in m_Data.m_ModuleData)
            {
                var possibility = moduleData.identity;
                Gizmos.color = Color.white.SetAlpha(.3f);
                Matrix4x4 translateMatrix = Matrix4x4.Translate( Vector3.right * (3f * width) + Vector3.forward * (3f * height) + Vector3.up * 1f);
                for (int i = 0; i < 8; i++)
                {
                    Gizmos.matrix = translateMatrix * UModule.LocalToModuleMatrix[i];
                    if(moduleData[i]<0)
                        continue;
                    Gizmos.color = Color.white.SetAlpha(.5f);
                    ref var mesh = ref m_Data.m_ModuleMeshes[moduleData[i]];
                    Gizmos_Extend.DrawLines(mesh.m_Vertices);
                }

                Gizmos.matrix = translateMatrix;
                Gizmos.color = Color.cyan;
                Gizmos_Extend.DrawLinesConcat(UModule.unitGQuad.ToArray());
                for (int i = 0; i < 8; i++)
                {
                    Gizmos.color = UByte.PosValid(possibility, i) ? Color.green : Color.red;
                    Gizmos.DrawSphere(shrinkQube[i],.1f);
                }
                
                width++;
                if (width > 7)
                {
                    width = -8;
                    height++;
                }
            }
        }
        
    }

    [CustomEditor(typeof(ModuleBaker))]
    public class ModuleBakerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.BeginVertical();
            if(GUILayout.Button("Bake"))
                (target as ModuleBaker).Bake();
            GUILayout.EndVertical();
        }
    }
}