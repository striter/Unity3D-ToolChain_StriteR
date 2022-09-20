using System.Collections.Generic;
using System.Linq;
using Geometry;
using UnityEngine;

namespace PolyGrid.Module.Baking
{
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.Extensions;
    public class ModuleBaker : MonoBehaviour
    {
        public EModuleType m_ModuleType;
        public ECornerStatus m_AvailableStatus;
        public UnityEngine.Object m_SourceModels;
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
            
            ModuleRuntimeData _data = CreateInstance<ModuleRuntimeData>();
            _data.m_Type = m_Baker.m_ModuleType;
            _data.m_AvailableStatus = m_Baker.m_AvailableStatus;
            
            List<OrientedModuleMeshData> totalModuleMeshes = new List<OrientedModuleMeshData>();
            foreach (var status in UEnum.GetEnums<ECornerStatus>())
            {
                totalModuleMeshes.Clear();
                if (m_Baker.m_AvailableStatus.IsFlagEnable(status))
                {
                    var parent = m_Baker.transform.Find(status.ToString());
                    foreach (var moduleBakeMesh in parent.GetComponentsInChildren<ModuleBakerCollector>())
                        totalModuleMeshes.Add(moduleBakeMesh.CollectModuleMesh(m_Baker.m_ModuleType,status));
                }
                _data[status] = totalModuleMeshes.ToArray();
            }
           
            UEAsset.CreateOrReplaceMainAsset(_data,UEPath.FileToAssetPath( filePath));
        }

        private void GenerateCubeTemplates()
        {
            var bakerParent = m_Baker.transform;
            bakerParent.UndoDestroyChildren();
            
            var models=m_Baker.m_SourceModels;
            Mesh[] meshes = null;
            if (AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(models)) as ModelImporter != null)
                meshes=AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(models)).Collect(p => p is Mesh).Select(p => p as Mesh).ToArray();
            List<GameObject> selections = new List<GameObject>();
            int height = -2;
            foreach (var status in UEnum.GetEnums<ECornerStatus>())
            {
                if (!m_Baker.m_AvailableStatus.IsFlagEnable(status))
                    continue;
                Transform parent = new GameObject(status.ToString()).transform;
                parent.SetParent( m_Baker.transform);
                parent.localPosition=Vector3.zero;
                Undo.RegisterCreatedObjectUndo(parent.gameObject, "Template");
                int width = -4;
                foreach (var tuple in UModuleByte.IterateAllValidBytes(status).LoopIndex())
                {
                    var moduleByte = tuple.value;
                    var possibility = new Qube<bool>();
                    possibility.SetByteElement(moduleByte);

                    var name= ModuleBakingDefines.GetModuleName(status,moduleByte);
                    var bakerModel = new GameObject(name).transform;
                    Undo.RegisterCreatedObjectUndo(bakerModel.gameObject, "Template");
                    bakerModel.SetParent(parent);
                    bakerModel.localPosition =
                        Vector3.right * (3f * width) + Vector3.forward * (3f * height) + Vector3.up * 1f;
                    bakerModel.gameObject.AddComponent<ModuleBakerCollector>().m_Relation = possibility;

                    var mesh = meshes?.Find(p => p.name.LastEquals(name));
                    if (mesh == null)
                    {
                        Debug.Log("Invalid Mesh:" + name);
                        for (int j = 0; j < 8; j++)
                        {
                            if (!possibility[j])
                                continue;

                            var subCube = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                            subCube.SetParent(bakerModel);
                            subCube.localScale = Vector3.one * .5f;
                            subCube.localPosition = UModule.halfUnitQube[j] + Vector3.up * .25f;
                            selections.Add(subCube.gameObject);
                        }
                    }
                    else
                    {
                        var subMesh = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
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

                height++;
            }
            Selection.objects = selections.ToArray();
        }
    }
    #endif
}