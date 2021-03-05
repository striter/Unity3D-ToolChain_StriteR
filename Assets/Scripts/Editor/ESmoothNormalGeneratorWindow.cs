using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TEditor
{
    public class ESmoothNormalGeneratorWindow : EditorWindow
    {
        [SerializeField] Mesh m_Mesh;
        SerializedObject m_SerializedWindow;
        SerializedProperty m_MeshProperty;

        bool m_GenerateToTangent;
        enum_Editor_MeshUV m_GenerateUV;
        private void OnEnable()
        {
            m_Mesh = null;
            m_SerializedWindow = new SerializedObject(this);
            m_MeshProperty = m_SerializedWindow.FindProperty(nameof(m_Mesh));
        }
        private void OnDisable()
        {
            m_SerializedWindow.Dispose();
            m_MeshProperty.Dispose();
        }
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.PropertyField(m_MeshProperty);
            m_SerializedWindow.ApplyModifiedProperties();
            m_GenerateToTangent = EditorGUILayout.Toggle("Generate As Tangent:",m_GenerateToTangent);
            if(!m_GenerateToTangent)
                m_GenerateUV=(enum_Editor_MeshUV)EditorGUILayout.EnumPopup("UV:",m_GenerateUV);
            if(m_Mesh!=null&&m_GenerateUV!= enum_Editor_MeshUV.None)
            {
                if(GUILayout.Button("Generate Smooth Normal"))
                {
                    if(TEditor.SaveFilePath(out string path,"asset", m_Mesh.name))
                    {
                        Mesh targetMesh = m_Mesh.Copy();
                        Vector3[] smoothNormals = GenerateSmoothNormals(targetMesh, !m_GenerateToTangent);
                        if (m_GenerateToTangent)
                            targetMesh.SetTangents(smoothNormals.ReconstructToArray(smoothNormal=>smoothNormal.ToVector4(1f)));
                        else
                            targetMesh.SetUVs((int)m_GenerateUV, smoothNormals);
                        TEditor.CreateOrReplaceMainAsset(targetMesh, TEditor.FilePathToAssetPath(path));
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }
        public static Vector3[] GenerateSmoothNormals(Mesh _srcMesh,bool _convertToTangentSpace)
        {
            var groups = _srcMesh.vertices.Select((vertex,index)=>new KeyValuePair<Vector3,int>(vertex,index)).GroupBy(pair=>pair.Key);
            Vector3[] normals = _srcMesh.normals;
            Vector3[] smoothNormals = normals.Copy();
            foreach(var group in groups)
            {
                if (group.Count() == 1)
                    continue;
                Vector3 smoothNormal = Vector3.zero;
                foreach(var index in group)
                    smoothNormal += normals[index.Value];
                smoothNormal = smoothNormal.normalized;
                foreach (var index in group)
                    smoothNormals[index.Value] = smoothNormal;
            }
            if (_convertToTangentSpace)
            {
                Vector4[] tangents = _srcMesh.tangents;
                for (int i = 0; i < smoothNormals.Length; i++)
                {
                    Vector3 tangent = tangents[i].ToVector3().normalized;
                    Vector3 normal = normals[i].normalized;
                    Vector3 biNormal = Vector3.Cross(normal, tangent).normalized * tangents[i].w;
                    Matrix3x3 tbnMatrix = Matrix3x3.identity;
                    tbnMatrix.SetRow(0, tangent);
                    tbnMatrix.SetRow(1, biNormal);
                    tbnMatrix.SetRow(2, normal);
                    smoothNormals[i] = tbnMatrix * smoothNormals[i].normalized;
                }
            }
            return smoothNormals;
        }

    }
}