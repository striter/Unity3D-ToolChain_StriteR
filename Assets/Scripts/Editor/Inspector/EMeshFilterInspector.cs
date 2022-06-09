using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace TEditor
{
    [CustomEditor(typeof(MeshFilter)), CanEditMultipleObjects]
    public class MeshFilterInspector : Editor
    {
        MeshFilter m_Target;
        bool m_EnableVertexDataVisualize;
        bool m_DrawVertex = false;
        Color m_VertexColor = Color.white;

        bool m_DrawBiTangents = false;
        float m_BiTangentsLength = .5f;
        Color m_BiTangentsColor = Color.yellow;
        EColorVisualize m_DrawColorType;
        float m_ColorLength = .5f;

        readonly ValueChecker<EVertexData> m_ColorVertexDataType = new ValueChecker<EVertexData>(EVertexData.None);
        float m_VertexData = .5f;
        bool m_DrawDirection;

        readonly Color m_VectorVertexDataColor = Color.blue;

        readonly ValueChecker<Mesh> m_SharedMesh=new ValueChecker<Mesh>(null);
        private Vector3[] m_vertices;
        private Vector3[] m_Normals;
        private Vector4[] m_Tangents;
        private Color[] m_Colors;
        readonly List<Vector4> m_ColorVertexData = new List<Vector4>();

        public bool m_EnableMeshDataOutput;
        
        void OnEnable()
        {
            m_Target = target as MeshFilter;
            Setup();
        }
        void Setup()
        {
            if (m_SharedMesh.Check(m_Target.sharedMesh))
            {
                var sharedMesh = m_Target.sharedMesh;
                m_vertices = sharedMesh.vertices;
                m_Normals = sharedMesh.normals;
                m_Tangents = sharedMesh.tangents;
                m_Colors = sharedMesh.colors;
            }
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (m_SharedMesh.m_Value == null||!m_SharedMesh.m_Value.isReadable)
                return;
            
            m_EnableVertexDataVisualize=EditorGUILayout.BeginFoldoutHeaderGroup(m_EnableVertexDataVisualize, "Visualize");
            if(m_EnableVertexDataVisualize)
            {
                bool haveNormals =m_Normals.Length > 0;
                bool haveTangents = m_Tangents.Length > 0;
                bool haveColors = m_Colors.Length > 0;

                EditorGUILayout.BeginHorizontal();
                m_DrawVertex = EditorGUILayout.Toggle("Draw Vertex", m_DrawVertex);
                m_VertexColor = EditorGUILayout.ColorField(m_VertexColor);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (m_ColorVertexDataType.Check((EVertexData)EditorGUILayout.EnumPopup("Draw Vertex Data", m_ColorVertexDataType.m_Value)))
                {
                    m_ColorVertexData.Clear();
                    if (m_ColorVertexDataType.m_Value != EVertexData.None)
                        m_Target.sharedMesh.GetVertexData(m_ColorVertexDataType.m_Value, m_ColorVertexData);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (m_ColorVertexData.Count != 0)
                {
                    m_DrawDirection = EditorGUILayout.Toggle("Directional",m_DrawDirection);
                    m_VertexData = EditorGUILayout.Slider(m_VertexData, 0f, 2f);
                }
                else
                    EditorGUILayout.LabelField("No Vertex Data Found",EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.LabelField("Helpers",EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                if (haveNormals && haveTangents)
                {
                    m_DrawBiTangents = EditorGUILayout.Toggle("Draw Bi-Tangents", m_DrawBiTangents);
                    m_BiTangentsColor = EditorGUILayout.ColorField(m_BiTangentsColor);
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
                    m_DrawColorType = (EColorVisualize)EditorGUILayout.EnumPopup("Draw Color", m_DrawColorType);
                    m_ColorLength = EditorGUILayout.Slider(m_ColorLength, 0f, 2f);
                }
                else
                {
                    m_DrawColorType = EColorVisualize.None;
                    EditorGUILayout.LabelField("No Color Data");
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            m_EnableMeshDataOutput = EditorGUILayout.BeginFoldoutHeaderGroup(m_EnableMeshDataOutput, "Output");
            if (m_EnableMeshDataOutput)
            {
                if(GUILayout.Button("Output Mesh"))
                    OutputMesh();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
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
                    Handles.DrawLine(m_vertices[indices[startIndex]], m_vertices[indices[startIndex + 1]]);
                    Handles.DrawLine(m_vertices[indices[startIndex + 1]], m_vertices[indices[startIndex + 2]]);
                    Handles.DrawLine(m_vertices[indices[startIndex + 2]], m_vertices[indices[startIndex]]);
                }
            }

            for (int i = 0; i < m_vertices.Length; i++)
            {
                if (m_DrawBiTangents)
                {
                    Handles.color = Color.yellow;
                    Handles.DrawLine(m_vertices[i], m_vertices[i] + Vector3.Cross(m_Tangents[i],m_Normals[i]).normalized * m_BiTangentsLength);
                }

                if (m_ColorVertexData.Count != 0)
                {
                    if(m_DrawDirection)
                    {
                        Handles.color = m_VectorVertexDataColor;
                        Handles.DrawLine(m_vertices[i], m_vertices[i] + m_ColorVertexData[i].ToVector3() * m_VertexData);
                    }
                    else
                    {
                        Handles.color = m_ColorVertexData[i].ToColor().SetAlpha(1f);
                        Handles.DrawLine(m_vertices[i], m_vertices[i] + m_Normals[i] * m_VertexData);
                    }

                }

                if (m_DrawColorType != EColorVisualize.None)
                {
                    Color vertexColor = Color.clear;
                    Handles.color = m_DrawColorType.FilterColor(m_Colors[i]);
                    Handles.DrawLine(m_vertices[i], m_vertices[i] + m_Normals[i] * m_ColorLength);
                }
            }
        }

        private void OutputMesh()
        {
            if (!UEAsset.SaveFilePath(out string filePath, "asset", m_SharedMesh.m_Value.name))
                return;

            Mesh mesh = new Mesh();
            UEAsset.CopyMesh(m_SharedMesh,mesh);
            UEAsset.CreateOrReplaceMainAsset(mesh,UEPath.FileToAssetPath(filePath));
        }
    }
}