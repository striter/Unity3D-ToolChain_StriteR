using UnityEngine;

namespace UnityEditor.Extensions
{
    public class ShaderGUIExtension : ShaderGUI
    {
        public MaterialEditorExtension m_Parent { get; set; }
        protected Component m_Renderer => m_Parent?.m_Renderer;
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
    }
}