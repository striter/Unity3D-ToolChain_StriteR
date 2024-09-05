using System.Reflection;
using UnityEngine;

namespace UnityEditor.Extensions
{
    public class ShaderGUIExtension : ShaderGUI
    {
        protected Component m_Renderer { get; private set; }
        private FieldInfo rendererCompField = typeof(MaterialEditor).GetField("m_MeshRendererComp", BindingFlags.NonPublic | BindingFlags.Instance);
        private MaterialEditor editor;
        protected virtual void OnEnable(MaterialEditor materialEditor)
        {
            m_Renderer = rendererCompField?.GetValue(materialEditor) as Component;
            editor = materialEditor;
            EditorApplication.update += TickDisable;
        }

        protected virtual void OnDisable()
        {
            m_Renderer = null;
            editor = null;
            EditorApplication.update -= TickDisable;
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);
            if (editor == null)
                OnEnable(materialEditor);
        }

        void TickDisable()
        {
            if (editor != null)
                return;
            OnDisable();
        }

    }
}