
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace TEditor
{
    public class MaterialPropertyDrawerBase: MaterialPropertyDrawer
    {
        public virtual bool PropertyAvailable(MaterialProperty prop) => true;
        public bool DrawPropertyAvailableGUI(Rect position, MaterialProperty prop)
        {
            if (!PropertyAvailable(prop))
            {
                GUI.Label(position, string.Format("{0} Type UnAvailable!", prop.displayName), UEGUIStyle_Window.m_ErrorLabel);
                return false;
            }
            return true;
        }
    }
    public class Vector2Drawer: MaterialPropertyDrawerBase
    {
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)=> EditorGUI.GetPropertyHeight( SerializedPropertyType.Vector2,new GUIContent(label));
        public override bool PropertyAvailable(MaterialProperty prop) => prop.type == MaterialProperty.PropType.Vector;
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (!DrawPropertyAvailableGUI(position, prop))
                return;
            prop.vectorValue = EditorGUI.Vector2Field(position, label,prop.vectorValue);
        }
    }
    public class Vector3Drawer : MaterialPropertyDrawerBase
    {
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)=> EditorGUI.GetPropertyHeight( SerializedPropertyType.Vector3,new GUIContent(label));
        public override bool PropertyAvailable(MaterialProperty prop) => prop.type == MaterialProperty.PropType.Vector;
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (!DrawPropertyAvailableGUI(position, prop))
                return;
            prop.vectorValue = EditorGUI.Vector3Field(position, label, prop.vectorValue);
        }
    }
    public class FoldDrawer : MaterialPropertyDrawerBase
    {
        private string[] m_Keywords;
        public FoldDrawer(string[] _keywords) { m_Keywords = _keywords; }
        public FoldDrawer(string _kw1) : this(new string[] { _kw1 }) { }
        public FoldDrawer(string _kw1, string _kw2) : this(new string[] { _kw1, _kw2 }) { }
        public FoldDrawer(string _kw1, string _kw2, string _kw3) : this(new string[] { _kw1, _kw2, _kw3 }) { }
        public FoldDrawer(string _kw1, string _kw2, string _kw3, string _kw4) : this(new string[] { _kw1, _kw2, _kw3, _kw4 }) { }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 0;
        }
        protected bool CheckAvailable(MaterialProperty prop)
        {
            foreach (Material material in prop.targets)
                if (m_Keywords.Any(keyword => material.IsKeywordEnabled(keyword)))
                    return true;
            return false;
        }
        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (CheckAvailable(prop))
                return;
            editor.DefaultShaderProperty(prop, label);
        }
    }
    public class FoldoutDrawer: FoldDrawer
    {
        public FoldoutDrawer(string _kw1) : base(new string[] { _kw1 }) { }
        public FoldoutDrawer(string _kw1, string _kw2) : base(new string[] { _kw1, _kw2 }) { }
        public FoldoutDrawer(string _kw1, string _kw2,string _kw3) : base(new string[] { _kw1, _kw2 ,_kw3}) { }
        public FoldoutDrawer(string _kw1, string _kw2,string _kw3,string _kw4) : base(new string[] { _kw1, _kw2 ,_kw3,_kw4}) { }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 0;
        }
        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (!CheckAvailable(prop))
                return;
            editor.DefaultShaderProperty(prop, label);
        }

    }

    public class ToggleTexDrawer: MaterialPropertyDrawerBase
    {
        protected readonly string m_Keyword;
        public ToggleTexDrawer(string _keyword) {  m_Keyword = _keyword; }
        public override bool PropertyAvailable(MaterialProperty prop) => prop.type == MaterialProperty.PropType.Texture;
        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (!DrawPropertyAvailableGUI(position, prop))
                return;

            EditorGUI.BeginChangeCheck();
            editor.DefaultShaderProperty(prop, label);
            if (!EditorGUI.EndChangeCheck()) 
                return;
            EnableKeyword(prop);
        }
        public override void Apply(MaterialProperty prop)
        {
            base.Apply(prop);
            EnableKeyword(prop);
        }

        void EnableKeyword(MaterialProperty _property)
        {
            foreach (Material material in _property.targets)
                material.EnableKeyword(m_Keyword, _property.textureValue != null);
        }
    }
}
