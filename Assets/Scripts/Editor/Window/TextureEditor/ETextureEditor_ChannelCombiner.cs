using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Extensions.TextureEditor
{
    [Serializable]
    class ChannelCombiner : ITextureEditor
    {
        [SerializeField] private ChannelCollector m_R = ChannelCollector.kDefault;
        [SerializeField] private ChannelCollector m_G = ChannelCollector.kDefault;
        [SerializeField] private ChannelCollector m_B = ChannelCollector.kDefault;
        [SerializeField] private ChannelCollector m_A = ChannelCollector.kDefault;
        
        private SerializedProperty pR, pG, pB, pA;
        public void OnEnable(SerializedProperty property)
        {
            pR = property.FindPropertyRelative(nameof(m_R));
            pG = property.FindPropertyRelative(nameof(m_G));
            pB = property.FindPropertyRelative(nameof(m_B));
            pA = property.FindPropertyRelative(nameof(m_A));
        }

        public void OnDisable()
        {
            pR = null; pG = null; pB = null; pA = null;
        }

        
        public void OnGUI()
        {
            HorizontalScope.NextLine(2, EditorGUI.GetPropertyHeight(pR));
            EditorGUI.PropertyField(HorizontalScope.NextRect(30, 300), pR,true);
            HorizontalScope.NextLine(2, EditorGUI.GetPropertyHeight(pG));
            EditorGUI.PropertyField(HorizontalScope.NextRect(30, 300), pG,true);
            HorizontalScope.NextLine(2, EditorGUI.GetPropertyHeight(pB));
            EditorGUI.PropertyField(HorizontalScope.NextRect(30, 300), pB,true);
            HorizontalScope.NextLine(2, EditorGUI.GetPropertyHeight(pA));
            EditorGUI.PropertyField(HorizontalScope.NextRect(30, 300), pA,true);
        }

        public bool IsValidTexture(out int width,out int height,out TextureFormat format)
        {
            var valid = m_R.Valid && m_G.Valid && m_B.Valid && m_A.Valid;
            IEnumerable<ChannelCollector> AllTextures()
            {
                yield return m_R;
                yield return m_G;
                yield return m_B;
                yield return m_A;
            }
            var firstValidTexture = AllTextures().Find(p => p.operation != EChannelOperation.Constant);
            width = firstValidTexture.texture!=null ? firstValidTexture.texture.width : 2;
            height = firstValidTexture.texture!=null ? firstValidTexture.texture.width : 2;
            format = TextureFormat.RGBA32;
            if (!valid)
                return false;
            foreach (var input in AllTextures())
            {
                if (input.operation == EChannelOperation.Constant)
                    continue;
                
                var currentWidth = input.texture.width;
                var currentHeight = input.texture.height;
                if (width != currentWidth || height != currentHeight)
                    return false;
            }

            return true;
        }

        public Texture2D GetTextureOutput()
        {
            if (!IsValidTexture(out var width, out var height,out var format))
                return null;
            
            m_R.Prepare(); m_G.Prepare(); m_B.Prepare(); m_A.Prepare();
            var totalSize = width * height;
            Color[] mix = new Color[totalSize];
            for (int i = 0; i < totalSize; i++)
                mix[i] = new Color(m_R.Collect(i), m_G.Collect(i), m_B.Collect(i), m_A.Collect(i));
            m_R.End(); m_G.End(); m_B.End(); m_A.End();
            
            var targetTexture = new Texture2D(width, height, TextureFormat.RGBA32, true);
            targetTexture.SetPixels(mix);
            targetTexture.Apply();
            return targetTexture;
        }

    }
}