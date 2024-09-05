using UnityEngine;
using UnityEditor;
using System.Linq.Extensions;
using Rendering.GI.SphericalHarmonics;

namespace UnityEditor.Extensions
{
    public class MaterialPropertyDrawerBase: MaterialPropertyDrawer
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
    public class Vector2Drawer: MaterialPropertyDrawerBase
    {
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)=> EditorGUI.GetPropertyHeight( SerializedPropertyType.Vector2,new GUIContent(label));
        public override bool PropertyTypeCheck(MaterialProperty.PropType type) => type == MaterialProperty.PropType.Vector;
        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            base.OnPropertyGUI(position, prop, label, editor);
            prop.vectorValue = EditorGUI.Vector2Field(position, label,prop.vectorValue);
        }
    }
    public class Vector3Drawer : MaterialPropertyDrawerBase
    {
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)=> EditorGUI.GetPropertyHeight( SerializedPropertyType.Vector3,new GUIContent(label));
        public override bool PropertyTypeCheck(MaterialProperty.PropType type) => type == MaterialProperty.PropType.Vector;
        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            base.OnPropertyGUI(position, prop, label, editor);
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

        public bool PropertyTypeCheck(MaterialProperty prop)
        {
            foreach (Material material in prop.targets)
                if (m_Keywords.Any(keyword => material.IsKeywordEnabled(keyword)))
                    return true;
            return false;
        }

        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            base.OnPropertyGUI(position, prop, label, editor);
            if (PropertyTypeCheck(prop))
                return;
            editor.DefaultShaderProperty(prop, label);
        }
    }
    public class FoldoutDrawer: FoldDrawer
    {
        public FoldoutDrawer(string _kw1) : base(new string[] { _kw1 }) {}
        public FoldoutDrawer(string _kw1, string _kw2) : base(new string[] { _kw1, _kw2 }) { }
        public FoldoutDrawer(string _kw1, string _kw2,string _kw3) : base(new string[] { _kw1, _kw2 ,_kw3}) { }
        public FoldoutDrawer(string _kw1, string _kw2,string _kw3,string _kw4) : base(new string[] { _kw1, _kw2 ,_kw3,_kw4}) { }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 0;
        }

        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (!PropertyTypeCheck(prop))
                return;
            
            base.OnPropertyGUI(position, prop, label, editor);
            editor.DefaultShaderProperty(prop, label);
        }
    }

    public class ToggleTexDrawer: MaterialPropertyDrawerBase
    {
        protected readonly string m_Keyword;
        public ToggleTexDrawer(string _keyword) {  m_Keyword = _keyword; }
        public override bool PropertyTypeCheck(MaterialProperty.PropType type) => type == MaterialProperty.PropType.Texture;
        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
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

    public class ColorUsageDrawer : MaterialPropertyDrawerBase
    {
        private bool m_Alpha;
        private bool m_HDR;
        public ColorUsageDrawer(string _alpha,string _hdr)
        {
            m_Alpha = bool.Parse(_alpha);
            m_HDR = bool.Parse(_hdr);
        }
        public override bool PropertyTypeCheck(MaterialProperty.PropType type)=>type== MaterialProperty.PropType.Color;
        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            prop.colorValue = EditorGUI.ColorField(position,new GUIContent(label), prop.colorValue,true,m_Alpha,m_HDR);
        }
    }
    
    public class MinMaxRangeDrawer : MaterialPropertyDrawerBase
    {
        private float m_Min;
        private float m_Max;
        private float m_ValueMin;
        private float m_ValueMax;
        private MaterialProperty property;
        public override bool PropertyTypeCheck(MaterialProperty.PropType type) => type == MaterialProperty.PropType.Range;

        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            var prop0 = MaterialEditor.GetMaterialProperty(editor.targets, prop.name);
            var prop1 =  MaterialEditor.GetMaterialProperty(editor.targets, prop.name + "End");

            float value0 = prop0.floatValue;
            float value1 =prop1.floatValue;
            
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 0.0f;
            // EditorGUI.showMixedValue = hasMixedValue1;
            EditorGUI.BeginChangeCheck();

            Rect minmaxRect = position.Collapse(new Vector2(position.size.x / 5, 0f),new Vector2(0f,0f));
            EditorGUI.MinMaxSlider(minmaxRect,label,ref value0,ref value1,prop.rangeLimits.x,prop.rangeLimits.y);
            Rect labelRect = position.Collapse(new Vector2(position.size.x*4f / 5, 0f),new Vector2(1f,0f)).Move(new Vector2(2f,0f));
            GUI.Label(labelRect,$"{value0:F1}-{value1:F1}");
            
            if (EditorGUI.EndChangeCheck())
            {
                prop0.floatValue = value0;
                prop1.floatValue = value1;
            }
            
            // EditorGUI.showMixedValue = false;
            EditorGUIUtility.labelWidth = labelWidth;
        }
    }

    public class SHGradientDrawer : MaterialPropertyDrawerBase
    {   //Dude
        public override bool PropertyTypeCheck(MaterialProperty.PropType _type) => _type == MaterialProperty.PropType.Color;

        private string keyword;
        public SHGradientDrawer(string _keyword)
        {
            keyword = _keyword;
        }

        public override void OnPropertyGUI(Rect _position, MaterialProperty _prop, string _label, MaterialEditor _editor)
        {
            EditorGUI.BeginChangeCheck();
            _prop.colorValue = EditorGUI.ColorField(_position,new GUIContent(_label),_prop.colorValue,true,false,true);

            if (EditorGUI.EndChangeCheck())
            {
                var sky = MaterialEditor.GetMaterialProperty(_editor.targets, keyword+"Sky");
                var equator =  MaterialEditor.GetMaterialProperty(_editor.targets, keyword + "Equator");
                var ground =  MaterialEditor.GetMaterialProperty(_editor.targets, keyword + "Ground");
                var shData = SphericalHarmonicsExport.ExportGradient(sky.colorValue.to3(), equator.colorValue.to3(), ground.colorValue.to3());
                var output = (SHL2ShaderConstants)shData;
                
                MaterialEditor.GetMaterialProperty(_editor.targets, keyword+"SHAr").vectorValue = output.shAr;
                MaterialEditor.GetMaterialProperty(_editor.targets, keyword+"SHAg").vectorValue = output.shAg;
                MaterialEditor.GetMaterialProperty(_editor.targets, keyword+"SHAb").vectorValue = output.shAb;
                MaterialEditor.GetMaterialProperty(_editor.targets, keyword+"SHBr").vectorValue = output.shBr;
                MaterialEditor.GetMaterialProperty(_editor.targets, keyword+"SHBg").vectorValue = output.shBg;
                MaterialEditor.GetMaterialProperty(_editor.targets, keyword+"SHBb").vectorValue = output.shBb;
                MaterialEditor.GetMaterialProperty(_editor.targets, keyword+"SHC").vectorValue = output.shC.to4();
            }
        }
    }

    public class WaveLengthDrawer : MaterialPropertyDrawerBase
    {
        private string keyword;
        public WaveLengthDrawer(string _keyword)
        {
            keyword = _keyword;
        }
        public override bool PropertyTypeCheck(MaterialProperty.PropType type)=>type == MaterialProperty.PropType.Vector;

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return base.GetPropertyHeight(prop, label, editor) + 20;
        }

        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            editor.DefaultShaderProperty(position.ResizeY(position.y-20),prop,label);
            Vector4 srcValue = prop.vectorValue;
            Vector3 waveLength = srcValue;
            float strength = srcValue.w;
            MaterialEditor.GetMaterialProperty(editor.targets, keyword).vectorValue = new Vector3(
                Mathf.Pow(400f/waveLength.x,4),Mathf.Pow(400f/waveLength.y,4),Mathf.Pow(400f/waveLength.z,4))*strength;
        }
    }
    
    public class Rotation2DDrawer : MaterialPropertyDrawerBase
    {
        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();
            prop.floatValue = EditorGUI.Slider(position,label,prop.floatValue, 0f,360f);
            if (EditorGUI.EndChangeCheck())
            {
                var rotationMatrix = umath.Rotate2D(prop.floatValue*kmath.kDeg2Rad);
                
                var matrixProperty = MaterialEditor.GetMaterialProperty(editor.targets, prop.name+"Matrix");
                matrixProperty.vectorValue = new Vector4(rotationMatrix.c0.x, rotationMatrix.c1.x, 
                    rotationMatrix.c1.x,rotationMatrix.c1.y);
            }
        }
    }
}
