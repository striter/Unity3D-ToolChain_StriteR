using UnityEditor;
using UnityEngine;
namespace TEditor
{
    public class EMeshVertexEditor : EditorWindow
    {
        Mesh m_SourceMesh;
        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            MeshEditor.End();
            m_SourceMesh = null;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            if (MeshModifingCheck())
                MeshSave();
            EditorGUILayout.EndVertical();
        }
        void OnSceneGUI(SceneView _sceneView)
        {
            MeshEditor.OnSceneGUI(_sceneView);
        }
        bool MeshModifingCheck()
        {
            if(!MeshEditor.m_Modifing)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Select A Mesh To Edit:", TEditor_GUIStyle.m_TitleLabel);
                m_SourceMesh = (Mesh)EditorGUILayout.ObjectField(m_SourceMesh, typeof(Mesh), false);
                GUILayout.EndHorizontal();
            }
            if (!m_SourceMesh)
                return false;

            if (!MeshEditor.m_Modifing)
            {
                if (GUILayout.Button("Begin Edit"))
                    MeshEditor.Begin(m_SourceMesh);
            }
            else
            {
                GUILayout.Label("Editing:" + m_SourceMesh.name,TEditor_GUIStyle.m_TitleLabel);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Reset"))
                    MeshEditor.Begin(m_SourceMesh);
                if (GUILayout.Button("Cancel"))
                    MeshEditor.End();
                GUILayout.EndHorizontal();
            }
            return MeshEditor.m_Modifing;
        }
        void MeshSave()
        {
            if (!GUILayout.Button("Save"))
                return;

            MeshEditor.Save();
        }

        public static class MeshEditor
        {
            static GameObject m_MeshObject;
            static MeshFilter m_MeshFilter;
            static MeshRenderer m_MeshRenderer;
            static Mesh m_ModifingMesh;
            static Vector3[] m_Normals;
            static Vector3[] m_Verticies;
            static Polygon[] m_Polygons;
            public static bool m_Modifing => m_ModifingMesh != null;
            public static void Begin(Mesh _srcMesh)
            {
                End();

                m_ModifingMesh = _srcMesh.Copy();
                m_Normals = m_ModifingMesh.normals;
                m_Polygons = m_ModifingMesh.GetPolygons(out m_Verticies,out int[] triangles);

                m_MeshObject = new GameObject("Modify Mesh");
                m_MeshObject.hideFlags = HideFlags.DontSave;
                m_MeshFilter = m_MeshObject.AddComponent<MeshFilter>();
                m_MeshRenderer = m_MeshObject.AddComponent<MeshRenderer>();
                m_MeshFilter.sharedMesh = m_ModifingMesh;
                m_MeshRenderer.material = new Material(Shader.Find("Game/Lit/Standard_Specular")) { hideFlags = HideFlags.HideAndDontSave };
            }
            public static void End()
            {
                if (m_MeshObject)
                    GameObject.DestroyImmediate(m_MeshObject);
                m_MeshFilter = null;
                m_MeshRenderer = null;
                m_MeshObject = null;
                m_ModifingMesh = null;
            }

            public static void OnSceneGUI(SceneView _sceneView)
            {
                if (!m_ModifingMesh)
                    return;

                Handles.matrix = m_MeshObject.transform.localToWorldMatrix;
                Handles.color = Color.green;
                Handles.DrawPolyLine(m_Polygons[0].m_Points.Add(m_Polygons[0].m_Point0));
                if (Event.current.type == EventType.MouseDown)
                {
                    Ray ray = _sceneView.camera.ScreenPointToRay(Event.current.mousePosition);

                }
                //for (int i = 0; i < 10; i++)
                //{
                //    Handles.SphereHandleCap(0, m_Verticies[i], Quaternion.identity, .2f, EventType.Repaint);
                //    Handles.DrawLine(m_Verticies[i], m_Verticies[i] + m_Normals[i] * 2f);
                //}
            }

            public static void Save()
            {
                if (!EUCommon.SaveFilePath(out string filePath, "asset", m_ModifingMesh.name))
                    return;

                EUCommon.CreateOrReplaceMainAsset(m_ModifingMesh, EUPath.FilePathToAssetPath(filePath));
            }
        }
    }

}