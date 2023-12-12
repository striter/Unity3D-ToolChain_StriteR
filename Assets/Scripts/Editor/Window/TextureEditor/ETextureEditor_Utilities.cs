using System;
using System.ComponentModel;
using UnityEngine;

namespace UnityEditor.Extensions.TextureEditor
{
    public enum EChannelOperation
    {
        Constant = 0,
        R = 10, ROneMinus,
        G = 20, GOneMinus,
        B = 30, BOneMinus,
        A = 40, AOneMinus,
        LightmapToLuminance,
    }
    
    [Serializable]
    public struct ChannelCollector
    {
        public EChannelOperation operation;
        [MFoldout(nameof(operation), EChannelOperation.Constant)] [Range(0, 1)] public float constantValue;
        [MFold(nameof(operation),EChannelOperation.Constant)] public Texture2D texture;

        public bool Valid => operation == EChannelOperation.Constant || (texture != null && texture.isReadable);
        
        private Color32[] pixels;
        public void Prepare()
        {
            if (operation == EChannelOperation.Constant)
                return;
            pixels = texture.GetPixels32();
        }
        
        public byte Collect(int _index)
        {
            if (operation == EChannelOperation.Constant)
                return UColor.toColor32(constantValue);

            var color = pixels[_index];
            return operation switch
            {
                EChannelOperation.R => color.r, EChannelOperation.ROneMinus => UColor.toColor32(1f - UColor.toColor(color.r)),
                EChannelOperation.G => color.g, EChannelOperation.GOneMinus => UColor.toColor32(1f - UColor.toColor(color.g)),
                EChannelOperation.B => color.b, EChannelOperation.BOneMinus => UColor.toColor32(1f - UColor.toColor(color.b)),
                EChannelOperation.A => color.a, EChannelOperation.AOneMinus => UColor.toColor32(1f - UColor.toColor(color.a)),
                EChannelOperation.LightmapToLuminance => UColor.toColor32(UColorTransform.RGBtoLuminance(color.toColor().to3())),
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

}