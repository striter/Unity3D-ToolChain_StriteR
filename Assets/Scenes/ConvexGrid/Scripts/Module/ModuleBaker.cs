using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Extend;
using Geometry.Pixel;
using Geometry.Voxel;
using LinqExtentions;
using TPoolStatic;
using Unity.Mathematics;
using UnityEngine;

namespace ConvexGrid
{
    using UnityEditor;
    public class ModuleBaker : MonoBehaviour
    {
        public ModuleRuntimeData m_Data;
    
        public void Bake()
        {
            if (m_Data == null)
                throw new Exception("Invalid Module Data Set!");

            List<OrientedModuleMesh> totalModuleMeshes = new List<OrientedModuleMesh>();
            
            foreach (var moduleBakeMesh in GetComponentsInChildren<ModuleBakeMesh>())
            {
                totalModuleMeshes.Add(moduleBakeMesh.CollectModuleMesh());
            }
            
            List<ModuleData> totalModules = new List<ModuleData>();
            
            for (int i = 0; i < 256; i++)
            {
                var possibility = (byte) i;
                ModuleData data = default;
                data.identity = possibility;
                for (int j = 0; j < 8; j++)
                    data.corners.SetCorner(j,(short)(UByte.PosValid(possibility,j)?0:-1));
                totalModules.Add(data);
            }

            m_Data.m_ModuleData = totalModules.ToArray();
            m_Data.m_ModuleMeshes = totalModuleMeshes.ToArray();
            EditorUtility.SetDirty(m_Data);
        }

        public bool m_Gizmos;
        public static readonly G2Quad[] splitG2Quads = UModule.unitG2Quad.SplitToQuads<G2Quad, Vector2>(true).Select(p=>new G2Quad(p.vB,p.vL,p.vF,p.vR)).ToArray();
        public static readonly GQube  shrinkQube = UModule.unitGQuad.ExpandToQUbe(Vector3.up,.5f).Resize<GQube,Vector3>(1f);
        private void OnDrawGizmos()
        {
            if (!m_Gizmos||!m_Data)
                return;
            
            int width = -8;
            int height = -8;
            var  list = TSPoolList<Vector3>.Spawn();
            foreach (var moduleData in m_Data.m_ModuleData)
            {
                var possibility = moduleData.identity;
                Gizmos.color = Color.white.SetAlpha(.3f);
                Gizmos.matrix=Matrix4x4.Translate( Vector3.right * (3f * width) + Vector3.forward * (3f * height) + Vector3.up * 1f)*Matrix4x4.Scale(Vector3.one*2f);
                for (int i = 0; i < 8; i++)
                {
                    if(moduleData.corners[i]<0)
                        continue;
                    Gizmos.color = Color.white.SetAlpha(.5f);
                    ref var mesh = ref m_Data.m_ModuleMeshes[moduleData.corners[i]];
                    list.Clear();
                    mesh.m_Vertices.Select(p=>UModule.ModuleToObjectVertex(i,p,splitG2Quads,.5f)).FillCollection(list);
                    Gizmos_Extend.DrawLines(list);
                }

                Gizmos.color = Color.cyan;
                Gizmos_Extend.DrawLinesConcat(UModule.unitGQuad.ToArray());
                for (int i = 0; i < 8; i++)
                {
                    Gizmos.color = UByte.PosValid(possibility, i) ? Color.green : Color.red;
                    Gizmos.DrawWireSphere(shrinkQube[i],.05f);
                }
                
                width++;
                if (width > 7)
                {
                    width = -8;
                    height++;
                }
            }
            TSPoolList<Vector3>.Recycle(list);
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