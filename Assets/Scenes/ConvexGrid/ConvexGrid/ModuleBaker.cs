using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Voxel;
using TPoolStatic;
using UnityEngine;

namespace ConvexGrid
{
    using UnityEditor;
    public class ModuleBaker : MonoBehaviour
    {
        public ModuleData m_Data;
        
        static readonly GQuad unitQuad = new GQuad(Vector3.right + Vector3.back, Vector3.left + Vector3.back,
            Vector3.left + Vector3.forward, Vector3.right + Vector3.forward);
        private static readonly GQube unitQube = unitQuad.ConvertToQube(Vector3.up*2f, .5f);
        
        static readonly GQube[] cornerQubes=unitQuad.SplitToQubes(Vector3.up).ToArray();
        
        public void Bake()
        {
            if (m_Data == null)
                throw new Exception("Invalid Module Data Set!");
            
            List<ModulePossibilityData> totalModules = new List<ModulePossibilityData>();
            
            var vertices = TSPoolList<Vector3>.Spawn();
            var indexes = TSPoolList<int>.Spawn();
            var uvs = TSPoolList<Vector2>.Spawn();
            var normals = TSPoolList<Vector3>.Spawn();

            for (int i = 0; i < 256; i++)
            {
                var possibility = (byte) i;
                for (int j = 0; j < 8; j++)
                {
                    if(!UByte.PosValid(possibility,j))
                        continue;
                
                    cornerQubes[j].FillFacingQuad(ECubeFacing.T,vertices,indexes,uvs,normals);
                    cornerQubes[j].FillFacingQuad(ECubeFacing.D,vertices,indexes,uvs,normals);
                    cornerQubes[j].FillFacingQuad(ECubeFacing.BL,vertices,indexes,uvs,normals);
                    cornerQubes[j].FillFacingQuad(ECubeFacing.LF,vertices,indexes,uvs,normals);
                    cornerQubes[j].FillFacingQuad(ECubeFacing.FR,vertices,indexes,uvs,normals);
                    cornerQubes[j].FillFacingQuad(ECubeFacing.RB,vertices,indexes,uvs,normals);
                }
                totalModules.Add(new ModulePossibilityData(){m_Identity =possibility,
                    m_Vertices = vertices.ToArray(),
                    m_UVs=uvs.ToArray(),
                    m_Indexes = indexes.ToArray(),
                    m_Normals = normals.ToArray(),
                });
                vertices.Clear();
                indexes.Clear();
                normals.Clear();
                uvs.Clear();
            }
            
            
            TSPoolList<Vector3>.Recycle(vertices);
            TSPoolList<int>.Recycle(indexes);
            TSPoolList<Vector2>.Recycle(uvs);
            TSPoolList<Vector3>.Recycle(normals);

            m_Data.m_ModulesData = totalModules.ToArray();
        }

        public bool m_Gizmos;
        private void OnDrawGizmos()
        {
            if (!m_Gizmos||!m_Data)
                return;

            int width = -8;
            int height = -8;
            foreach (var moduleData in m_Data.m_ModulesData)
            {
                var possibility = moduleData.m_Identity;
                Gizmos.matrix = Matrix4x4.Translate(Vector3.right*3f*width+Vector3.forward*3f*height+Vector3.up*1f);
                Gizmos.color = Color.white.SetAlpha(.5f);
                if (moduleData.m_Vertices.Length > 0)
                    Gizmos_Extend.DrawLines(moduleData.m_Vertices);
                
                for (int i = 0; i < 8; i++)
                {
                    Gizmos.color = UByte.PosValid(possibility, i) ? Color.green : Color.red;
                    Gizmos.DrawSphere(unitQube[i],.1f);
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