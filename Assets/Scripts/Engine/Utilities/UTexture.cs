using Unity.Mathematics;
using UnityEngine;

public static class UTexture
{
    public static float2 GetResolution(this Texture _texture) => new (_texture.width, _texture.height);
    public static Vector4 GetTexelSizeParameters(this Texture _texture)=>new (1f/_texture.width,1f/_texture.height,_texture.width,_texture.height);
    public static float2 TransformTex(float2 _uv,float4 _st) => _uv * _st.xy + _st.zw;
    
    public static Color[] ReadPixels(this Texture2D _texture,int _parameter = 1,bool _paramDownSample = true)
    {
        if(_texture.isReadable && _parameter == 1 && _paramDownSample)
            return _texture.GetPixels();
        
        var width = _paramDownSample ? _texture.width / _parameter : _parameter;
        var height = _paramDownSample ? _texture.height / _parameter : _parameter;
        var rt = RenderTexture.GetTemporary(width,height, 0, RenderTextureFormat.ARGB32);
        rt.filterMode = FilterMode.Point;
        Graphics.Blit(_texture, rt);
            
        var readableTexture = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        readableTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readableTexture.Apply();
        var pixels = readableTexture.GetPixels();
        Object.DestroyImmediate(readableTexture);
        RenderTexture.ReleaseTemporary(rt);
        return pixels;
    }
}
