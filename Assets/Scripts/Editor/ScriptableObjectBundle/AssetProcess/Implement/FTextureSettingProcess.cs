using System;
using UnityEngine;

namespace UnityEditor.Extensions.AssetPipeline.Process
{
    public enum ETextureResolution
    {
        _32 = 32,
        _64 = 64,
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
        _8192 = 8192,
        _16384 = 16384
    }
    
    public class FTextureSettingProcess : ATextureProcess
    {
        public ETextureResolution maxTextureSize = ETextureResolution._1024;
        public bool sRGB = true;
        protected override bool Preprocess(TextureImporter _importer)
        {
            _importer.sRGBTexture = sRGB;
            _importer.maxTextureSize = (int)maxTextureSize;
            return true;
        }

        protected override bool PostProcess(Texture2D _target)
        {
            return false;
        }
    }
}