using System;
using System.Collections;
using System.Collections.Generic;
using Geometry.Three;
using GridTest;
using ObjectPool;
using ObjectPoolStatic;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;
using TTouchTracker;
using UnityEngine;
namespace ConvexGrid
{
    public partial class ConvexGrid
    {
        private TObjectPoolClass<ConvexGridRenderer> m_AreaMeshes;
        private class ConvexGridRenderer : ITransform,IPoolCallback
        {
            public Transform iTransform { get; }
            public MeshFilter m_MeshFilter { get; }

            public ConvexGridRenderer(Transform _transform)
            {
                iTransform = _transform;
                m_MeshFilter = _transform.GetComponent<MeshFilter>();
            }
            public void OnPoolInit(Action<int> _DoRecycle) { }

            public void OnPoolSpawn(int identity)
            {
                
            }

            public void OnPoolRecycle()
            {
                DestroyImmediate(m_MeshFilter.sharedMesh);
                m_MeshFilter.sharedMesh=null;
            }

            public void ApplyMesh(Mesh _mesh)
            {
                m_MeshFilter.sharedMesh = _mesh;
            }
        }
        
        private Transform m_CameraRoot;
        private Camera m_Camera;
        private Transform m_Selection;
        private Mesh m_SelectionMesh;
        private Vector3 m_RootPosition=Vector3.zero;
        private float m_Pitch = 45f;
        private float m_Yaw = 0f;
        private float m_Offset = 50f;
        private void Awake()
        {
            if (!Application.isPlaying)
                return;
            Setup();
            m_Camera = transform.Find("Camera").GetComponent<Camera>();
            m_AreaMeshes = new TObjectPoolClass<ConvexGridRenderer>(transform.Find("AreaContainer/AreaMesh"));
            m_Selection = transform.Find("Selection");
            m_SelectionMesh = new Mesh {name="Selection",hideFlags = HideFlags.HideAndDontSave};
            m_Selection.GetComponent<MeshFilter>().sharedMesh = m_SelectionMesh;
            ValidateArea(HexagonCoordC.zero);
            UIT_TouchConsole.InitDefaultCommands();
            UIT_TouchConsole.Command("Reset",KeyCode.R).Button(Clear);
        }

        void Update()
        {
            if (!Application.isPlaying)
                return;
            Tick(Time.deltaTime);
            InputTick();
        }

        void InputTick()
        {
            float deltaTime = Time.unscaledDeltaTime;
            var touch=TouchTracker.Execute(deltaTime);
            foreach (var clickPos in touch.ResolveClicks()) 
                OnClick(clickPos);

            int dragCount = touch.Count;
            var drag = touch.CombinedDrag()*deltaTime*5f;
            if (dragCount == 1)
            {
                m_Pitch =Mathf.Clamp(m_Pitch + drag.y, 1, 89);
                m_Yaw += drag.x;
            }
            else
            {
                var pinch= touch.CombinedPinch()*deltaTime*5f;
                m_Offset = Mathf.Clamp( m_Offset + pinch,20f,80f);
                m_RootPosition += Quaternion.Euler(0, m_Yaw, 0) * new Vector3(drag.x,0,drag.y);
            }

            var rotation = Quaternion.Euler(m_Pitch,m_Yaw,0);
            var position = m_RootPosition + rotation * Vector3.forward * -m_Offset;
            m_Camera.transform.SetPositionAndRotation(Vector3.Lerp(m_Camera.transform.position,position,deltaTime*10f),
                Quaternion.Slerp(m_Camera.transform.rotation,rotation,deltaTime*10f));
        }

        void OnClick(Vector2 screenPos)
        {
            GRay ray = m_Camera.ScreenPointToRay(screenPos);
            GPlane plane = new GPlane(Vector3.up, transform.position);
            var hitPos = ray.GetPoint(UGeometry.RayPlaneDistance(plane, ray));
            var hitCoord = (transform.InverseTransformPoint(hitPos) / m_CellRadius).ToCoord();
            var hitHex=hitCoord.ToAxial();
            var hitArea = UHexagonArea.GetBelongAreaCoord(hitHex);
            if (m_GridSelected.Check(ValidateSelection(hitCoord, out m_QuadSelected)))
                PopulateSelectionMesh();
            
            ValidateArea(hitArea);
        }

        partial void ValidateAreaRuntime(HexagonCoordC _areaCoord)
        {
            m_ConvexIterator[EConvexIterate.Meshed].Push(ConstructArea(_areaCoord));
            Debug.LogWarning($"Area Validated{_areaCoord}");
        }

        partial void ClearRuntime()
        {
            m_AreaMeshes?.Clear();
            if(m_Selection!=null)
                m_SelectionMesh.Clear();
        }
        
        void PopulateSelectionMesh()
        {
            bool valid = m_QuadSelected != -1;
            m_Selection.SetActive(valid);
            if (!valid)
                return;

            List<Vector3> vertices = TSPoolList<Vector3>.Spawn();
            List<int> indices = TSPoolList<int>.Spawn();
            List<Vector2> uvs = TSPoolList<Vector2>.Spawn();
            
            var selectVertex = m_Vertices[m_GridSelected];
            foreach (ConvexQuad quad in selectVertex.m_RelativeQuads)
            {
                int startIndex = vertices.Count;
                int[] indexes = {startIndex, startIndex + 1, startIndex + 2, startIndex + 3};

                var hexQuad = quad.m_HexQuad;
                var hexVertices = hexQuad;
                var offset = hexVertices.FindIndex(p=>p==m_GridSelected);
                for (int i = 0; i < 4; i++)
                {
                    int index=(i+offset)%4;
                    vertices.Add( m_TransformMatrix.MultiplyPoint( m_Vertices[hexQuad[index]].m_Coord.ToWorld()));
                    uvs.Add(URender.IndexToQuadUV(i));
                }

                indices.Add(indexes[0]);
                indices.Add(indexes[1]);
                indices.Add(indexes[3]);
                indices.Add(indexes[1]);
                indices.Add(indexes[2]);
                indices.Add(indexes[3]);
            }
            
            m_SelectionMesh.Clear();
            m_SelectionMesh.SetVertices(vertices);
            m_SelectionMesh.SetUVs(0,uvs);
            m_SelectionMesh.SetIndices(indices,MeshTopology.Triangles,0,false);
            
            TSPoolList<Vector3>.Recycle(vertices);
            TSPoolList<Vector2>.Recycle(uvs);
            TSPoolList<int>.Recycle(indices);
        }

        IEnumerator ConstructArea(HexagonCoordC _areaCoord)
        {
            if (!m_Areas.ContainsKey(_areaCoord))
                yield break;
            m_AreaMeshes.AddItem().ApplyMesh (m_Areas[_areaCoord].SetupMesh(m_Vertices,m_TransformMatrix));
            yield return null;
        }
        
        #if UNITY_EDITOR
        private void OnGUI()
        {
            TouchTracker.DrawDebugGUI();
        }

        partial void DrawGizmosRuntime()
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_RootPosition,1f);
        }
        #endif
    }
}