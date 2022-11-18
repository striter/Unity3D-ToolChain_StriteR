using UnityEngine;

public static class KShaderProperties
{
    public static readonly int kColor = Shader.PropertyToID("_Color");
    public static readonly int kEmissionColor = Shader.PropertyToID("_EmissionColor");
    public static readonly int kAlpha = Shader.PropertyToID("_Alpha");
    
    public static readonly int kColorMask = Shader.PropertyToID("_ColorMask");
    public static readonly int kZTest = Shader.PropertyToID("_ZTest");
    public static readonly int kZWrite = Shader.PropertyToID("_ZWrite");
    public static readonly int kCull = Shader.PropertyToID("_Cull");
}