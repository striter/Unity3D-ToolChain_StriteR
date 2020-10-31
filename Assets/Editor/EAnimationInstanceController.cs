using UnityEditor;
using UnityEngine;
using Rendering.Optimize;

namespace TEditor
{
    [CustomEditor(typeof(AnimationInstanceController))]
    public class EAnimationInstanceController : Editor
    {
        AnimationInstanceController m_Target;
        public float m_TestTick = 0;
        public int m_StartAnim = 0;
        MaterialPropertyBlock m_TargetBlock;
        MeshRenderer m_Renderer;
        private void OnEnable()
        {
            m_Target = (target as AnimationInstanceController);

            if (AssetDatabase.IsMainAsset(m_Target.gameObject))
                return;

            if (!m_Target.m_Data)
                return;

            m_TargetBlock = new MaterialPropertyBlock();
            m_Renderer = m_Target.GetComponent<MeshRenderer>();
            m_Target.Init(m_TargetBlock);
            m_Target.SetAnimation(m_StartAnim);
            EditorApplication.update += Update;
        }
        private void OnDisable()
        {
            if (!m_Target.m_Inited)
                return;

            EditorApplication.update -= Update;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (!m_Target.m_Inited)
                return;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Editor Play Test");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Anim Tick:");
            m_TestTick = EditorGUILayout.Slider(m_TestTick, 0, .1f);
            int anim = EditorGUILayout.IntField(m_StartAnim);
            if (anim != m_StartAnim)
            {
                m_StartAnim = anim;
                m_Target.SetAnimation(m_StartAnim);
            }

            if (GUILayout.Button("Replay"))
                m_Target.SetTime(0);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Culling Gizmo");
            AnimationInstanceController.m_DrawGizmos = EditorGUILayout.Toggle(AnimationInstanceController.m_DrawGizmos);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        void Update()
        {
            if (m_TestTick <= 0)
                return;
            m_Target.Tick(m_TestTick);
            m_Renderer.SetPropertyBlock(m_TargetBlock);
            EditorUtility.SetDirty(this);
        }
    }
}
