
//Transformations
float3 TransformWorldToViewDir(float3 _positionWS,float4x4 _matrixV)
{
    // [branch]if(unity_OrthoParams.w)
    //     return normalize(_matrixV[2].xyz);
    // else
        return GetCameraPositionWS()-_positionWS;
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
    float2 uv=_hClip.xy*rcp(_hClip.w);
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

//Screen Space Calculations
float3 _FrustumCornersRayBL;
float3 _FrustumCornersRayBR;
float3 _FrustumCornersRayTL;
float3 _FrustumCornersRayTR;

float3 TransformNDCToViewDirWS(float2 uv){return bilinearLerp(_FrustumCornersRayTL, _FrustumCornersRayTR, _FrustumCornersRayBL, _FrustumCornersRayBR, uv);}
float3 TransformNDCToWorld_Frustum(float2 uv,float _rawDepth){ return GetCameraPositionWS() + RawToEyeDepth(_rawDepth) *  TransformNDCToViewDirWS(uv);}
float3 TransformNDCToWorld_VPMatrix(float2 uv,float _rawDepth){ return TransformClipToWorld(TransformNDCToClip(uv,_rawDepth));}
float3 TransformNDCToWorld(float2 uv,float rawDepth)
{
    [branch]
    if(unity_OrthoParams.w)
        return TransformNDCToWorld_VPMatrix(uv,rawDepth);
    else
        return TransformNDCToWorld_Frustum(uv,rawDepth);
}
