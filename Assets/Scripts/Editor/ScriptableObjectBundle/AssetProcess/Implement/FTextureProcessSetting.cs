using System;
using UnityEngine;

namespace UnityEditor.Extensions.AssetProcess
{
    public class FTextureProcessSetting : ATextureProcess
    {
        [IntEnum(32,64,128,256,512,1024,2048,4096,8192,16384)] public int maxTextureSize = 1024;
        public bool sRGB = true;
        protected override bool Preprocess(TextureImporter _importer)
        {
            _importer.sRGBTexture = sRGB;
            _importer.maxTextureSize = (int)maxTextureSize;
            return true;
        }

        protected override bool Postprocess(Texture2D _target)
        {
            return false;
        }
    }
}