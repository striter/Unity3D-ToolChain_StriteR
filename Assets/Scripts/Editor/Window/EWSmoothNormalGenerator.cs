using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using UnityEditor;
using UnityEngine;

namespace UnityEditor.Extensions
{
    using static UModeling;
    public class SmoothNormalGenerator : EditorWindow
    {
        GameObject m_ModelPrefab;

        EVertexAttribute m_GenerateUV= EVertexAttribute.UV7;
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

            m_GenerateUV = (EVertexAttribute)EditorGUILayout.EnumPopup("Generate UV:", m_GenerateUV);
            if (m_GenerateUV != EVertexAttribute.None && GUILayout.Button("Generate"))
                if (UEAsset.SaveFilePath(out string filePath, "prefab", UEPath.RemoveExtension(UEPath.GetPathName(AssetDatabase.GetAssetPath(m_ModelPrefab))) + "_SN"))
                    GenerateSkinnedTarget(UEPath.FileToAssetPath(filePath), m_ModelPrefab, m_GenerateUV);

            EditorGUILayout.EndVertical();
        }

        public static void GenerateSkinnedTarget(string assetPath, GameObject _targetFBX, EVertexAttribute _generateUV)
        {
            GameObject prefabSource = Instantiate(_targetFBX);

            SkinnedMeshRenderer[] skinnedRenderers = prefabSource.GetComponentsInChildren<SkinnedMeshRenderer>();
            MeshFilter[] meshFilters = prefabSource.GetComponentsInChildren<MeshFilter>();
            List<Mesh> sourceMeshes = new List<Mesh>();
            foreach (SkinnedMeshRenderer renderer in skinnedRenderers)
                sourceMeshes.Add(renderer.sharedMesh);
            foreach (MeshFilter filter in meshFilters)
                sourceMeshes.Add(filter.sharedMesh);
            
            List<Object> targetSubAsset = new List<Object>();
            for (int i = 0; i < sourceMeshes.Count; i++)
                targetSubAsset.Add( GenerateMesh(sourceMeshes[i], _generateUV));

            GameObject mainAsset = PrefabUtility.SaveAsPrefabAsset(prefabSource, assetPath);
            UEAsset.CreateOrReplaceSubAsset(assetPath, targetSubAsset.ToArray());
            Mesh[] meshes = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath).Select(obj=>(Mesh)obj).ToArray();

            skinnedRenderers = mainAsset.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < skinnedRenderers.Length; i++)
                skinnedRenderers[i].sharedMesh = meshes[i];

            meshFilters = mainAsset.GetComponentsInChildren<MeshFilter>();
            for (int i = 0; i < meshFilters.Length; i++)
                meshFilters[i].sharedMesh = meshes.Find(p => p.name == meshFilters[i].sharedMesh.name);
            PrefabUtility.SavePrefabAsset(mainAsset);
            GameObject.DestroyImmediate(prefabSource);
        }

        static Mesh GenerateMesh(Mesh _src,EVertexAttribute _generateUV)
        {
            Mesh target = _src.Copy();
            Vector3[] smoothNormals = GenerateSmoothNormals(target, ConvertToTangentSpace(_generateUV));
            target.SetVertexData(_generateUV,smoothNormals.ToList());
            return target;
        }
        public static bool ConvertToTangentSpace(EVertexAttribute _target)
        {
            switch(_target)
            {
                case EVertexAttribute.Normal:
                case EVertexAttribute.Tangent:
                    return false;
            }
            return true;
        }
    }
}