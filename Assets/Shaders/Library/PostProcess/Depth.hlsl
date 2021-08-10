TEXTURE2D( _CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);
half4 _CameraDepthTexture_TexelSize;

float SampleRawDepth(float2 uv){return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture,uv).r;}
float SampleEyeDepth(float2 uv){return RawToEyeDepth(SampleRawDepth(uv));}
float Sample01Depth(float2 uv){return RawTo01Depth(SampleRawDepth(uv));}
float3 TransformNDCToWorld(float2 uv){return TransformNDCToWorld(uv,SampleRawDepth(uv));}

float3 WorldSpaceNormalFromDepth(float2 uv,inout float3 positionWS,inout half depth)
{
    depth=SampleRawDepth(uv);
    positionWS=TransformNDCToWorld(uv,depth);
    float3 position1=TransformNDCToWorld(uv+_MainTex_TexelRight);
    float3 position2=TransformNDCToWorld(uv+_MainTex_TexelUp);
    return normalize(cross(position2-positionWS,position1-positionWS));
}
half3 ClipSpaceNormalFromDepth(float2 uv)
{
    half depth = SampleEyeDepth(uv);
    half depth1 = SampleEyeDepth(uv + _MainTex_TexelRight);
    half depth2 = SampleEyeDepth(uv + _MainTex_TexelUp);
				
    half3 p1 = half3(_MainTex_TexelRight, depth1 - depth);
    half3 p2 = half3(_MainTex_TexelUp, depth2 - depth);
    return normalize(cross(p1, p2));
}
