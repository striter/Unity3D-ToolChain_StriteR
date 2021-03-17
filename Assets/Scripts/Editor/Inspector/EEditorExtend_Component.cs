using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace TEditor
{
    using static UERender;
    #region Extend
    [CustomEditor(typeof(MeshFilter)), CanEditMultipleObjects]
    public class MeshFilterEditor : Editor
    {
        MeshFilter m_Target;
        bool m_EnableVertexDataVisualize;
        bool m_DrawVertex = true;
        Color m_VertexColor = Color.white;

        bool m_DrawBiTangents = false;
        float m_BiTangentsLength = .5f;
        Color m_BitangentColor = Color.yellow;
        enum_Editor_MeshColor m_DrawColorType;
        float m_ColorLength = .5f;

        ValueChecker<enum_VertexData> m_ColorVertexDataType = new ValueChecker<enum_VertexData>(enum_VertexData.None);
        float m_ColorVertexDataLength = .5f;

        ValueChecker<enum_VertexData> m_VectorVertexDataType = new ValueChecker<enum_VertexData>(enum_VertexData.None);
        float m_VectorVertexDataLength = .5f;
        Color m_VectorVertexDataColor = Color.blue;

        ValueChecker< Mesh> m_SharedMesh=new ValueChecker<Mesh>(null);
        Vector3[] m_Verticies;
        Vector3[] m_Normals;
        Vector4[] m_Tangents;
        Color[] m_Colors;
        List<Vector4> m_ColorVertexData = new List<Vector4>();
        List<Vector3> m_VectorVertexData = new List<Vector3>();
        void OnEnable()
        {
            m_Target = target as MeshFilter;
            Setup();
        }
        void Setup()
        {
            if (m_SharedMesh.Check(m_Target.sharedMesh))
            {
                m_Verticies = m_Target.sharedMesh.vertices;
                m_Normals = m_Target.sharedMesh.normals;
                m_Tangents = m_Target.sharedMesh.tangents;
                m_Colors = m_Target.sharedMesh.colors;
            }
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.BeginVertical();
            m_EnableVertexDataVisualize = EditorGUILayout.Foldout(m_EnableVertexDataVisualize, "Vertex Data Visualize");

            if (m_EnableVertexDataVisualize)
            {
                bool haveNormals =m_Normals.Length > 0;
                bool haveTangents = m_Tangents.Length > 0;
                bool haveColors = m_Colors.Length > 0;

                EditorGUILayout.BeginHorizontal();
                m_DrawVertex = EditorGUILayout.Toggle("Draw Vertex", m_DrawVertex);
                m_VertexColor = EditorGUILayout.ColorField(m_VertexColor);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (m_ColorVertexDataType.Check((enum_VertexData)EditorGUILayout.EnumPopup("Draw Vertex Data", m_ColorVertexDataType.m_Value)) && m_ColorVertexDataType.m_Value != enum_VertexData.None)
                    m_Target.sharedMesh.GetVertexData(m_ColorVertexDataType.m_Value, m_ColorVertexData);

                if (m_ColorVertexData.Count != 0)
                    m_ColorVertexDataLength = EditorGUILayout.Slider(m_ColorVertexDataLength, 0f, 2f);
                else
                    EditorGUILayout.LabelField("No Vertex Data");

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (m_VectorVertexDataType.Check((enum_VertexData)EditorGUILayout.EnumPopup("Draw Vertex Data", m_VectorVertexDataType.m_Value)) && m_VectorVertexDataType.m_Value != enum_VertexData.None)
                    m_Target.sharedMesh.GetVertexData(m_VectorVertexDataType.m_Value, m_VectorVertexData);
                m_VectorVertexDataColor = EditorGUILayout.ColorField(m_VectorVertexDataColor);
                m_VectorVertexDataLength = EditorGUILayout.Slider(m_VectorVertexDataLength, 0f, 2f);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (haveNormals && haveTangents)
                {
                    m_DrawBiTangents = EditorGUILayout.Toggle("Draw Bi-Tangents", m_DrawBiTangents);
                    m_BitangentColor = EditorGUILayout.ColorField(m_BitangentColor);
                    m_BiTangentsLength = EditorGUILayout.Slider(m_BiTangentsLength, 0f, 2f);
                }
                else
                {
                    m_DrawBiTangents = false;
                    EditorGUILayout.LabelField("Unable To Calculate Bi Tangents");
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (haveColors)
                {
                    m_DrawColorType = (enum_Editor_MeshColor)EditorGUILayout.EnumPopup("Draw Color", m_DrawColorType);
                    m_ColorLength = EditorGUILayout.Slider(m_ColorLength, 0f, 2f);
                }
                else
                {
                    m_DrawColorType = enum_Editor_MeshColor.None;
                    EditorGUILayout.LabelField("No Color Data");
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        private void OnSceneGUI()
        {
            if (!m_Target || !m_EnableVertexDataVisualize)
                return;
            Setup();
            Handles.matrix = m_Target.transform.localToWorldMatrix;

            int[] indices = m_Target.sharedMesh.GetIndices(0);
            if (m_DrawVertex)
            {
                Handles.color = m_VertexColor;
                int triangleCount = indices.Length / 3;
                for (int i = 0; i < triangleCount; i++)
                {
                    int startIndex = i * 3;
                    Handles.DrawLine(m_Verticies[indices[startIndex]], m_Verticies[indices[startIndex + 1]]);
                    Handles.DrawLine(m_Verticies[indices[startIndex + 1]], m_Verticies[indices[startIndex + 2]]);
                    Handles.DrawLine(m_Verticies[indices[startIndex + 2]], m_Verticies[indices[startIndex]]);
                }
            }

            for (int i = 0; i < m_Verticies.Length; i++)
            {
                if (m_DrawBiTangents)
                {
                    Handles.color = Color.yellow;
                    Handles.DrawLine(m_Verticies[i], m_Verticies[i] + Vector3.Cross(m_Tangents[i],m_Normals[i]).normalized * m_BiTangentsLength);
                }

                if (m_ColorVertexData.Count != 0)
                {
                    Handles.color = m_ColorVertexData[i].ToColor().SetAlpha(1f);
                    Handles.DrawLine(m_Verticies[i], m_Verticies[i] + m_Normals[i] * m_ColorVertexDataLength);
                }

                if(m_VectorVertexData.Count!=0)
                {
                    Handles.color = m_VectorVertexDataColor;
                    Handles.DrawLine(m_Verticies[i], m_Verticies[i] + m_VectorVertexData[i] * m_VectorVertexDataLength);
                }

                if (m_DrawColorType != enum_Editor_MeshColor.None)
                {
                    Color vertexColor = Color.clear;

                    switch (m_DrawColorType)
                    {
                        case enum_Editor_MeshColor.RGBA: vertexColor = m_Colors[i]; break;
                        case enum_Editor_MeshColor.R: vertexColor = Color.red * m_Colors[i].r; ; break;
                        case enum_Editor_MeshColor.G: vertexColor = Color.green * m_Colors[i].g; break;
                        case enum_Editor_MeshColor.B: vertexColor = Color.blue * m_Colors[i].b; break;
                        case enum_Editor_MeshColor.A: vertexColor = Color.white * m_Colors[i].a; break;
                    }
                    Handles.color = vertexColor;
                    Handles.DrawLine(m_Verticies[i], m_Verticies[i] + m_Normals[i] * m_ColorLength);
                }
            }
        }
    }
    #endregion
}