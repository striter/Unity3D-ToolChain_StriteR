using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Voxel;
using LinqExtentions;
using TPoolStatic;
using Unity.Mathematics;
using UnityEngine;
using Object = System.Object;

namespace ConvexGrid
{
#if UNITY_EDITOR
    using UnityEditor;
    using TEditor;
    public class ModuleBaker : MonoBehaviour
    {
        public EModuleType m_ModuleType;
        public UnityEngine.Object m_Models;
    }

    [CustomEditor(typeof(ModuleBaker))]
    public class ModuleBakerEditor : Editor
    {
        private ModuleBaker m_Baker;
        private void OnEnable()
        {
            m_Baker=(target as ModuleBaker);
        }

        private void OnDisable()
        {
            m_Baker = null;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.BeginVertical();
            GUILayout.Space(10f);
            GUILayout.Label("Template",UEGUIStyle_Window.m_TitleLabel);
            if (GUILayout.Button("Generate Templates"))
                GenerateCubeTemplates();
            
            GUILayout.Space(10f);
            GUILayout.Label("Persistent",UEGUIStyle_Window.m_TitleLabel);
            if(GUILayout.Button("Bake"))
                Bake();
            
            GUILayout.EndVertical();
        }
        
        public void Bake()
        {
            if (!UEAsset.SaveFilePath(out string filePath, "asset")) 
                return;
            
            List<OrientedModuleMeshData> totalModuleMeshes = new List<OrientedModuleMeshData>();
            
            foreach (var moduleBakeMesh in m_Baker.GetComponentsInChildren<ModuleBakerModel>())
                totalModuleMeshes.Add(moduleBakeMesh.CollectModuleMesh());

            ModuleRuntimeData _data = CreateInstance<ModuleRuntimeData>();
            _data.m_OrientedMeshes = totalModuleMeshes.ToArray();
            _data.m_Type = m_Baker.m_ModuleType;
            UEAsset.CreateAssetCombination(UEPath.FileToAssetPath( filePath), _data);
        }
        public void GenerateCubeTemplates()
        {
            var bakerParent = m_Baker.transform;
            if (bakerParent.childCount > 0)
            {
                Debug.LogWarning("Please Clear All Child Of this Object");
                return;
            }

            var moduleType = m_Baker.m_ModuleType;
            var models=m_Baker.m_Models;
            var parent = m_Baker.transform;
            Mesh[] meshes = null;
            if (AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(models)) as ModelImporter != null)
                meshes=AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(models)).Collect(p => p is Mesh).Select(p => p as Mesh).ToArray();
            int width = -4;
            int height = -4;
            List<GameObject> selections = new List<GameObject>();
            foreach (var tuple in UModuleByteDealer.IterateAllVoxelModuleBytes().LoopIndex())
            {
                var moduleByte =tuple.value;
                var possibility = new Qube<bool>();
                possibility.SetByteCorners(moduleByte);
                
                var bakerModel = new GameObject($"{moduleType}:{moduleByte}").transform;
                Undo.RegisterCreatedObjectUndo(bakerModel.gameObject,"Bake");
                bakerModel.SetParent(parent);
                bakerModel.localPosition =  Vector3.right * (3f * width) + Vector3.forward * (3f * height) + Vector3.up * 1f;
                bakerModel.gameObject.AddComponent<ModuleBakerModel>().m_Relation = possibility;

                var moduleName = moduleByte.ToString();
                var mesh = meshes?.Find(p => p.name.CollectAllNumber().Equals(moduleName));
                if (mesh == null)
                {
                    Debug.Log("Invalid Mesh:"+moduleName);
                    for (int j = 0; j < 8; j++)
                    {
                        if(!possibility[j])
                            continue;

                        var subCube = GameObject.CreatePrimitive( PrimitiveType.Cube).transform;
                        subCube.SetParent(bakerModel);
                        subCube.localScale = Vector3.one * .5f;
                        subCube.localPosition = UModule.halfUnitQube[j]+Vector3.up*.25f;
                        selections.Add(subCube.gameObject);
                    }
                }
                else
                {
                    var subMesh = GameObject.CreatePrimitive( PrimitiveType.Cube).transform;
                    subMesh.GetComponent<MeshFilter>().sharedMesh = mesh;
                    subMesh.transform.SetParent(bakerModel);
                    subMesh.transform.localPosition = Vector3.zero;
                    selections.Add(subMesh.gameObject);
                }

                width++;
                if (width > 3)
                {
                    width = -4;
                    height++;
                }
            }
            Selection.objects = selections.ToArray();
        }
    }
    #endif
}