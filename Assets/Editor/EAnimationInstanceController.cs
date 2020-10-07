using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Rendering.Optimize;
namespace TEditor
{
    [CustomEditor(typeof(AnimationInstanceController))]
    public class EAnimationInstanceController : Editor
    {
        AnimationInstanceController m_Target;
        public bool m_TestTick = false;
        public bool m_SlowTick = false;
        public int m_StartAnim = 0;
        private void OnEnable()
        {
            m_Target = (target as AnimationInstanceController);
            EditorApplication.update += Update;
        }
        private void OnDisable()
        {
            EditorApplication.update -= Update;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (!m_Target.m_Data)
                return;
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Editor Play Test");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Anim Tick:");
            m_TestTick = EditorGUILayout.Toggle(m_TestTick);
            m_SlowTick = EditorGUILayout.Toggle(m_SlowTick);
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
            if (!m_TestTick)
                return;
            m_Target.Tick(Time.deltaTime * (m_SlowTick ? .1f : 1f));
        }
    }
}
