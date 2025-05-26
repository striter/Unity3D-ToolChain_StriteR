using System;
using UnityEngine;

namespace UnityEditor.Extensions.TextureEditor
{
    [Serializable]
    public class ChannelModifier : ITextureEditor
    {
        Texture2D m_ModifyTexture;
        private Texture2D m_TargetTexture;
        EColorChannelFlags m_ChannelModify = EColorChannelFlags.None;
        public void OnEnable(SerializedProperty _parent)
        {
        }

        public void OnDisable()
        {
            m_ModifyTexture = null;
        }

        public bool IsValidTexture(out int width, out int height,out TextureFormat format)
        {
            width = 2;
            height = 2;
            format = TextureFormat.RGBA32;
            if (m_TargetTexture != null)
            {
                width = m_TargetTexture.width;
                height = m_TargetTexture.height;
                format = m_TargetTexture.format;
                return true;
            }
            return false;
        }

        public Texture2D GetTextureOutput() => m_TargetTexture;

        public void OnGUI()
        {
            HorizontalScope.NextLine(2,20);
            EditorGUI.LabelField(HorizontalScope.NextRect(0, 60), "Texture:");
            EditorGUI.BeginChangeCheck();
            m_ModifyTexture = (Texture2D)EditorGUI.ObjectField(HorizontalScope.NextRect(5, 65), m_ModifyTexture, typeof(Texture2D), false);

            if (m_ModifyTexture == null || !m_ModifyTexture.isReadable)
                return;
            
            if (EditorGUI.EndChangeCheck())
            {
                m_TargetTexture = new Texture2D(m_ModifyTexture.width, m_ModifyTexture.height, TextureFormat.RGBA32, true);
                ResetModifyTexture(m_TargetTexture);
            }

            HorizontalScope.NextLine(2, 20);
            EditorGUI.LabelField(HorizontalScope.NextRect(5, 60), "Modify:", UEGUIStyle_Window.m_TitleLabel);
            m_ChannelModify = (EColorChannelFlags)EditorGUI.EnumPopup(HorizontalScope.NextRect(5, 120), m_ChannelModify);

            if (m_ChannelModify != EColorChannelFlags.None)
            {
                HorizontalScope.NextLine(2, 20);
                if (GUI.Button(HorizontalScope.NextRect(10, 60), "Reverse"))
                    DoColorModify(m_TargetTexture, m_ChannelModify, value => 1 - value);
                if (GUI.Button(HorizontalScope.NextRect(10, 60), "Fill"))
                    DoColorModify(m_TargetTexture, m_ChannelModify, value => 1);
                if (GUI.Button(HorizontalScope.NextRect(10, 60), "Clear"))
                    DoColorModify(m_TargetTexture, m_ChannelModify, value => 0);
            }

            if (GUI.Button(HorizontalScope.NextRect(5, 80), "Reset"))
                ResetModifyTexture(m_TargetTexture);
        }

        void ResetModifyTexture(Texture2D _targetTexture)
        {
            _targetTexture.SetPixels(m_ModifyTexture.GetPixels());
            _targetTexture.Apply();
        }

        static void DoColorModify(Texture2D _target, EColorChannelFlags _color, Func<float, float> _OnEachValue)
        {
            Color[] colors = _target.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                switch (_color)
                {
                    case EColorChannelFlags.RGBA: colors[i] = new Color(_OnEachValue(colors[i].r), _OnEachValue(colors[i].g), _OnEachValue(colors[i].b), _OnEachValue(colors[i].a)); break;
                    case EColorChannelFlags.RGB: colors[i] = new Color(_OnEachValue(colors[i].r), _OnEachValue(colors[i].g), _OnEachValue(colors[i].b), colors[i].a); break;
                    case EColorChannelFlags.R: colors[i] = new Color(_OnEachValue(colors[i].r), colors[i].g, colors[i].b, colors[i].a); break;
                    case EColorChannelFlags.G: colors[i] = new Color(colors[i].r, _OnEachValue(colors[i].g), colors[i].b, colors[i].a); break;
                    case EColorChannelFlags.B: colors[i] = new Color(colors[i].r, colors[i].g, _OnEachValue(colors[i].b), colors[i].a); break;
                    case EColorChannelFlags.A: colors[i] = new Color(colors[i].r, colors[i].g, colors[i].b, _OnEachValue(colors[i].a)); break;
                }
            }
            _target.SetPixels(colors);
            _target.Apply();
        }
    }
}