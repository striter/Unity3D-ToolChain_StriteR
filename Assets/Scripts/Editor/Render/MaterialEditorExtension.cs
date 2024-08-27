using UnityEngine;
using System.Reflection;

namespace UnityEditor.Extensions
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Material))]
    public class MaterialEditorExtension : MaterialEditor
    {
        public Component m_Renderer { get; private set; }

        private FieldInfo rendererCompField = typeof(MaterialEditor).GetField("m_MeshRendererComp", BindingFlags.NonPublic | BindingFlags.Instance);
        protected override void OnHeaderGUI()
        {
            base.OnHeaderGUI();
            m_Renderer = rendererCompField?.GetValue(this) as Component;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            if (customShaderGUI is ShaderGUIExtension shaderGUI)
            {
                shaderGUI.OnEnable();
                shaderGUI.m_Parent = this;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (customShaderGUI is ShaderGUIExtension shaderGUI)
            {
                shaderGUI.m_Parent = null;
                shaderGUI.OnDisable();
            }
        }
    }
}