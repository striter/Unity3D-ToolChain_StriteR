float2 TransformTex(float2 _uv, float4 _st) {return _uv * _st.xy + _st.zw;}
float2 TransformTex_Flow(float2 _uv,float4 _st) {return _uv * _st.xy + _Time.y*_st.zw;}

float2 UVRemap_Triplanar(float3 _positionWS, half3 _normalWS)
{
    return _positionWS.zy * _normalWS.x + _positionWS.xz * _normalWS.y + _positionWS.xy * _normalWS.z;
}

float2 UVRemap_TRS(float2 uv,float2 offset, float rotateAngle, float2 tilling)
{
    const float2 center = float2(.5, .5);
    uv = uv + offset;
    offset += center;
    float2 centerUV = uv - offset;
    return mul( Rotate2x2(rotateAngle), centerUV) * tilling + offset;
}