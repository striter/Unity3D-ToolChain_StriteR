using UnityEditor;
using UnityEngine;
using Rendering.Optimize;

namespace TEditor
{
    [CustomEditor(typeof(AnimationInstanceController))]
    public class EAnimationInstanceController : Editor
    {
        PreviewRenderUtility m_Preview;
        AnimationInstanceController m_PreviewTarget;
        MeshRenderer m_PreviewMeshRenderer;
        GameObject m_BoundsViewer;
        MaterialPropertyBlock m_TargetBlock;
        float m_PreviewTickSpeed = 1f;
        int m_PreviewAnimIndex = 0;
        bool m_PreviewReplay = true;
        Vector2 m_RotateDelta;
        Vector3 m_CameraDirection;
        float m_CameraDistance=8f;
        public override bool HasPreviewGUI()
        {
            if (!(target as AnimationInstanceController).m_Data)
                return false;
            return true;
        } 
        private void OnEnable()
        {
            if (!HasPreviewGUI())
                return;
            m_CameraDirection = Vector3.Normalize(new Vector3(0f,3f,15f));
            m_Preview = new PreviewRenderUtility();
            m_Preview.camera.fieldOfView = 30.0f;
            m_Preview.camera.nearClipPlane = 0.3f;
            m_Preview.camera.farClipPlane = 1000;
            m_Preview.camera.transform.position = m_CameraDirection * m_CameraDistance;

            m_PreviewTarget = GameObject.Instantiate(((Component)target).gameObject).GetComponent<AnimationInstanceController>();
            m_Preview.AddSingleGO(m_PreviewTarget.gameObject);
            m_PreviewMeshRenderer = m_PreviewTarget.GetComponent<MeshRenderer>();
            m_PreviewMeshRenderer.sharedMaterial.enableInstancing = true;
            m_TargetBlock = new MaterialPropertyBlock();
            m_PreviewTarget.transform.position = Vector3.zero;
            m_PreviewTarget.Init();
            m_PreviewTarget.SetAnimation(0);

            Material transparentMaterial = new Material(Shader.Find("Game/Unlit/Transparent"));
            transparentMaterial.SetColor("_Color", new Color(1, 1, 1, .3f));

            m_BoundsViewer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m_BoundsViewer.GetComponent<MeshRenderer>().material = transparentMaterial;

            m_Preview.AddSingleGO(m_BoundsViewer.gameObject);
            m_BoundsViewer.transform.SetParent(m_PreviewTarget.transform);
            m_BoundsViewer.transform.localRotation = Quaternion.identity;
            m_BoundsViewer.transform.localScale = m_PreviewTarget.m_MeshFilter.sharedMesh.bounds.size;
            m_BoundsViewer.transform.localPosition = m_PreviewTarget.m_MeshFilter.sharedMesh.bounds.center;
            m_BoundsViewer.SetActive(false);

            EditorApplication.update += Update;
        }

        private void OnDisable()
        {
            if (!HasPreviewGUI())
                return;
            m_Preview.Cleanup();
            m_Preview = null;
            m_PreviewTarget = null;
            m_PreviewMeshRenderer = null;
            m_TargetBlock = null;
            EditorApplication.update -= Update;
        }
        public override void OnPreviewSettings()
        {
            base.OnPreviewSettings();
            AnimationInstanceParam param= m_PreviewTarget.m_Data.m_Animations[m_PreviewTarget.m_CurrentAnimIndex];
            GUILayout.Label(string.Format("{0},Loop:{1}",param.m_Name, param.m_Loop?1:0));
        }
        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            InputCheck();
            PreviewGUI();
            m_Preview.BeginPreview(r, background);
            m_Preview.camera.Render();
            m_Preview.EndAndDrawPreview(r);
        }
        void InputCheck()
        {
            if (Event.current == null)
                return;
            if(Event.current.type == EventType.MouseDrag)
                m_RotateDelta += Event.current.delta;

            if (Event.current.type == EventType.ScrollWheel)
                m_CameraDistance = Mathf.Clamp(m_CameraDistance+Event.current.delta.y*.2f, 0,20f);
        }
        void PreviewGUI()
        {
            AnimationInstanceData m_Data = m_PreviewTarget.m_Data;
            string[] anims = new string[m_Data.m_Animations.Length];
            for (int i = 0; i < anims.Length; i++)
                anims[i] = m_Data.m_Animations[i].m_Name.Substring(m_Data.m_Animations[i].m_Name.LastIndexOf("_") + 1);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Play:");
            m_PreviewAnimIndex = GUILayout.SelectionGrid(m_PreviewAnimIndex, anims, m_Data.m_Animations.Length > 5 ? 5 : m_Data.m_Animations.Length);
            if (m_PreviewTarget.m_CurrentAnimIndex != m_PreviewAnimIndex)
                m_PreviewTarget.SetAnimation(m_PreviewAnimIndex);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Speed:");
            m_PreviewTickSpeed= GUILayout.HorizontalSlider(m_PreviewTickSpeed, 0f,3f);
            GUILayout.Label("Replay:");
            m_PreviewReplay = GUILayout.Toggle(m_PreviewReplay, "");
            GUILayout.Label("Bounds:");
            m_BoundsViewer.SetActive(GUILayout.Toggle(m_BoundsViewer.activeSelf,""));
            GUILayout.EndHorizontal();
        }
        void Update()
        {
            if(m_PreviewReplay&&m_PreviewTarget.GetScale()>=1)
                m_PreviewTarget.SetTime(0f);

            m_PreviewTarget.Tick(0.012f* m_PreviewTickSpeed,m_TargetBlock);
            m_PreviewTarget.m_MeshRenderer.SetPropertyBlock(m_TargetBlock);

            m_PreviewMeshRenderer.SetPropertyBlock(m_TargetBlock);
            m_Preview.camera.transform.position = m_CameraDirection * m_CameraDistance;
            m_Preview.camera.transform.LookAt(m_PreviewTarget.transform);
            m_PreviewTarget.transform.rotation = Quaternion.Euler(m_RotateDelta.y,m_RotateDelta.x,0f);
            Repaint();
        }
    }
}
