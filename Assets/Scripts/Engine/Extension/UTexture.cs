using Unity.Mathematics;
using UnityEngine;

public static class UTexture
{
    public static float2 GetResolution(this Texture _texture) => new float2(_texture.width, _texture.height);
    public static Vector4 GetTexelSizeParameters(this Texture _texture)=>new Vector4(1f/_texture.width,1f/_texture.height,_texture.width,_texture.height);
    public static float2 TransformTex(float2 _uv,float4 _st) => _uv * _st.xy + _st.zw;
}
