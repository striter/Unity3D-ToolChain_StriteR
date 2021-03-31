using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TEditor
{
    using static UERender;
    public class EWSmoothNormalGenerator : EditorWindow
    {
        GameObject m_ModelPrefab;

        enum_VertexData m_GenerateUV= enum_VertexData.UV7;
        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Select Target FBX Source");
            m_ModelPrefab = EditorGUILayout.ObjectField( m_ModelPrefab,typeof(GameObject),false) as GameObject;

            ModelImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(Selection.activeObject)) as ModelImporter;
            if (importer != null)
                m_ModelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(Selection.activeObject));
            else
                importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(m_ModelPrefab)) as ModelImporter;

            if(!importer)
            {
                EditorGUILayout.LabelField("<Color=#FF0000>Select FBX Model</Color>", UEGUIStyle_Window.m_ErrorLabel);
                return;
            }

            m_GenerateUV = (enum_VertexData)EditorGUILayout.EnumPopup("Generate UV:", m_GenerateUV);
            if (m_GenerateUV != enum_VertexData.None && GUILayout.Button("Generate"))
                GenerateSkinnedTarget(m_ModelPrefab,m_GenerateUV);

            EditorGUILayout.EndVertical();
        }

        void GenerateSkinnedTarget(GameObject _targetFBX,  enum_VertexData _generateUV)
        {
            GameObject prefabSource = GameObject.Instantiate(_targetFBX);

            SkinnedMeshRenderer[] skinnedRenderers = prefabSource.GetComponentsInChildren<SkinnedMeshRenderer>();
            MeshFilter[] meshFilters = prefabSource.GetComponentsInChildren<MeshFilter>();
            List<Mesh> sourceMeshes = new List<Mesh>();
            foreach (SkinnedMeshRenderer renderer in skinnedRenderers)
                sourceMeshes.Add(renderer.sharedMesh);
            foreach (MeshFilter filter in meshFilters)
                sourceMeshes.Add(filter.sharedMesh);

            List< KeyValuePair<string,Object>> targetSubAsset = new List< KeyValuePair<string,Object>>();
            for (int i = 0; i < sourceMeshes.Count; i++)
                targetSubAsset.Add(new KeyValuePair<string,Object>(sourceMeshes[i].name, GenerateMesh(sourceMeshes[i], _generateUV)));


            if( UECommon.SaveFilePath(out string filePath,"prefab", UEPath.RemoveExtension(UEPath.GetPathName(AssetDatabase.GetAssetPath(_targetFBX))) + "_SN"))
            {
                string assetPath =  UEPath.FilePathToAssetPath(filePath);
                GameObject mainAsset= PrefabUtility.SaveAsPrefabAsset(prefabSource,assetPath);
                UECommon.CreateOrReplaceSubAsset(assetPath,targetSubAsset.ToArray());
                Mesh[] meshes = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath).ToArray(obj => (Mesh)obj);

                skinnedRenderers = mainAsset.GetComponentsInChildren<SkinnedMeshRenderer>();
                for (int i = 0; i < skinnedRenderers.Length; i++)
                    skinnedRenderers[i].sharedMesh = meshes[i];

                meshFilters = mainAsset.GetComponentsInChildren<MeshFilter>();
                for (int i = 0; i < meshFilters.Length; i++)
                    meshFilters[i].sharedMesh = meshes.Find(p => p.name == meshFilters[i].sharedMesh.name);
                PrefabUtility.SavePrefabAsset(mainAsset);
            }
            GameObject.DestroyImmediate(prefabSource);
        }
        
        static Mesh GenerateMesh(Mesh _src,enum_VertexData _generateUV)
        {
            Mesh target = _src.Copy();
            Vector3[] smoothNormals = GenerateSmoothNormals(target, ConvertToTangentSpace(_generateUV));
            target.SetVertexData(_generateUV,smoothNormals.ToList());
            return target;
        }
        static bool ConvertToTangentSpace(enum_VertexData _target)
        {
            switch(_target)
            {
                case enum_VertexData.Normal:
                case enum_VertexData.Tangent:
                    return false;
            }
            return true;
        }
    }
}