using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TEditor
{
    public class EMeshEditor : EditorWindow
    {
        public enum enum_EditorMode
        {
            Edit,
            Paint,
        }
        public ValueChecker<Mesh> m_SourceMesh { get; private set; } = new ValueChecker<Mesh>(null);
        public Mesh m_ModifingMesh { get; private set; }
        bool m_Debugging = false;
        public ValueChecker<bool> m_MaterialOverride { get; private set; } = new ValueChecker<bool>(false);
        public Material[] m_Materials { get; private set; }
        GameObject m_MeshObject;
        MeshFilter m_MeshFilter;
        MeshRenderer m_MeshRenderer;

        Dictionary<enum_EditorMode, MeshEditorHelperBase> m_EditorHelpers;
        MeshEditorHelperBase m_Helper => m_EditorHelpers.ContainsKey(m_EditorMode) ? m_EditorHelpers[m_EditorMode] : null;
        ValueChecker<enum_EditorMode> m_EditorMode = new ValueChecker<enum_EditorMode>();

        private void OnEnable()
        {
            m_MeshObject = new GameObject("Modify Mesh");
            m_MeshObject.hideFlags = HideFlags.HideAndDontSave;
            m_MeshFilter = m_MeshObject.AddComponent<MeshFilter>();
            m_MeshRenderer = m_MeshObject.AddComponent<MeshRenderer>();
            m_EditorHelpers = new Dictionary<enum_EditorMode, MeshEditorHelperBase>() { { enum_EditorMode.Edit, new MeshEditorHelper_Edit(this) }, { enum_EditorMode.Paint, new MeshEditorHelper_Paint(this) } };
            SceneView.duringSceneGui += OnSceneGUI;

            m_EditorMode.Bind(SwitchEditorMode);
            m_SourceMesh.Bind(SwitchSourceMesh);

            m_EditorMode.Set(enum_EditorMode.Edit);
        }
        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            m_EditorHelpers.Clear();
            m_SourceMesh.Check(null);
            End();

            if (m_MeshObject) GameObject.DestroyImmediate(m_MeshObject);
            m_MeshFilter = null;
            m_MeshRenderer = null;
            m_MeshObject = null;
        }
        void OnSceneGUI(SceneView _sceneView)
        {
            if (!m_ModifingMesh)
                return;

            OnKeyboradInteract();
            if (m_Debugging)
                m_Helper.OnEditorSceneGUIDebug(_sceneView, m_MeshObject);

            Handles.matrix = m_MeshObject.transform.localToWorldMatrix;
            m_Helper.OnEditorSceneGUI(_sceneView, m_MeshObject, this);
        }

        public void Begin()
        {
            End();
            m_ModifingMesh = m_SourceMesh.m_Value.Copy();
            m_MeshFilter.sharedMesh = m_ModifingMesh;
            SwitchMaterials(m_Materials);
            m_Helper.Begin();
        }
        public void End()
        {
            m_Helper?.End();
            m_ModifingMesh = null;
            m_MeshFilter.sharedMesh = null;
        }
        void SwitchMaterials(Material[] _materials)
        {
            if (_materials == null)
                _materials = new Material[] { m_Helper.GetDefaultMaterial() };
            m_Materials = _materials;
            m_MeshRenderer.sharedMaterials = _materials;
        }
        void SwitchSourceMesh(Mesh _srcMesh)
        {
            if (_srcMesh == null)
                return;
            Begin();
            SceneView targetView = SceneView.sceneViews[0] as SceneView;
            targetView.pivot = m_MeshObject.transform.localToWorldMatrix.MultiplyPoint(_srcMesh.bounds.GetPoint(Vector3.back + Vector3.up));
            targetView.rotation = Quaternion.LookRotation(m_MeshObject.transform.position - targetView.pivot);
        }
        void SwitchEditorMode(enum_EditorMode preVal, enum_EditorMode val)
        {
            if (m_EditorHelpers.ContainsKey(preVal))
                m_EditorHelpers[preVal].End();
            if (!m_MaterialOverride)
                SwitchMaterials(null);
            if (!m_ModifingMesh)
                return;
            m_Helper.Begin();
        }
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            if (!UEGUI.EditorApplicationPlayingCheck())
                return;
            EditorWindowGUI();
            EditorGUILayout.EndVertical();
        }
        void EditorWindowGUI()
        {
            if (!MeshModifingCheck())
                return;
            GUILayout.Label("Editing:" + m_SourceMesh.m_Value.name, UEGUIStyle_Window.m_TitleLabel);
            m_EditorMode.Check((enum_EditorMode)EditorGUILayout.EnumPopup("Edit Mode (~)", m_EditorMode));

            m_Debugging = GUILayout.Toggle(m_Debugging, "Collision Debug");
            if (m_MaterialOverride.Check(GUILayout.Toggle(m_MaterialOverride, "Material Override")))
                SwitchMaterials(null);
            if (m_MaterialOverride)
                SwitchMaterials(GUILayout_Extend.ArrayField(m_Materials));

            GUILayout.Label("Commands:", UEGUIStyle_Window.m_TitleLabel);

            m_Helper.OnEditorWindowGUI();

            if (GUILayout.Button("Save"))
                Save();
        }

        bool MeshModifingCheck()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Select A Mesh To Edit:", UEGUIStyle_Window.m_TitleLabel);
            m_SourceMesh.Check((Mesh)EditorGUILayout.ObjectField(m_SourceMesh, typeof(Mesh), false));
            GUILayout.EndHorizontal();
            if (!m_SourceMesh.m_Value)
                return false;

            if (!m_ModifingMesh)
            {
                if (GUILayout.Button("Begin Edit"))
                    Begin();
            }
            else
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Reset"))
                    Begin();
                if (GUILayout.Button("Cancel"))
                    End();
                GUILayout.EndHorizontal();
            }
            return m_ModifingMesh;
        }


        static bool m_RightClicking;
        static readonly List<KeyCode> s_RightClickIgnoreKeycodes = new List<KeyCode>() { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.LeftShift };
        void OnKeyboradInteract()
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
                m_RightClicking = true;
            else if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
                m_RightClicking = false;

            KeyCode _keyCode = Event.current.keyCode;
            if (Event.current.type != EventType.KeyDown || (m_RightClicking && s_RightClickIgnoreKeycodes.Contains(_keyCode)))
                return;

            switch (_keyCode)
            {
                default: m_Helper.OnKeyboradInteract(_keyCode); break;
                case KeyCode.BackQuote: m_EditorMode.Check(m_EditorMode.m_Value.Next()); break;
                case KeyCode.UpArrow: m_MeshObject.transform.Rotate(90f, 0, 0, Space.World); break;
                case KeyCode.DownArrow: m_MeshObject.transform.Rotate(-90f, 0, 0, Space.World); break;
                case KeyCode.LeftArrow: m_MeshObject.transform.Rotate(0, 90f, 0, Space.World); break;
                case KeyCode.RightArrow: m_MeshObject.transform.Rotate(0, -90f, 0, Space.World); break;
            }
            Repaint();
        }
        public void Save()
        {
            if (!UEAsset.SaveFilePath(out string filePath, "asset", m_ModifingMesh.name))
                return;

            UEAsset.CreateOrReplaceMainAsset(m_ModifingMesh, UEPath.FilePathToAssetPath(filePath));
        }
    }
    public class MeshEditorHelperBase
    {
        public EMeshEditor m_Parent { get; private set; }
        protected Mesh m_SourceMesh => m_Parent.m_SourceMesh;
        protected Mesh m_ModifingMesh => m_Parent.m_ModifingMesh;
        protected GMeshPolygon[] m_Polygons { get; private set; }
        public MeshEditorHelperBase(EMeshEditor _parent) { m_Parent = _parent; }
        public virtual void Begin()
        {
            m_Polygons = m_ModifingMesh.GetPolygons(out int[] triangles);
        }
        public virtual Material GetDefaultMaterial() => new Material(Shader.Find("Game/Lit/UberPBR")) { hideFlags = HideFlags.HideAndDontSave };
        public virtual void End() { }
        public virtual void OnEditorSceneGUI(SceneView _sceneView, GameObject _meshObject, EditorWindow _window) { }
        public virtual void OnEditorWindowGUI() { }
        public virtual void OnKeyboradInteract(KeyCode _keycode) { }
        static Ray mouseRay;
        static Vector3 collisionPoint;
        static GDirectedTriangle collisionTriangle;
        public virtual void OnEditorSceneGUIDebug(SceneView _sceneView, GameObject _meshObject)
        {
            Handles.color = Color.red;
            Handles_Extend.DrawArrow(mouseRay.origin, mouseRay.direction, .2f, .01f);
            Handles.DrawLine(mouseRay.origin, mouseRay.direction * 10f + mouseRay.origin);
            Handles.matrix = _meshObject.transform.localToWorldMatrix;
            Handles.SphereHandleCap(0, collisionPoint, Quaternion.identity, .05f, EventType.Repaint);
            Handles.DrawLines(collisionTriangle.triangle.GetDrawLinesVerticies());
        }

        protected static Ray ObjLocalSpaceRay(SceneView _sceneView, GameObject _meshObj)
        {
            GRay ray = _sceneView.camera.ScreenPointToRay(_sceneView.GetScreenPoint());
            mouseRay = ray;
            ray.origin = _meshObj.transform.worldToLocalMatrix.MultiplyPoint(ray.origin);
            ray.direction = _meshObj.transform.worldToLocalMatrix.MultiplyVector(ray.direction);
            return ray;
        }

        protected static int RayDirectedTriangleIntersect(GMeshPolygon[] _polygons, Vector3[] _verticies, GRay _ray, out Vector3 hitPoint, out GDirectedTriangle hitTriangle)
        {
            collisionPoint = Vector3.zero;
            float minDistance = float.MaxValue;
            int index = _polygons.LastIndex(p =>
            {
                GDirectedTriangle triangle = p.GetDirectedTriangle(_verticies);
                bool intersect = UGeometry.RayDirectedTriangleIntersect(triangle, _ray, true, true, out float distance);
                if (intersect && minDistance > distance)
                {
                    collisionTriangle = triangle;
                    collisionPoint = _ray.GetPoint(distance);
                    minDistance = distance;
                    return true;
                }
                return false;
            });
            hitPoint = collisionPoint;
            hitTriangle = collisionTriangle;
            return index;
        }

    }
    public class MeshEditorHelper_Edit : MeshEditorHelperBase
    {
        public MeshEditorHelper_Edit(EMeshEditor _parent) : base(_parent) { }
        enum enum_VertexEditMode
        {
            None,
            Position,
            Rotation,
        }
        int m_SelectedPolygon = -1;
        int m_SelectedVertexIndex = -1;

        float m_GUISize = 1f;
        bool m_EditSameVertex = true;
        const float C_VertexSphereRadius = .02f;
        readonly RangeFloat s_GUISizeRange = new RangeFloat(.005f, 4.995f);

        List<int> m_SubPolygons = new List<int>();
        bool m_SelectingPolygon => m_SelectedPolygon >= 0;
        bool m_SelectingVertex => m_SelectedVertexIndex != -1;
        ValueChecker<Vector3> m_PositionChecker = new ValueChecker<Vector3>().Set(Vector3.zero);
        ValueChecker<Quaternion> m_RotationChecker = new ValueChecker<Quaternion>().Set(Quaternion.identity);

        enum_VertexEditMode m_VertexEditMode;

        Vector3[] m_Verticies;
        ValueChecker<enum_VertexData> m_VertexDataSource = new ValueChecker<enum_VertexData>(enum_VertexData.Normal);
        List<Vector3> m_VertexDatas = new List<Vector3>();
        bool m_AvailableDatas => m_VertexDatas.Count > 0;
        bool m_EditingVectors => m_VertexDataSource != enum_VertexData.None && m_VertexDatas.Count > 0;
        public override void Begin()
        {
            base.Begin();
            m_Verticies = m_ModifingMesh.vertices;
            SelectVectorData(enum_VertexData.Normal);
            SelectVertex(0);
            SelectPolygon(0);
        }
        public override void End()
        {
            base.End();
            m_PositionChecker.Check(Vector3.zero);
            m_RotationChecker.Check(Quaternion.identity);
            m_VertexDataSource.Check(enum_VertexData.None);
            m_SelectedPolygon = -1;
            m_SelectedVertexIndex = -1;
            m_VertexEditMode = enum_VertexEditMode.Position;
            m_VertexDatas.Clear();
            m_SubPolygons.Clear();
            m_VertexDatas.Clear();
        }
        void SelectPolygon(int _index)
        {
            SelectVertex(-1);
            m_SelectedPolygon = _index;
            m_SubPolygons.Clear();
            if (_index < 0)
                return;
            GMeshPolygon mainPolygon = m_Polygons[m_SelectedPolygon];
            GTriangle mainTriangle = mainPolygon.GetTriangle(m_Verticies);
            m_Polygons.FindAllIndexes(m_SubPolygons, (index, polygon) => index != m_SelectedPolygon && polygon.GetTriangle(m_Verticies).verticies.Any(subVertex => mainTriangle.verticies.Any(mainVertex => mainVertex == subVertex)));
        }
        void SelectVertex(int _index)
        {
            m_SelectedVertexIndex = _index;
            if (_index < 0)
                return;
            m_PositionChecker.Check(m_Verticies[_index]);
            if (m_EditingVectors)
                m_RotationChecker.Check(Quaternion.LookRotation(m_VertexDatas[_index]));
        }
        void RecalculateBounds()
        {
            m_ModifingMesh.bounds = UBoundsChecker.GetBounds(m_Verticies);
        }
        void SelectVectorData(enum_VertexData _data)
        {
            if (!m_VertexDataSource.Check(_data))
                return;
            if (m_VertexDataSource != enum_VertexData.None)
                m_ModifingMesh.GetVertexData(m_VertexDataSource, m_VertexDatas);
        }
        public override void OnEditorSceneGUI(SceneView _sceneView, GameObject _meshObject, EditorWindow _window)
        {
            base.OnEditorWindowGUI();
            OnSceneInteract(_meshObject, _sceneView);
            OnDrawSceneHandles(_sceneView);
        }
        public void OnSceneInteract(GameObject _meshObject, SceneView _sceneView)
        {
            if (!m_AvailableDatas)
                return;

            if (OnVertexInteracting())
                return;

            if (!(Event.current.type == EventType.Used && Event.current.button == 0))
                return;

            m_SelectedVertexIndex = -1;
            Ray ray = ObjLocalSpaceRay(_sceneView, _meshObject);
            if (OnSelectVertexCheck(ray))
                return;
            SelectPolygon(RayDirectedTriangleIntersect(m_Polygons, m_Verticies, ray, out Vector3 _hitPoint, out GDirectedTriangle _hitTriangle));
        }
        void OnDrawSceneHandles(SceneView _sceneView)
        {
            Handles.color = Color.white.SetAlpha(.5f);
            Handles.DrawWireCube(m_ModifingMesh.bounds.center, m_ModifingMesh.bounds.size * 1.2f);

            if (!m_SelectingPolygon)
                return;

            GMeshPolygon _mainPolygon = m_Polygons[m_SelectedPolygon];

            foreach (var subPolygon in m_SubPolygons)
            {
                GDirectedTriangle directedTriangle = m_Polygons[subPolygon].GetDirectedTriangle(m_Verticies);
                if (Vector3.Dot(directedTriangle.normal, _sceneView.camera.transform.forward) > 0)
                    continue;
                Handles.color = Color.yellow.SetAlpha(.1f);
                Handles.DrawAAConvexPolygon(directedTriangle.triangle.verticies);
                Handles.color = Color.yellow;
                Handles.DrawLines(directedTriangle.triangle.GetDrawLinesVerticies());
            }
            GTriangle mainTriangle = _mainPolygon.GetTriangle(m_Verticies);
            Handles.color = Color.green.SetAlpha(.3f);
            Handles.DrawAAConvexPolygon(mainTriangle.verticies);
            Handles.color = Color.green;
            Handles.DrawLines(mainTriangle.GetDrawLinesVerticies());

            if (!m_EditingVectors)
                return;
            Handles.color = Color.green;
            foreach (var indice in _mainPolygon.indices)
            {
                Handles_Extend.DrawArrow(m_Verticies[indice], m_VertexDatas[indice], .1f * m_GUISize, .01f * m_GUISize);
                if (m_SelectedVertexIndex == indice)
                    continue;
                Handles_Extend.DrawWireSphere(m_Verticies[indice], m_VertexDatas[indice], C_VertexSphereRadius * m_GUISize);
            }
            Handles.color = Color.yellow;
            foreach (var subPolygon in m_SubPolygons)
            {
                foreach (var indice in m_Polygons[subPolygon].indices)
                    Handles.DrawLine(m_Verticies[indice], m_Verticies[indice] + m_VertexDatas[indice] * .03f * m_GUISize);
            }
        }
        bool OnVertexInteracting()
        {
            if (!m_SelectingVertex)
                return false;

            switch (m_VertexEditMode)
            {
                default: return false;
                case enum_VertexEditMode.Position:
                    {
                        if (m_PositionChecker.Check(Handles.PositionHandle(m_PositionChecker, m_EditingVectors ? Quaternion.LookRotation(m_VertexDatas[m_SelectedVertexIndex]) : Quaternion.identity)))
                        {
                            foreach (var index in GetModifingIndices(m_SelectedVertexIndex))
                                m_Verticies[index] = m_PositionChecker;
                            m_ModifingMesh.SetVertices(m_Verticies);
                            RecalculateBounds();
                        }
                    }
                    break;
                case enum_VertexEditMode.Rotation:
                    {
                        if (!m_EditingVectors)
                            return false;

                        if (m_RotationChecker.Check(Handles.RotationHandle(m_RotationChecker, m_Verticies[m_SelectedVertexIndex])))
                        {
                            foreach (var index in GetModifingIndices(m_SelectedVertexIndex))
                                m_VertexDatas[index] = m_RotationChecker.m_Value * Vector3.forward;
                            m_ModifingMesh.SetVertexData(m_VertexDataSource, m_VertexDatas);
                        }
                    }
                    break;
            }

            if (Event.current.button == 0 && Event.current.type == EventType.MouseDown)
            {
                SelectVertex(-1);
                return false;
            }
            return true;
        }

        List<int> GetModifingIndices(int _srcIndex)
        {
            List<int> modifingIndices = new List<int>();
            modifingIndices.Add(_srcIndex);
            if (m_EditSameVertex)
                modifingIndices.AddRange(m_Verticies.FindAllIndexes(p => p == m_Verticies[_srcIndex]));
            return modifingIndices;
        }

        bool OnSelectVertexCheck(Ray _ray)
        {
            if (!m_SelectingPolygon)
                return false;

            foreach (var indice in m_Polygons[m_SelectedPolygon].indices)
            {
                if (!UGeometry.RayBSIntersect(new GSphere( m_Verticies[indice], C_VertexSphereRadius * m_GUISize), _ray))
                    continue;
                SelectVertex(indice);
                return true;
            }
            return false;
        }
        void ResetVertex(int _index)
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
            m_SourceMesh.GetVertexData(m_VertexDataSource, vectors);
            foreach (var index in indices)
                m_VertexDatas[index] = vectors[index];
            m_ModifingMesh.SetVertexData(m_VertexDataSource, m_VertexDatas);
            m_RotationChecker.Check(Quaternion.LookRotation(m_VertexDatas[_index]));
        }
        public override void OnEditorWindowGUI()
        {
            base.OnEditorWindowGUI();
            SelectVectorData((enum_VertexData)EditorGUILayout.EnumPopup("Data Source", m_VertexDataSource));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Scene GUI Size (Z X):");
            m_GUISize = GUILayout.HorizontalSlider(m_GUISize, s_GUISizeRange.start, s_GUISizeRange.end);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Edit Same Vertex (Tab):");
            m_EditSameVertex = GUILayout.Toggle(m_EditSameVertex, "");
            GUILayout.EndHorizontal();
            m_VertexEditMode = (enum_VertexEditMode)EditorGUILayout.EnumPopup("Vertex Edit Mode (Q , E):", m_VertexEditMode);
        }
        public override void OnKeyboradInteract(KeyCode _keycode)
        {
            base.OnKeyboradInteract(_keycode);
            switch (_keycode)
            {
                case KeyCode.W:
                    m_VertexEditMode = enum_VertexEditMode.Position;
                    break;
                case KeyCode.E:
                    m_VertexEditMode = enum_VertexEditMode.Rotation;
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
                case KeyCode.Z: m_GUISize = Mathf.Clamp(m_GUISize - .1f, s_GUISizeRange.start, s_GUISizeRange.end); break;
                case KeyCode.X: m_GUISize = Mathf.Clamp(m_GUISize + .1f, s_GUISizeRange.start, s_GUISizeRange.end); break;
            }
        }
    }

    public class MeshEditorHelper_Paint : MeshEditorHelperBase
    {
        public enum enum_PaintMode
        {
            Const,
            Modify,
        }
        public enum enum_PaintNormal
        {
            Every,
            ViewNormal,
            TriangleNormal,
        }
        public enum enum_PaintColor
        {
            R = 1,
            G = 2,
            B = 3,
            A = 4,
        }
        Vector3[] m_Verticies;
        Vector3[] m_Normals;
        enum_PaintMode m_PaintMode = enum_PaintMode.Const;
        ValueChecker<enum_PaintColor> m_PaintColor = new ValueChecker<enum_PaintColor>(enum_PaintColor.R);

        float m_PaintSize = 1f;
        float m_PaintValue = .5f;
        static readonly RangeFloat s_PaintSizeRange = new RangeFloat(.001f, 2f);
        Vector3 m_PaintPosition;
        List<int> m_PaintAffectedIndices = new List<int>();
        enum_PaintNormal m_PaintNormal = enum_PaintNormal.TriangleNormal;
        ValueChecker<enum_VertexData> m_VertexDataSource = new ValueChecker<enum_VertexData>(enum_VertexData.Color);
        List<Vector4> m_VertexDatas = new List<Vector4>();
        bool m_AvailableDatas => m_VertexDatas.Count > 0;
        public override Material GetDefaultMaterial() => new Material(Shader.Find("Hidden/VertexColorVisualize")) { hideFlags = HideFlags.HideAndDontSave };
        static readonly string[] KW_Sample = new string[] { "_SAMPLE_UV0", "_SAMPLE_UV1", "_SAMPLE_UV2", "_SAMPLE_UV3", "_SAMPLE_UV4", "_SAMPLE_UV5", "_SAMPLE_UV6", "_SAMPLE_UV7", "_SAMPLE_COLOR", "_SAMPLE_NORMAL", "_SAMPLE_TANGENT" };
        static readonly string[] KW_Color = new string[] { "_VISUALIZE_R", "_VISUALIZE_G", "_VISUALIZE_B", "_VISUALIZE_A" };
        public MeshEditorHelper_Paint(EMeshEditor _parent) : base(_parent)
        {
            m_PaintColor.Bind(value => {
                if (!m_Parent.m_MaterialOverride)
                    m_Parent.m_Materials[0].EnableKeywords(KW_Color, (int)value);
            });
            m_VertexDataSource.Bind(value =>
            {
                m_VertexDatas.Clear();
                if (value != enum_VertexData.None)
                {
                    m_ModifingMesh.GetVertexData(value, m_VertexDatas);
                    if (!m_Parent.m_MaterialOverride)
                        m_Parent.m_Materials[0].EnableKeywords(KW_Sample, (int)m_VertexDataSource.m_Value - 1);
                }
            });
        }
        public override void Begin()
        {
            base.Begin();
            m_Verticies = m_ModifingMesh.vertices;
            m_Normals = m_ModifingMesh.normals;
            m_PaintColor.Set(m_PaintColor);
            m_VertexDataSource.Set(m_VertexDataSource);
        }
        public override void End()
        {
            base.End();
            m_VertexDatas.Clear();
        }
        public override void OnEditorSceneGUI(SceneView _sceneView, GameObject _meshObject, EditorWindow _window)
        {
            base.OnEditorSceneGUI(_sceneView, _meshObject, _window);
            if (!m_AvailableDatas)
                return;
            OnInteractGUI(_sceneView, _meshObject);
            OnDrawHandles();
        }
        void OnDataChange() => m_ModifingMesh.SetVertexData(m_VertexDataSource, m_VertexDatas);
        void OnInteractGUI(SceneView _sceneView, GameObject _meshObject)
        {
            Handles.color = GetPaintColor(m_PaintColor);
            if (m_PaintPosition != Vector3.zero)
                Handles_Extend.DrawWireSphere(m_PaintPosition, Quaternion.identity, m_PaintSize);

            if (Event.current.type == EventType.MouseMove)
            {
                Vector3 cameraLocal = _meshObject.transform.worldToLocalMatrix.MultiplyPoint(_sceneView.camera.transform.position);
                if (RayDirectedTriangleIntersect(m_Polygons, m_ModifingMesh.vertices, ObjLocalSpaceRay(_sceneView, _meshObject), out Vector3 hitPosition, out GDirectedTriangle hitTriangle) != -1)
                {
                    m_PaintPosition = hitPosition;
                    m_PaintAffectedIndices.Clear();
                    float sqrRaidus = m_PaintSize * m_PaintSize;
                    m_Verticies.FindAllIndexes(m_PaintAffectedIndices, (index, p) => {
                        bool normalPassed = false;
                        switch (m_PaintNormal)
                        {
                            case enum_PaintNormal.ViewNormal: normalPassed = Vector3.Dot(m_Normals[index], p - cameraLocal) < 0; break;
                            case enum_PaintNormal.TriangleNormal: normalPassed = Vector3.Dot(m_Normals[index], hitTriangle.normal) > 0; break;
                            case enum_PaintNormal.Every: normalPassed = true; break;
                        }
                        return normalPassed && (hitPosition - p).sqrMagnitude < sqrRaidus;
                    });
                }
            }

            if (!m_AvailableDatas)
                return;
            if (Event.current.type == EventType.MouseDown)
            {
                int button = Event.current.button;
                if (button != 0 && button != 2)
                    return;
                switch (m_PaintMode)
                {
                    default: throw new Exception("Invalid Type:" + m_PaintMode);
                    case enum_PaintMode.Const:
                        m_PaintAffectedIndices.Traversal(index => m_VertexDatas[index] = ApplyModify(m_VertexDatas[index], m_PaintValue, m_PaintMode, m_PaintColor));
                        break;
                    case enum_PaintMode.Modify:
                        float value = button == 0 ? m_PaintValue : -m_PaintValue;
                        m_PaintAffectedIndices.Traversal(index => m_VertexDatas[index] = ApplyModify(m_VertexDatas[index], value, m_PaintMode, m_PaintColor));
                        break;
                }
                OnDataChange();
            }
        }
        Vector4 ApplyModify(Vector4 _src, float _value, enum_PaintMode _paintMode, enum_PaintColor _targetColor)
        {
            switch (_paintMode)
            {
                default: throw new Exception("Invalid Type:" + _paintMode);
                case enum_PaintMode.Const:
                    switch (_targetColor)
                    {
                        default: throw new Exception("Invalid Target:" + _targetColor);
                        case enum_PaintColor.R: return new Vector4(_value, _src.y, _src.z, _src.w);
                        case enum_PaintColor.G: return new Vector4(_src.x, _value, _src.z, _src.w);
                        case enum_PaintColor.B: return new Vector4(_src.x, _src.y, _value, _src.w);
                        case enum_PaintColor.A: return new Vector4(_src.x, _src.y, _src.z, _value);
                    }
                case enum_PaintMode.Modify:
                    switch (_targetColor)
                    {
                        default: throw new Exception("Invalid Target:" + _targetColor);
                        case enum_PaintColor.R: return new Vector4(Mathf.Clamp(_src.x, 0, 1) + _value, _src.y, _src.z, _src.w);
                        case enum_PaintColor.G: return new Vector4(_src.x, Mathf.Clamp(_src.y + _value, 0, 1), _src.z, _src.w);
                        case enum_PaintColor.B: return new Vector4(_src.x, _src.y, Mathf.Clamp(_src.z + _value, 0, 1), _src.w);
                        case enum_PaintColor.A: return new Vector4(_src.x, _src.y, _src.z, Mathf.Clamp(_src.w + _value, 0, 1));
                    }
            }
        }
        void OnDrawHandles()
        {
            Handles.color = Color.magenta;
            foreach (var indice in m_PaintAffectedIndices)
            {
                Vector4 targetcolor = m_VertexDatas[indice];
                float colorvalue = 0;
                switch ((enum_PaintColor)m_PaintColor)
                {
                    case enum_PaintColor.R: colorvalue = targetcolor.x; break;
                    case enum_PaintColor.G: colorvalue = targetcolor.y; break;
                    case enum_PaintColor.B: colorvalue = targetcolor.z; break;
                    case enum_PaintColor.A: colorvalue = targetcolor.w; break;
                }
                Handles.color = GetPaintColor(m_PaintColor) * colorvalue;
                Handles.DrawLine(m_Verticies[indice], m_Verticies[indice] + m_Normals[indice] * .5f * m_PaintSize);
            }
        }
        public override void OnEditorWindowGUI()
        {
            base.OnEditorWindowGUI();
            m_VertexDataSource.Check((enum_VertexData)EditorGUILayout.EnumPopup("Data Source", m_VertexDataSource));
            if (!m_AvailableDatas)
            {
                EditorGUILayout.LabelField("<Color=#FF0000>Empty Vertex Data</Color>", UEGUIStyle_Window.m_ErrorLabel);
                if (GUILayout.Button("Fill With Empty Colors"))
                {
                    for (int i = 0; i < m_Verticies.Length; i++)
                        m_VertexDatas.Add(Vector4.zero);
                    m_ModifingMesh.SetVertexData(m_VertexDataSource, m_VertexDatas);
                }
                return;
            }
            m_PaintColor.Check((enum_PaintColor)EditorGUILayout.EnumPopup("Color (LCtrl)", m_PaintColor));
            m_PaintMode = (enum_PaintMode)EditorGUILayout.EnumPopup("Mode (Tab)", m_PaintMode);
            m_PaintNormal = (enum_PaintNormal)EditorGUILayout.EnumPopup("Normal (Capslock)", m_PaintNormal);
            m_PaintSize = EditorGUILayout.Slider("Size (Z X)", m_PaintSize, s_PaintSizeRange.start, s_PaintSizeRange.end);
            m_PaintValue = EditorGUILayout.Slider("Value (Q E)", m_PaintValue, 0f, 1f);
        }
        public override void OnKeyboradInteract(KeyCode _keycode)
        {
            base.OnKeyboradInteract(_keycode);
            switch (_keycode)
            {
                case KeyCode.R: ResetSelected(); break;
                case KeyCode.Tab: m_PaintMode = m_PaintMode.Next(); break;
                case KeyCode.LeftControl: m_PaintColor.Check(m_PaintColor.m_Value.Next()); break;
                case KeyCode.CapsLock: m_PaintNormal = m_PaintNormal.Next(); break;
                case KeyCode.Q: m_PaintValue = Mathf.Clamp(m_PaintValue - .1f, 0, 1); break;
                case KeyCode.E: m_PaintValue = Mathf.Clamp(m_PaintValue + .1f, 0, 1); break;
                case KeyCode.Z: m_PaintSize = Mathf.Clamp(m_PaintSize - .1f, s_PaintSizeRange.start, s_PaintSizeRange.end); break;
                case KeyCode.X: m_PaintSize = Mathf.Clamp(m_PaintSize + .1f, s_PaintSizeRange.start, s_PaintSizeRange.end); break;
            }
        }
        void ResetSelected()
        {
            List<Vector4> originDatas = new List<Vector4>();
            m_SourceMesh.GetVertexData(m_VertexDataSource, originDatas);
            if (originDatas.Count == 0)
                return;
            m_PaintAffectedIndices.Traversal(index => m_VertexDatas[index] = originDatas[index]);
            OnDataChange();
        }
        public static Color GetPaintColor(enum_PaintColor _color)
        {
            switch (_color)
            {
                default: return Color.magenta;
                case enum_PaintColor.R: return Color.red;
                case enum_PaintColor.G: return Color.green;
                case enum_PaintColor.B: return Color.blue;
                case enum_PaintColor.A: return Color.cyan;
            }
        }
    }
}