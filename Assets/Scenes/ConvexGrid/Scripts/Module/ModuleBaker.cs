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

            ModuleRuntimeData _data = ScriptableObject.CreateInstance<ModuleRuntimeData>();
            _data.m_OrientedMeshes = totalModuleMeshes.ToArray();
            UEAsset.CreateAssetCombination(UEPath.FileToAssetPath( filePath), _data);
        }
        public void GenerateCubeTemplates(Mesh[] _meshes,Material _sharedMaterial)
        {
            if (transform.childCount > 0)
            {
                Debug.LogWarning("Please Clear All Child Of this Object");
                return;
            }
            transform.DestroyChildren(true);
            int width = -4;
            int height = -4;
            List<GameObject> selections = new List<GameObject>();
            foreach (var tuple in UModule.IterateAllVoxelModuleBytes().LoopIndex())
            {
                var moduleByte =tuple .value;
                var possibility = new BoolQube();
                possibility.SetByteCorners(moduleByte);
                
                var bakerModel = new GameObject($"Module:{moduleByte}").transform;
                Undo.RegisterCreatedObjectUndo(bakerModel.gameObject,"Bake");
                bakerModel.SetParent(transform);
                bakerModel.localPosition = Vector3.right * (3f * width) + Vector3.forward * (3f * height) + Vector3.up * 1f;
                bakerModel.gameObject.AddComponent<ModuleBakerModel>().m_Relation = possibility;

                var moduleName = moduleByte.ToString();
                var mesh = _meshes?.Find(p => p.name.LastEquals(moduleName));
                if (mesh == null)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if(!possibility[j])
                            continue;

                        var subCube = GameObject.CreatePrimitive( PrimitiveType.Cube).transform;
                        subCube.GetComponent<MeshRenderer>().sharedMaterial = _sharedMaterial;
                        subCube.SetParent(bakerModel);
                        subCube.localScale = Vector3.one * .5f;
                        subCube.localPosition = UModule.halfUnitQube[j]+Vector3.up*.25f;
                        selections.Add(subCube.gameObject);
                    }
                }
                else
                {
                    var subMesh = new GameObject(mesh.name);
                    subMesh.AddComponent<MeshFilter>().sharedMesh = mesh;
                    subMesh.AddComponent<MeshRenderer>().sharedMaterial=_sharedMaterial;
                    subMesh.transform.SetParent(bakerModel);
                    subMesh.transform.localPosition = Vector3.zero;
                    selections.Add(subMesh);
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

    
    [CustomEditor(typeof(ModuleBaker))]
    public class ModuleBakerEditor : Editor
    {
        public UnityEngine.Object m_ModelCombination;
        public Material m_ModelMaterial;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var baker = (target as ModuleBaker);
            GUILayout.BeginVertical();
            GUILayout.Space(10f);
            GUILayout.Label("Template",UEGUIStyle_Window.m_TitleLabel);
            m_ModelCombination = EditorGUILayout.ObjectField(m_ModelCombination,typeof(UnityEngine.Object),false);
            m_ModelMaterial=(Material)EditorGUILayout.ObjectField(m_ModelMaterial,typeof(Material),false);
            if (GUILayout.Button("Generate Templates"))
            {
                Mesh[] templates = null;
                if (m_ModelCombination &&  (AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(m_ModelCombination)) as ModelImporter != null))
                    templates=AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(m_ModelCombination))  .Collect(p => p is Mesh).Select(p => p as Mesh).ToArray();
                
                baker.GenerateCubeTemplates(templates,m_ModelMaterial);
            }
            
            GUILayout.Space(10f);
            GUILayout.Label("Persistent",UEGUIStyle_Window.m_TitleLabel);
            if(GUILayout.Button("Bake"))
                baker.Bake();
            
            GUILayout.EndVertical();
        }
    }
    #endif
}