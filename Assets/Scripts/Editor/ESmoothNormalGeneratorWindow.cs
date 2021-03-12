using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TEditor
{
    using static TEditor_Render;
    public class ESmoothNormalGeneratorWindow : EditorWindow
    {
        GameObject m_ModelPrefab;

        bool m_GenerateToTangent;
        enum_Editor_MeshUV m_GenerateUV= enum_Editor_MeshUV.UV7;
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
                EditorGUILayout.LabelField("<Color=#FF0000>Select FBX Model</Color>", TEditor_GUIStyle.m_ErrorLabel);
                return;
            }

            m_GenerateToTangent = EditorGUILayout.Toggle("Generate As Tangent:", m_GenerateToTangent);
            if (!m_GenerateToTangent)
                m_GenerateUV = (enum_Editor_MeshUV)EditorGUILayout.EnumPopup("Generate UV:", m_GenerateUV);

            bool generateVailable = m_GenerateToTangent ||(!m_GenerateToTangent&&m_GenerateUV != enum_Editor_MeshUV.None);

            if(generateVailable&& GUILayout.Button("Generate"))
                GenerateSkinnedTarget(m_ModelPrefab,m_GenerateToTangent,m_GenerateUV);

            EditorGUILayout.EndVertical();
        }

        void GenerateSkinnedTarget(GameObject _targetFBX, bool _generateTangent, enum_Editor_MeshUV _generateUV)
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
                targetSubAsset.Add(new KeyValuePair<string,Object>(sourceMeshes[i].name, GenerateMesh(sourceMeshes[i], _generateTangent, _generateUV)));


            if( TEditor.SaveFilePath(out string filePath,"prefab",TEditor.RemoveExtension( TEditor.GetPathName(AssetDatabase.GetAssetPath(_targetFBX))) + "_SmoothNormal"))
            {
                string assetPath =  TEditor.FilePathToAssetPath(filePath);
                GameObject mainAsset= PrefabUtility.SaveAsPrefabAsset(prefabSource,assetPath);
                TEditor.CreateOrReplaceSubAsset(assetPath,targetSubAsset.ToArray());
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
        
        static Mesh GenerateMesh(Mesh _src,bool _generateTangent,enum_Editor_MeshUV _generateUV)
        {
            Mesh target = _src.Copy();
            Vector3[] smoothNormals = GenerateSmoothNormals(target, !_generateTangent);
            if (_generateTangent)
                target.SetTangents(smoothNormals.ToArray(smoothNormal => smoothNormal.ToVector4(1f)));
            else
                target.SetUVs((int)_generateUV, smoothNormals);
            return target;
        }
    }
}