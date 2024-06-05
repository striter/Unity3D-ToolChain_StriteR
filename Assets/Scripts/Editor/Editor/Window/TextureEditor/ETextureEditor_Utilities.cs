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
    
    public enum ETextureExportType
    {
        PNG,
        JPG,
        TGA,
        EXR,
    }


    public static class UTextureEditor
    {
        public static string GetExtension(this ETextureExportType _exportType) => _exportType switch
            {
                ETextureExportType.JPG => "jpg",
                ETextureExportType.PNG => "png",
                ETextureExportType.TGA => "tga",
                ETextureExportType.EXR => "exr",
                _ => throw new Exception("Invalid Type:" + _exportType)
            };
        
        public static void ExportTexture(Texture2D _exportTexture,string _filePath,ETextureExportType _exportType)
        {
            var bytes = _exportType switch
            {
                ETextureExportType.TGA => _exportTexture.EncodeToTGA(),
                ETextureExportType.EXR => _exportTexture.EncodeToEXR(),
                ETextureExportType.JPG => _exportTexture.EncodeToJPG(),
                ETextureExportType.PNG => _exportTexture.EncodeToPNG(),
                _ => null
            };
            UEAsset.CreateOrReplaceFile(_filePath,bytes);
        }
    }
}