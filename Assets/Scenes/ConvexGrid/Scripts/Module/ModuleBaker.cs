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
#if UNITY_EDITOR
    using UnityEditor;
    using TEditor;
    public class ModuleBaker : MonoBehaviour
    {
        public void Bake()
        {
            if (!UEAsset.SaveFilePath(out string filePath, "asset")) 
                return;
            
            List<OrientedModuleMeshData> totalModuleMeshes = new List<OrientedModuleMeshData>();
            
            foreach (var moduleBakeMesh in GetComponentsInChildren<ModuleBakerModel>())
                totalModuleMeshes.Add(moduleBakeMesh.CollectModuleMesh());
            
            List<ModuleData> totalModules = new List<ModuleData>();
            
            for (int i = 0; i <= byte.MaxValue; i++)
            {
                var voxel = UModule.voxelModule[i];
            
                ModuleData data = default;
                data.identity = (byte)i;
                for (int j = 0; j < 8; j++)
                {
                    UModule.GetVoxelModuleUnit( voxel[j],out var moduleIndex,out var moduleOrientation);
                    data.modules.SetCorner(j,moduleIndex);
                    data.orientations.SetCorner(j,moduleOrientation);
                }
                totalModules.Add(data);
            }

            ModuleRuntimeData _data = ScriptableObject.CreateInstance<ModuleRuntimeData>();
            _data.m_ModuleData = totalModules.ToArray();
            _data.m_OrientedMeshes = totalModuleMeshes.ToArray();
            _data= UEAsset.CreateAssetCombination(UEPath.FileToAssetPath( filePath), _data);
            m_Data = _data;
        }

        public void GenerateTemplates()
        {
            if (transform.childCount > 0)
            {
                Debug.LogWarning("Please Clear All Child Of this Object");
                return;
            }
            transform.DestroyChildren(true);
            int width = -4;
            int height = -4;
            foreach (var tuple in UModule.IterateAllVoxelModuleBytes().LoopIndex())
            {
                var moduleByte =tuple .value;
                var possibility = new BoolQube();
                possibility.SetByteCorners(moduleByte);
                
                var moduleBakerMesh = new GameObject($"Module:{moduleByte}").transform;
                moduleBakerMesh.SetParent(transform);
                moduleBakerMesh.localPosition = Vector3.right * (3f * width) + Vector3.forward * (3f * height) + Vector3.up * 1f;
                moduleBakerMesh.gameObject.AddComponent<ModuleBakerModel>().m_Relation = possibility;

                for (int j = 0; j < 8; j++)
                {
                    if(!possibility[j])
                        continue;

                    var subCube = GameObject.CreatePrimitive( PrimitiveType.Cube).transform;
                    subCube.SetParent(moduleBakerMesh);
                    subCube.localScale = Vector3.one * .5f;
                    subCube.localPosition = UModule.halfUnitQube[j]+Vector3.up*.25f;
                }
                
                width++;
                if (width > 3)
                {
                    width = -4;
                    height++;
                }
            }
        }

        public bool m_Gizmos;
        public ModuleRuntimeData m_Data;
        public static readonly G2Quad[] splitG2Quads = UModule.unitG2Quad.SplitToQuads<G2Quad, Vector2>(false).Select(p=>new G2Quad(p.vB,p.vL,p.vF,p.vR)).ToArray();
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
                Gizmos.matrix =Matrix4x4.Translate( Vector3.right * (3f * width) + Vector3.forward * (3f * height) + Vector3.up * 1f)*Matrix4x4.Scale(Vector3.one*2f);
                for (int i = 0; i < 8; i++)
                {
                    if(moduleData.modules[i]<0)
                        continue;
                    Gizmos.color = Color.white.SetAlpha(.5f);
                    // Gizmos.DrawSphere(UModule.unitQube[i],.1f);
                    ref var mesh = ref m_Data.m_OrientedMeshes[moduleData.modules[i]];
                    var orientation = moduleData.orientations[i];
                    list.Clear();
                    mesh.m_Vertices.Select(p=>UModule.ModuleToObjectVertex(i,orientation,p,splitG2Quads,.5f)).FillCollection(list);
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
            if (GUILayout.Button("Templates"))
                (target as ModuleBaker).GenerateTemplates();
            if(GUILayout.Button("Bake"))
                (target as ModuleBaker).Bake();
            GUILayout.EndVertical();
        }
    }
    #endif
}