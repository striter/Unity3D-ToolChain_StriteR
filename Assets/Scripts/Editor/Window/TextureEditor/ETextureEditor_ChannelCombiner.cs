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

        public bool IsValidTexture(out int width, out int height, out TextureFormat format) => IsValidTexture(m_R,m_G,m_B,m_A,out width,out height,out format);
        public Texture2D GetTextureOutput() => Combine(m_R,m_G,m_B,m_A);

        public static bool IsValidTexture(ChannelCollector _r,ChannelCollector _g,ChannelCollector _b,ChannelCollector _a,out int _width,out int _height,out TextureFormat _format)
        {         
            var valid = _r.Valid && _g.Valid && _b.Valid && _a.Valid;
            IEnumerable<ChannelCollector> AllTextures()
            {
                yield return _r;
                yield return _g;
                yield return _b;
                yield return _a;
            }
            var firstValidTexture = AllTextures().Find(p => p.operation != EChannelOperation.Constant);
            _width = firstValidTexture.texture!=null ? firstValidTexture.texture.width : 2;
            _height = firstValidTexture.texture!=null ? firstValidTexture.texture.height : 2;
            _format = TextureFormat.RGBA32;
            if (!valid)
                return false;
            foreach (var input in AllTextures())
            {
                if (input.operation == EChannelOperation.Constant)
                    continue;
                
                var currentWidth = input.texture.width;
                var currentHeight = input.texture.height;
                if (_width != currentWidth || _height != currentHeight)
                    return false;
            }
            
            return true;
        }
        
        public static Texture2D Combine(ChannelCollector _r,ChannelCollector _g,ChannelCollector _b,ChannelCollector _a)
        {
            if (!IsValidTexture(_r, _g, _b, _a, out var width, out var height, out var format))
            {
                return null;
            }
            
            _r.Prepare(); _g.Prepare(); _b.Prepare(); _a.Prepare();
            var totalSize = width * height;
            Color[] mix = new Color[totalSize];
            for (int i = 0; i < totalSize; i++)
                mix[i] = new Color(_r.Collect(i), _g.Collect(i), _b.Collect(i), _a.Collect(i));
            _r.End(); _g.End(); _b.End(); _a.End();
            
            var targetTexture = new Texture2D(width, height, format, true);
            targetTexture.SetPixels(mix);
            targetTexture.Apply();
            return targetTexture;
        }
        
    }
}