half3 TransformObjectDirToWorld(float3 _dirOS,bool _doNormalize = true)
{
    half3 dirWS=mul((float3x3)unity_ObjectToWorld,_dirOS);
    if(_doNormalize)
        dirWS=normalize(dirWS);
    return dirWS;
}

half3 TransformObjectNormalToWorld(float3 _normalOS,bool _doNormalize = true)
{
    #ifdef UNITY_ASSUME_UNIFORM_SCALING
        return TransformObjectDirToWorld(normalOS,_doNormalize);
    #else

    half3 normalWS=mul(_normalOS,(float3x3)unity_WorldToObject);
    if(_doNormalize)
        normalWS=normalize(normalWS);
    return normalWS;
    #endif
}


float4 TransformObjectToView(float3 positionOS)
{
    return mul(UNITY_MATRIX_MV,float4(positionOS,1));
}

float3 TransformNDCToClip(float2 _uv,float _depth)
{
    float deviceDepth=_depth;
    #if !UNITY_REVERSED_Z
        deviceDepth=lerp(UNITY_NEAR_CLIP_VALUE,1,deviceDepth);
    #endif
    
    float2 uv=_uv*2.-1;
    #if UNITY_UV_STARTS_AT_TOP
    uv.y=-uv.y;
    #endif
    return float3(uv,deviceDepth);
}

float2 TransformHClipToNDC(float4 _hClip)
{
    float2 uv=_hClip.xy/max(FLT_EPS,_hClip.w);
    uv=uv*.5h+.5h;
    #if UNITY_UV_STARTS_AT_TOP
        uv.y=1.h-uv.y;
    #endif
    return uv;
}

float4x4 _Matrix_VP;
float4x4 _Matrix_V;
float4x4 _Matrix_I_VP;
float3 TransformClipToWorld(float3 _positionCS)
{
    float4 pos=mul(_Matrix_I_VP,float4(_positionCS,1));
    return pos.xyz/pos.w;
}

float2 TransformViewToNDC(float4 positionVS)
{
    float height=2*positionVS.z/_Matrix_V._m11;
    float width=_ScreenParams.x/_ScreenParams.y *height;
    return float2(positionVS.x/width,positionVS.y/height)+.5;
}

void TransformHClipToUVDepth(float4 positionCS,out float2 uv,out float rawDepth)
{
    float3 divide=positionCS.xyz/positionCS.w;
    rawDepth = divide.z;
    uv= (divide.xy + 1.) * .5;
    #if UNITY_UV_STARTS_AT_TOP
        uv.y = 1 - uv.y;
    #endif
}

float3 TransformNDCToWorld_Perspective(float2 uv,float _rawDepth){ return GetCameraPositionWS() + RawToEyeDepth(_rawDepth) *  TransformNDCToFrustumCornersRay(uv);}
float3 TransformNDCToWorld_VPMatrix(float2 uv,float _rawDepth){ return TransformClipToWorld(TransformNDCToClip(uv,_rawDepth));}
float3 TransformNDCToWorld(float2 uv,float rawDepth)
{
    [branch]
    if(unity_OrthoParams.w)
        return TransformNDCToWorld_VPMatrix(uv,rawDepth);
    else
        return TransformNDCToWorld_Perspective(uv,rawDepth);
}