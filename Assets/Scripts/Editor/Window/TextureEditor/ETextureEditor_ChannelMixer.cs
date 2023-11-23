using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Extensions.TextureEditor
{
    [Serializable]
    class ChannelMixer : ITextureEditor
    {
        public enum EChannelOperation
        {
            Constant = 0,
            R = 10, ROneMinus,
            G = 20, GOneMinus,
            B = 30, BOneMinus,
            A = 40, AOneMinus,
            RGBtoLuminance,
        }
        
        [Serializable]
        public struct ChannelCollector
        {
            public EChannelOperation operation;
            [MFoldout(nameof(operation), EChannelOperation.Constant)] [Range(0, 1)] public float constantValue;
            [MFold(nameof(operation),EChannelOperation.Constant)] public Texture2D texture;

            public bool Valid => operation == EChannelOperation.Constant || (texture != null && texture.isReadable);
            
            private Color[] pixels;
            public void Prepare()
            {
                if (operation == EChannelOperation.Constant)
                    return;
                pixels = texture.GetPixels();
            }
            
            public float Collect(int _index)
            {
                if (operation == EChannelOperation.Constant)
                    return constantValue;

                var color = pixels[_index];
                return operation switch
                {
                    EChannelOperation.R => color.r, EChannelOperation.ROneMinus => 1 - color.r,
                    EChannelOperation.G => color.g, EChannelOperation.GOneMinus => 1 - color.g,
                    EChannelOperation.B => color.b, EChannelOperation.BOneMinus => 1 - color.b,
                    EChannelOperation.A => color.a, EChannelOperation.AOneMinus => 1 - color.a,
                    EChannelOperation.RGBtoLuminance => UColor.RGBtoLuminance(color.ToFloat3()),
                    _ => throw new InvalidEnumArgumentException()
                };
            }

            public void End()
            {
                pixels = null;
            }
            
            public static readonly ChannelCollector kDefault = new ChannelCollector()
            {
                texture = null,
                operation = EChannelOperation.R,
            };
        }

        [SerializeField] private ChannelCollector m_R = ChannelCollector.kDefault,
                                                    m_G = ChannelCollector.kDefault,
                                                    m_B = ChannelCollector.kDefault,
                                                    m_A = ChannelCollector.kDefault;
        
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
        
        public bool IsValidTexture() => m_R.Valid && m_G.Valid && m_B.Valid && m_A.Valid;

        public Texture2D GetTextureOutput()
        {
            IEnumerable<ChannelCollector> AllTextures()
            {
                yield return m_R;
                yield return m_G;
                yield return m_B;
                yield return m_A;
            }

            var firstValidTexture = AllTextures().Find(p => p.operation != EChannelOperation.Constant);
            
            var width = firstValidTexture.texture!=null ? firstValidTexture.texture.width : 2;
            var height = firstValidTexture.texture!=null ? firstValidTexture.texture.width : 2;
            foreach (var input in AllTextures())
            {
                if (input.operation == EChannelOperation.Constant)
                    continue;
                
                var currentWidth = input.texture.width;
                var currentHeight = input.texture.height;
                if (width != currentWidth || height != currentHeight)
                    return null;
            }
            
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