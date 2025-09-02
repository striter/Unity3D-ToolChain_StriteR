using UnityEngine;

namespace UnityEditor.Extensions.MaterialPropertyDrawer
{
    public abstract class AMaterialPropertyDrawerBase: UnityEditor.MaterialPropertyDrawer
    {
        public virtual bool PropertyTypeCheck(MaterialProperty.PropType type) => true;
        private bool isEnabled = false;
        public sealed override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            OnGUI(position,prop,label.text,editor);
        }

        public sealed override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (!isEnabled)
            {
                OnEnable();
                isEnabled = true;
            }

            if (!PropertyTypeCheck(prop.type))
                GUI.Label(position, $"{prop.displayName} Type UnAvailable!", UEGUIStyle_Window.m_ErrorLabel);
            else
                OnPropertyGUI(position, prop, label, editor);
            
            if (!editor.isVisible)
            {
                isEnabled = false;
                OnDisable();
            }
        }

        protected virtual void OnEnable()
        {
            
        }

        protected virtual void OnDisable()
        {
            
        }
        
        public virtual void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            
        }
    }
}