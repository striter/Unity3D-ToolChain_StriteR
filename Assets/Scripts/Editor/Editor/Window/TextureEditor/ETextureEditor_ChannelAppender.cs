using System;
using UnityEngine;

namespace UnityEditor.Extensions.TextureEditor
{
    [Serializable]
    public class ChannelAppender : ITextureEditor
    {
        private Texture2D m_TargetTexture;
        private EColorChannel m_ChannelModify = EColorChannel.None;
        [SerializeField] private ChannelCollector m_AppendCollector = ChannelCollector.kDefault;
        private SerializedProperty pColletor;
        public void OnEnable(SerializedProperty _parentProperty)
        {
            pColletor = _parentProperty.FindPropertyRelative(nameof(m_AppendCollector));
        }

        public void OnDisable()
        {
            pColletor = null;
        }

        public void OnGUI()
        {
            HorizontalScope.NextLine(2,20);
            m_TargetTexture = (Texture2D)EditorGUI.ObjectField(HorizontalScope.NextRect(5, 65), m_TargetTexture, typeof(Texture2D), false);
            HorizontalScope.NextLine(2,20);
            m_ChannelModify = (EColorChannel)EditorGUI.EnumPopup(HorizontalScope.NextRect(5, 65), m_ChannelModify);
            HorizontalScope.NextLine(2, EditorGUI.GetPropertyHeight(pColletor));
            EditorGUI.PropertyField(HorizontalScope.NextRect(30, 300), pColletor,true);
        }

        public bool IsValidTexture(out int width, out int height, out TextureFormat format)
        {
            var valid = m_AppendCollector.Valid();
            var firstValidTexture = m_AppendCollector.operation != EChannelOperation.Constant ? m_AppendCollector.texture:null;
            width = firstValidTexture!=null ? firstValidTexture.width : 2;
            height = firstValidTexture!=null ? firstValidTexture.width : 2;
            format = TextureFormat.RGBA32;
            if (m_ChannelModify == EColorChannel.None)
                return false;

            if (!valid)
                return false;
            if (!m_TargetTexture || !m_TargetTexture.isReadable)
                return false;
            
            var currentWidth = m_TargetTexture.width;
            var currentHeight = m_TargetTexture.height;
            if (width != currentWidth || height != currentHeight)
                return false;

            width = m_TargetTexture.width;
            height = m_TargetTexture.height;
            format = TextureFormat.RGBA32;
            return true;
        }

        public Texture2D GetTextureOutput()
        {
            if (!IsValidTexture(out var width, out var height,out var format))
                return null;
            
            m_AppendCollector.Prepare();
            var totalSize = width * height;
            Color32[] mix = m_TargetTexture.GetPixels32();
            for (int i = 0; i < totalSize; i++)
            {
                switch (m_ChannelModify)
                {
                    case EColorChannel.Red:mix[i].r = UColor.toColor32(m_AppendCollector.Collect(i));break;
                    case EColorChannel.Green:mix[i].g = UColor.toColor32(m_AppendCollector.Collect(i));break;
                    case EColorChannel.Blue:mix[i].b = UColor.toColor32(m_AppendCollector.Collect(i));break;
                    case EColorChannel.Alpha:mix[i].a = UColor.toColor32(m_AppendCollector.Collect(i));break;
                }
            }
            m_AppendCollector.End(); 
            
            var targetTexture = new Texture2D(width, height, format, true);
            targetTexture.SetPixels32(mix);
            targetTexture.Apply();
            return targetTexture;
        }
    }
}