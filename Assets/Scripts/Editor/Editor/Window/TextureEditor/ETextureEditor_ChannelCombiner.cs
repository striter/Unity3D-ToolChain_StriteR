using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Extensions;
using UnityEngine;

namespace UnityEditor.Extensions.TextureEditor
{
    interface IChannelCollector
    {
        EChannelOperation Operation { get; }
        Texture2D Texture { get; }
        float ConstantValue { get; }
        Color[] PixelsResolved { get; set; }
    }
    
    internal static class IChannelCollector_Extension
    {
        public static bool Valid(this IChannelCollector _r)=>_r.Operation == EChannelOperation.Constant || (_r.Texture != null && _r.Texture.isReadable);
        
        public static void Prepare(this IChannelCollector _r)
        {
            if (_r.Operation == EChannelOperation.Constant)
                return;
            _r.PixelsResolved = _r.Texture.GetPixels();
        }
        
        public static float Collect(this IChannelCollector _r,int _index)
        {
            if (_r.Operation == EChannelOperation.Constant)
                return UColor.toColor32(_r.ConstantValue);

            var color = _r.PixelsResolved[_index];
            return _r.Operation switch
            {
                EChannelOperation.R => color.r, EChannelOperation.ROneMinus => 1f - color.r,
                EChannelOperation.G => color.g, EChannelOperation.GOneMinus => 1f - color.g,
                EChannelOperation.B => color.b, EChannelOperation.BOneMinus => 1f - color.b,
                EChannelOperation.A => color.a, EChannelOperation.AOneMinus => 1f - color.a,
                EChannelOperation.LightmapToLuminance => color.to3().sum()/3f,
                _ => throw new InvalidEnumArgumentException()
            };
        }

        public static void End(this IChannelCollector _r)
        {
            _r.PixelsResolved = null;
        }
    }
    
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

        public bool IsValidTexture(out int width, out int height,out TextureFormat _format)
        {
            _format = TextureFormat.ARGB32;
            return IsValidTexture(m_R,m_G,m_B,m_A,out width,out height);
        }
        public Texture2D GetTextureOutput() => Combine(m_R,m_G,m_B,m_A,TextureFormat.ARGB32);

        public static bool IsValidTexture(IChannelCollector _r,IChannelCollector _g,IChannelCollector _b,IChannelCollector _a,out int _width,out int _height)
        {         
            var valid = _r.Valid() && _g.Valid() && _b.Valid() && _a.Valid();
            IEnumerable<IChannelCollector> AllTextures()
            {
                yield return _r;
                yield return _g;
                yield return _b;
                yield return _a;
            }
            var firstValidTexture = AllTextures().Find(p => p.Operation != EChannelOperation.Constant);
            _width = firstValidTexture.Texture!=null ? firstValidTexture.Texture.width : 2;
            _height = firstValidTexture.Texture!=null ? firstValidTexture.Texture.height : 2;
            if (!valid)
                return false;
            foreach (var input in AllTextures())
            {
                if (input.Operation == EChannelOperation.Constant)
                    continue;
                
                var currentWidth = input.Texture.width;
                var currentHeight = input.Texture.height;
                if (_width != currentWidth || _height != currentHeight)
                    return false;
            }
            
            return true;
        }
        public static Texture2D Combine(IChannelCollector _r,IChannelCollector _g,IChannelCollector _b,IChannelCollector _a,TextureFormat _format)
        {
            if (!IsValidTexture(_r, _g, _b, _a, out var width, out var height))
                return null;
            
            _r.Prepare(); _g.Prepare(); _b.Prepare(); _a.Prepare();
            var totalSize = width * height;
            Color[] mix = new Color[totalSize];
            for (int i = 0; i < totalSize; i++)
                mix[i] = new Color(_r.Collect(i), _g.Collect(i), _b.Collect(i), _a.Collect(i));
            _r.End(); _g.End(); _b.End(); _a.End();
            
            var targetTexture = new Texture2D(width, height, _format, true);
            targetTexture.SetPixels(mix);
            targetTexture.Apply();
            return targetTexture;
        }
        
    }
}