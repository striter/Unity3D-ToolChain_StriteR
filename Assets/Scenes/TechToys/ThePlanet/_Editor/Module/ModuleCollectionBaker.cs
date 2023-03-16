using TechToys.ThePlanet.Module.Cluster;

#if UNITY_EDITOR
namespace TechToys.ThePlanet.Baking
{
    using UnityEditor.Extensions;
    using UnityEditor;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Module;
    using Object = System.Object;

    public class ModuleCollectionBaker : MonoBehaviour
    {
    }

    [CustomEditor(typeof(ModuleCollectionBaker))]
    public class ModuleCollectionBakerEditor : Editor
    {
        private ModuleCollectionBaker m_Baker;
        private void OnEnable()=>m_Baker=(target as ModuleCollectionBaker);
        private void OnDisable()=>m_Baker = null;
        private EClusterType m_Type= EClusterType.Vanilla;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.BeginVertical();
            GUILayout.Space(10f);
            GUILayout.Label("Template",UEGUIStyle_Window.m_TitleLabel);

            m_Type = (EClusterType)EditorGUILayout.EnumPopup("Type:",m_Type);
            if (GUILayout.Button("Import Cluster"))
                ImportTemplates(m_Type);
            if (GUILayout.Button("Default Cluster"))
                ImportModule("Default",null,m_Type);
            
            GUILayout.Space(10f);
            GUILayout.Label("Persistent",UEGUIStyle_Window.m_TitleLabel);
            if(GUILayout.Button("Bake"))
                Bake();
            
            GUILayout.EndVertical();
        }
        private void Bake()
        {
            if (!UEAsset.SaveFilePath(out string filePath, "asset",$"ModuleCollection_Default")) 
                return;

            ModuleCollection collectionData = CreateInstance<ModuleCollection>();
            List<Material> materials = new List<Material>();
            List<Mesh> meshes = new List<Mesh>();
            List<ScriptableObject> subData = new List<ScriptableObject>();

            var dataBakers = m_Baker.transform.GetComponentsInChildren<ModuleCollector>();
            foreach (var dataBaker in dataBakers)
                subData.Add(dataBaker.Export(meshes,materials));

            collectionData.m_MeshLibrary = meshes.ToArray();
            collectionData.m_MaterialLibrary = materials.ToArray();
            var assetPath = UEPath.FileToAssetPath(filePath);
            UEAsset.ClearSubAssets(assetPath);
            var savedCollection=UEAsset.CreateAssetCombination(assetPath, collectionData,subData);
            var subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            savedCollection.m_ModuleLibrary = subAssets.CollectAs<Object, ModuleData>().ToArray();
            EditorUtility.SetDirty(savedCollection);
        }
        private void ImportTemplates(EClusterType _clusterType)
        {
            if (!UEAsset.SelectFilePath(out var filePath, "FBX"))
                return;

            var assetPath=UEPath.FileToAssetPath(filePath);
            var importer=AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null)
                return;
            ImportModule(UEPath.GetPathName(filePath),AssetDatabase.LoadAllAssetsAtPath(assetPath).Collect(p => p is Mesh).Select(p => p as Mesh).ToArray(),_clusterType);
        }
        private void ImportModule(string _name, Mesh[] _importMeshes,EClusterType _clusterType)
        {
            var dataParent = new GameObject(_name).transform;
            dataParent.SetParent(m_Baker.transform);
            dataParent.transform.localPosition = Vector3.zero;
            
            var moduleDataBaker = dataParent.gameObject.AddComponent<ModuleCollector>();
            moduleDataBaker.ImportCluster(_importMeshes,_clusterType);
            Undo.RegisterCreatedObjectUndo(moduleDataBaker.gameObject,"Module Data Baker");
            Selection.activeObject = moduleDataBaker.gameObject;
        }
    }
}
#endif