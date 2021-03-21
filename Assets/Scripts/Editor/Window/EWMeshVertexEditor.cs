using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace TEditor
{
    public class EWMeshVertexEditor : EditorWindow
    {
        Mesh m_SourceMesh;
        bool m_Debugging=false;
        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            MeshEditor.End();
            m_SourceMesh = null;
            m_Debugging = false;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            OnEditorGUI();
            EditorGUILayout.EndVertical();
        }

        void OnEditorGUI()
        {
            if (!UEGUI.EditorApplicationPlayingCheck())
                return;

            if (MeshModifingCheck())
                MeshEditor.OnGUI();
        }
        void OnSceneGUI(SceneView _sceneView)=>MeshEditor.OnSceneGUI(_sceneView,this, m_Debugging);
        bool MeshModifingCheck()
        {
            if(!MeshEditor.m_Modifing)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Select A Mesh To Edit:", UEGUIStyle_Window.m_TitleLabel);
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
                GUILayout.Label("Editing:" + m_SourceMesh.name, UEGUIStyle_Window.m_TitleLabel);
                m_Debugging = GUILayout.Toggle(m_Debugging, "Debug");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Reset"))
                    MeshEditor.Begin(m_SourceMesh);
                if (GUILayout.Button("Cancel"))
                    MeshEditor.End();
                GUILayout.EndHorizontal();
            }
            return MeshEditor.m_Modifing;
        }

        public static class MeshEditor
        {
            enum enum_VertexEditMode
            {
                None,
                Position,
                Rotation,
            }
            static GameObject m_MeshObject;
            static MeshFilter m_MeshFilter;
            static MeshRenderer m_MeshRenderer;
            static Mesh m_ModifingMesh;
            static Mesh m_SourceMesh;

            static ValueChecker<enum_VertexData> m_VectorVertexTarget=new ValueChecker<enum_VertexData>( enum_VertexData.Normal);
            static List<Vector3> m_VectorDatas = new List<Vector3>();
            public static bool m_EditingVectors => m_VectorVertexTarget.m_Value != enum_VertexData.None&&m_VectorDatas.Count > 0;
            static Vector3[] m_Verticies;
            static MeshPolygon[] m_Polygons;
            public static bool m_Modifing => m_ModifingMesh != null;
            public static int m_SelectedPolygon = -1;
            public static List<int> m_SubPolygons = new List<int>();
            public static bool m_SelectingPolygon => m_SelectedPolygon >= 0;
            public static int m_SelectedVertexIndex = -1;
            public static bool m_SelectingVertex => m_SelectedVertexIndex != -1;
            static ValueChecker<Vector3> m_PositionChecker = new ValueChecker<Vector3>(Vector3.zero);
            static ValueChecker<Quaternion> m_RotationChecker = new ValueChecker<Quaternion>(Quaternion.identity);
            static ValueChecker< Material[]> m_Materials = new ValueChecker<Material[]>(new Material[] { new Material(Shader.Find("Game/Lit/Standard_Specular")) { hideFlags = HideFlags.HideAndDontSave } });

            static Ray mouseRay;
            static Vector3 collisionPoint;

            static float m_GUISize = 1f;
            static bool m_EditSameVertex=true;
            static enum_VertexEditMode m_VertexEditMode;
            const float C_VertexSphereRadius = .02f;
            static readonly RangeFloat GUISizeRange = new RangeFloat(.005f, 4.995f);

            public static void Begin(Mesh _srcMesh)
            {
                End();
                m_SourceMesh = _srcMesh;
                m_ModifingMesh = m_SourceMesh.Copy();
                m_Polygons = m_ModifingMesh.GetPolygons(out int[] triangles);
                m_Verticies = m_ModifingMesh.vertices;
                m_MeshObject = new GameObject("Modify Mesh");
                m_MeshObject.hideFlags = HideFlags.HideAndDontSave;
                m_MeshFilter = m_MeshObject.AddComponent<MeshFilter>();
                m_MeshRenderer = m_MeshObject.AddComponent<MeshRenderer>();
                m_MeshFilter.sharedMesh = m_ModifingMesh;
                m_MeshRenderer.sharedMaterials = m_Materials.m_Value;

                SelectVectorData(enum_VertexData.Normal);
                SelectVertex(0);
                SelectPolygon(0);
            }
            public static void End()
            {
                m_PositionChecker.Check(Vector3.zero);
                m_RotationChecker.Check(Quaternion.identity);
                m_VectorVertexTarget.Check(enum_VertexData.None);
                if (m_MeshObject)
                    GameObject.DestroyImmediate(m_MeshObject);
                m_MeshFilter = null;
                m_MeshRenderer = null;
                m_MeshObject = null;
                m_ModifingMesh = null;
                m_SelectedPolygon = -1;
                m_SelectedVertexIndex = -1;
                m_VertexEditMode = enum_VertexEditMode.Position;
                m_SubPolygons.Clear();
            }
            static void SelectPolygon(int _index)
            {
                SelectVertex(-1);
                m_SelectedPolygon = _index;
                m_SubPolygons.Clear();
                if (_index < 0)
                    return;
                MeshPolygon mainPolygon = m_Polygons[m_SelectedPolygon];
                Triangle mainTriangle = mainPolygon.GetTriangle(m_Verticies);
                m_Polygons.FindAllIndexes(m_SubPolygons, (index, polygon) => index!=m_SelectedPolygon&&polygon.GetTriangle(m_Verticies).m_Verticies.Any(subVertex => mainTriangle.m_Verticies.Any(mainVertex=>mainVertex==subVertex)));
            }
            static void SelectVertex(int _index)
            {
                m_SelectedVertexIndex = _index;
                if(_index<0)
                    return;
                m_PositionChecker.Check(m_Verticies[_index]);
                if(m_EditingVectors)
                    m_RotationChecker.Check(Quaternion.LookRotation(m_VectorDatas[_index]));
            }
            static void RecalculateBounds()
            {
                m_ModifingMesh.bounds = UBoundsChecker.GetBounds(m_Verticies);
            }
            static void SelectVectorData(enum_VertexData _data)
            {
                if (!m_VectorVertexTarget.Check(_data))
                    return;
                if (m_VectorVertexTarget.m_Value == enum_VertexData.None)
                    m_VectorDatas.Clear();
                else
                    m_ModifingMesh.GetVertexData(m_VectorVertexTarget.m_Value, m_VectorDatas);
            }
            public static void OnSceneGUI(SceneView _sceneView,EditorWindow _window,bool _debug)
            {
                if (!m_ModifingMesh)
                    return;
                OnSceneInteract(_sceneView);
                OnKeyboradInteract(_window);
                OnDrawSceneHandles(_sceneView);
                if (_debug)
                    OnDrawSceneGUIDebug();
            }
            #region Interact
            static bool m_RightClicking;
            public static void OnKeyboradInteract(EditorWindow _window)
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
                    m_RightClicking = true;
                else if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
                    m_RightClicking = false;

                if (Event.current.type != EventType.KeyDown)
                    return;
                switch (Event.current.keyCode)
                {
                    case KeyCode.W:
                        {
                            if (!m_RightClicking)
                                m_VertexEditMode = enum_VertexEditMode.Position;
                        }
                        break;
                    case KeyCode.E:
                        {
                            if (!m_RightClicking)
                                m_VertexEditMode = enum_VertexEditMode.Rotation;
                        }
                        break;
                    case KeyCode.R:
                        ResetVertex(m_SelectedVertexIndex);
                        break;
                    case KeyCode.Tab:
                        m_EditSameVertex = !m_EditSameVertex;
                        break;
                    case KeyCode.Escape:
                        {
                            if (m_SelectingVertex)
                                SelectVertex(-1);
                            else if (m_SelectingPolygon)
                                SelectPolygon(-1);
                        }
                        break;
                    case KeyCode.Minus: m_GUISize = Mathf.Clamp(m_GUISize - .1f, GUISizeRange.start, GUISizeRange.end);break;
                    case KeyCode.Equals: m_GUISize = Mathf.Clamp(m_GUISize + .1f, GUISizeRange.start, GUISizeRange.end);break;
                    case KeyCode.UpArrow:m_MeshObject.transform.Rotate(90f, 0, 0, Space.World); break;
                    case KeyCode.DownArrow: m_MeshObject.transform.Rotate(-90f, 0, 0, Space.World); break;
                    case KeyCode.LeftArrow: m_MeshObject.transform.Rotate(0, 90f, 0, Space.World); break;
                    case KeyCode.RightArrow: m_MeshObject.transform.Rotate(0, -90f, 0, Space.World); break;
                }
                _window.Repaint();
            }
            public static void OnGUI()
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Scene GUI Size (- , +):");
                m_GUISize = GUILayout.HorizontalSlider(m_GUISize, GUISizeRange.start, GUISizeRange.end);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Edit Same Vertex (Tab):");
                m_EditSameVertex = GUILayout.Toggle(m_EditSameVertex,"");
                GUILayout.EndHorizontal();
                m_VertexEditMode = (enum_VertexEditMode)EditorGUILayout.EnumPopup("Vertex Edit Mode (Q , E):",m_VertexEditMode);
                SelectVectorData((enum_VertexData)EditorGUILayout.EnumPopup("Vector Edit Target", m_VectorVertexTarget.m_Value));
                if (m_Materials.Check(UEGUI.Layout.ArrayField(m_Materials.m_Value, "Materials", false)))
                    m_MeshRenderer.sharedMaterials = m_Materials.m_Value;
                if (GUILayout.Button("Save"))
                    Save();
            }
            #endregion
            #region SceneGUI
            static void OnDrawSceneHandles(SceneView _sceneView)
            {
                Handles.matrix = m_MeshObject.transform.localToWorldMatrix;
                Handles.color = Color.white.SetAlpha(.5f);
                Handles.Label(m_ModifingMesh.bounds.GetPoint(Vector3.up * .5f), "Vertex Editing", UEGUIStyle_SceneView.m_TitleLabel);
                Handles.DrawWireCube(m_ModifingMesh.bounds.center, m_ModifingMesh.bounds.size*1.2f);

                if (!m_SelectingPolygon)
                    return;

                MeshPolygon _mainPolygon = m_Polygons[m_SelectedPolygon];

                foreach (var subPolygon in m_SubPolygons)
                {
                    DirectedTriangle directedTriangle= m_Polygons[subPolygon].GetDirectedTriangle(m_Verticies);
                    if (Vector3.Dot(directedTriangle.m_Normal, _sceneView.camera.transform.forward) > 0)
                        continue;
                    Handles.color = Color.yellow.SetAlpha(.1f);
                    Handles.DrawAAConvexPolygon(directedTriangle.m_Triangle.m_Verticies);
                    Handles.color = Color.yellow;
                    Handles.DrawLines(directedTriangle.m_Triangle.GetDrawLinesVerticies());
                }
                Triangle mainTriangle = _mainPolygon.GetTriangle(m_Verticies);
                Handles.color = Color.green.SetAlpha(.3f);
                Handles.DrawAAConvexPolygon(mainTriangle.m_Verticies);
                Handles.color = Color.green;
                Handles.DrawLines(mainTriangle.GetDrawLinesVerticies());

                if (!m_EditingVectors)
                    return;
                Handles.color = Color.green;
                foreach(var indice in _mainPolygon.m_Indices)
                {
                    Handles_Extend.DrawArrow(m_Verticies[indice], m_VectorDatas[indice], .1f*m_GUISize, .01f * m_GUISize);
                    if (m_SelectedVertexIndex == indice)
                        continue;
                    Handles_Extend.DrawWireSphere(m_Verticies[indice], m_VectorDatas[indice], C_VertexSphereRadius * m_GUISize);
                }
                Handles.color = Color.yellow;
                foreach (var subPolygon in m_SubPolygons)
                {
                    foreach (var indice in m_Polygons[subPolygon].m_Indices)
                        Handles.DrawLine(m_Verticies[indice], m_Verticies[indice] + m_VectorDatas[indice] * .03f * m_GUISize);
                }
            }
            static void OnDrawSceneGUIDebug()
            {
                Handles.color = Color.red;
                Handles_Extend.DrawArrow(mouseRay.origin, mouseRay.direction, .2f * m_GUISize, .01f);
                Handles.DrawLine(mouseRay.origin, mouseRay.direction * 10f + mouseRay.origin);
                Handles.matrix = m_MeshObject.transform.localToWorldMatrix;
                Handles.SphereHandleCap(0, collisionPoint, Quaternion.identity, .05f, EventType.Repaint);
            }
            #endregion
            #region Interact
            static void OnSceneInteract(SceneView _sceneView)
            {
                Handles.matrix = m_MeshObject.transform.localToWorldMatrix;
                if (OnVertexInteract())
                    return;

                if (!(Event.current.type == EventType.Used && Event.current.button == 0))
                    return;

                EditorUtility.SetDirty(_sceneView);
                Ray ray = _sceneView.camera.ScreenPointToRay(_sceneView.GetScreenPoint());
                mouseRay = ray;
                ray.origin = m_MeshObject.transform.worldToLocalMatrix.MultiplyPoint(ray.origin);
                ray.direction = m_MeshObject.transform.worldToLocalMatrix.MultiplyVector(ray.direction);

                if (OnSelectVertexCheck(ray))
                    return;

                float minDistance = float.MaxValue;
                SelectPolygon(m_Polygons.LastIndex(p =>
                {
                    bool intersect = UBoundingCollision.RayDirectedTriangleIntersect(p.GetDirectedTriangle(m_Verticies), ray, true, true, out float distance);
                    if (intersect && minDistance > distance)
                    {
                        collisionPoint = ray.GetPoint(distance);
                        minDistance = distance;
                        return true;
                    }
                    return false;
                }));
            }

            static bool OnVertexInteract()
            {
                if (!m_SelectingVertex)
                    return false;

                switch (m_VertexEditMode)
                {
                    default:return false;
                    case enum_VertexEditMode.Position:
                        {
                            if(m_PositionChecker.Check( Handles.PositionHandle(m_PositionChecker.m_Value,m_EditingVectors?  Quaternion.LookRotation(m_VectorDatas[m_SelectedVertexIndex]):Quaternion.identity)))
                            {
                                foreach (var index in GetModifingIndices(m_SelectedVertexIndex))
                                    m_Verticies[index] = m_PositionChecker.m_Value;
                                m_ModifingMesh.SetVertices(m_Verticies);
                                RecalculateBounds();
                            }
                        }
                        break;
                    case enum_VertexEditMode.Rotation:
                        {
                            if (!m_EditingVectors)
                                return false;

                            if (m_RotationChecker.Check(Handles.RotationHandle(m_RotationChecker.m_Value, m_Verticies[m_SelectedVertexIndex])))
                            {
                                foreach (var index in GetModifingIndices(m_SelectedVertexIndex))
                                    m_VectorDatas[index] = m_RotationChecker.m_Value*Vector3.forward;
                                m_ModifingMesh.SetVertexData(m_VectorVertexTarget.m_Value,m_VectorDatas);
                            }
                        }
                        break;
                }
                return true;
            }

            static List<int> GetModifingIndices(int _srcIndex)
            {
                List<int> modifingIndices = new List<int>();
                modifingIndices.Add(_srcIndex);
                if (m_EditSameVertex)
                    modifingIndices.AddRange(m_Verticies.FindAllIndexes(p => p == m_Verticies[_srcIndex]));
                return modifingIndices;
            }

            static bool OnSelectVertexCheck(Ray _ray)
            {
                if (!m_SelectingPolygon)
                    return false;

                foreach (var indice in m_Polygons[m_SelectedPolygon].m_Indices)
                {
                    if (!UBoundingCollision.RayBSIntersect(m_Verticies[indice], C_VertexSphereRadius * m_GUISize, _ray))
                        continue;
                    SelectVertex(indice);
                    return true;
                }
                return false;
            }
            #endregion
            static void ResetVertex(int _index)
            {
                if (_index < 0)
                    return;
                Vector3[] verticies = m_SourceMesh.vertices;
                List<int> indices = GetModifingIndices(_index);
                foreach (var index in indices)
                    m_Verticies[index] = verticies[index];
                m_PositionChecker.Check(m_Verticies[_index]);
                m_ModifingMesh.SetVertices(m_Verticies);
                RecalculateBounds();

                if (!m_EditingVectors)
                    return;
                List<Vector3> vectors = new List<Vector3>();
                m_SourceMesh.GetVertexData(m_VectorVertexTarget.m_Value, vectors);
                foreach (var index in indices)
                    m_VectorDatas[index] = vectors[index];
                m_ModifingMesh.SetVertexData(m_VectorVertexTarget.m_Value,m_VectorDatas);
                m_RotationChecker.Check(Quaternion.LookRotation(m_VectorDatas[_index]));
            }
            public static void Save()
            {
                if (!UECommon.SaveFilePath(out string filePath, "asset", m_ModifingMesh.name))
                    return;

                UECommon.CreateOrReplaceMainAsset(m_ModifingMesh, UEPath.FilePathToAssetPath(filePath));
            }
        }
    }

}