using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime;
using Runtime.CameraController;
using Runtime.CameraController.Demo;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Runtime.TouchTracker;
using TPool;
using UnityEngine;

namespace Examples.ArtScenes.CornellBox
{

    public class CornellBox : MonoBehaviour
    {
        public GBox m_Bounding;
        public FControllerInput m_Input;
        public ACameraController m_Controller;
        private FCameraControllerCore m_Core = new FCameraControllerCore();

        private ObjectPoolClass<int,CornellBoxView> m_Views;
        #region Constants
        private static readonly string kViewModelRoot = "ViewModels";
        private static readonly string kViewsTemplateRoot = "Views/Template";
        #endregion
        private void OnEnable()
        {
            var views = transform.Find(kViewModelRoot);
            m_Views = new (transform.Find(kViewsTemplateRoot));
            var quads  = m_Bounding.GetQuads().FillList(UList.Empty<GQuad>());
            if (quads.Count != views.childCount)
            {
                Debug.LogError($"Expected {views.childCount} but got {quads.Count}");
                return;
            }
            
            for(var i = 0; i < views.childCount; i++)
            {
                var root = views.GetChild(i);
                m_Views.Spawn(i).Initialize(i, root, quads[i]);
            }
        }

        private void OnDisable()
        {
            m_Views.Dispose();
        }

        // Update is called once per frame
        void LateUpdate()
        {
            var deltaTime = Time.deltaTime;
            var tracks = UTouchTracker.Execute(deltaTime);
            m_Input.PlayerPinch += tracks.CombinedPinch();
            m_Input.PlayerDrag += tracks.CombinedDrag();
            
            m_Core.Switch(m_Controller);
            m_Core.Tick(deltaTime, ref m_Input);
            
            m_Views.Traversal(p=>p.Tick(m_Input.camera));
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            if (m_Views != null)
            {
                foreach (var view in m_Views)
                    view.DrawGizmos(m_Input.camera);
                return;
            }
            
            foreach (var (index,quad) in m_Bounding.GetQuads().LoopIndex())
            {
                Gizmos.color = UColor.IndexToColor(index);
                new GQuad(quad.Shrink(.99f)).DrawGizmos();
            }
            
            var viewModelRoot = transform.Find(kViewModelRoot);
            if (viewModelRoot != null)
            {
                var quads  = m_Bounding.GetQuads().FillList(UList.Empty<GQuad>());
                for (int i = 0; i < viewModelRoot.childCount; i++)
                {
                    var quad = quads[i];
                    var normal = quad.GetVertexNormals().Average();
                    var center = quad.GetBaryCenter();
                    
                    Gizmos.color = UColor.IndexToColor(i);
                    Gizmos.matrix = transform.localToWorldMatrix;
                    UGizmos.DrawArrow(center,normal,1f,.15f);
                    
                    var root = viewModelRoot.GetChild(i);
                    Gizmos.matrix = root.transform.localToWorldMatrix;
                    m_Bounding.DrawGizmos();
                }
            }
        }
    }

    public class CornellBoxView : APoolTransform<int>
    {
        public Transform m_ViewRoot;
        public RenderTexture m_RenderTexture;
        public GQuad m_Quad;
        private Camera m_Camera;
        private MeshRenderer m_MeshRenderer;

        public CornellBoxView(Transform _transform) : base(_transform) { }
        public CornellBoxView Initialize(int _layerIndex,Transform _viewRoot, GQuad _quad)
        {
            var layer = _layerIndex + 1;
            var layerMask = 1 << layer;
            m_ViewRoot = _viewRoot;
            m_Camera = transform.GetComponentInChildren<Camera>(true);
            m_ViewRoot.SetChildLayer(layer);
            m_RenderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            m_Quad = _quad;
            
            var quadRenderer = transform.GetComponentInChildren<QuadRenderer>();
            quadRenderer.m_Quad = _quad;
            quadRenderer.SetDirty();
            
            var meshRoot = transform.GetComponentInChildren<MeshRenderer>();
            meshRoot.GetComponentInChildren<MeshRenderer>().material.mainTexture = m_RenderTexture;
            m_Camera.targetTexture = m_RenderTexture;
            m_Camera.cullingMask = layerMask;
            return this;
        }

        public void Tick(Camera _camera)
        {
            var plane = new GPlane(m_Quad.GetVertexNormals().Average(),m_Quad.GetBaryCenter());
            var valid = plane.IsFront(_camera.transform.position);

            m_Camera.gameObject.SetActive(valid);
            if (!valid)
                return;
            
            m_Camera.fieldOfView = _camera.fieldOfView;

            var localPosition = _camera.transform.localPosition;
            var localRotation = _camera.transform.localRotation;

            var localToWorldMatrix = m_ViewRoot.localToWorldMatrix;
            var transformedPosition = localToWorldMatrix.MultiplyPoint(localPosition);
            var transformedRotation = localToWorldMatrix.rotation * localRotation;
            
            m_Camera.transform.SetPositionAndRotation(transformedPosition,transformedRotation);
        }

        public override void OnPoolDispose()
        {
            base.OnPoolDispose();
            RenderTexture.ReleaseTemporary(m_RenderTexture);
        }

        public void DrawGizmos(Camera _camera)
        {
            m_Quad.DrawGizmos();
        }
    }
    
}