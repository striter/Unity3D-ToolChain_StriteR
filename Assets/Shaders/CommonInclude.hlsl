#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Library/ValueMapping.hlsl"

float2 TransformTex(float2 _uv, float4 _st) {return _uv * _st.xy + _st.zw;}
#define PI_HALF 1.5707963267949
#define PI_TWO 6.2831853071796
#define PI_ONE_DIV 0.31830988618379
#define PI_ONE_DIV_TWO 0.15915494309189
#define INSTANCING_BUFFER_START UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
#define INSTANCING_PROP(type,param) UNITY_DEFINE_INSTANCED_PROP(type,param)
#define INSTANCING_BUFFER_END UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)
#define INSTANCE(param) UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,param)
#define TRANSFORM_TEX_INSTANCE(uv,tex) TransformTex(uv,INSTANCE(tex##_ST));

//Depth
#if !UNITY_REVERSED_Z
#define Z_Multiply 1.h
#define Z_BEGIN 1.h
#define Z_END 0.h
#else
#define Z_Multiply -1.h
#define Z_BEGIN 1.h
#define Z_END 0.h
#endif
bool DepthGreater(float _depthSrc,float _depthComp)
{
    #if !UNITY_REVERSED_Z
        return _depthSrc>_depthComp;
    #else
        return _depthSrc<_depthComp;
    #endif
}

//Transformations
float3 TransformUVDepthToClipPosition(float2 _uv,float _depth)
{
    #if UNITY_UV_STARTS_AT_TOP
    _uv.y=1-_uv.y;
    #endif
    return float3(_uv*2.-1.,_depth);
}
float3 TransformHClipToClipPosition(float4 _hClip)
{
    _hClip.xy=_hClip.xy*.5+1;
    return _hClip;
}
half2 TransformHClipToScreenUV(float4 _hClip)
{
    half2 uv=_hClip.xy*rcp(_hClip.w);
    uv=uv*.5+.5;
#if UNITY_UV_STARTS_AT_TOP
    uv.y=1-uv.y;
#endif
    return uv;
}

float4x4 _Matrix_VP;
float4x4 _Matrix_I_VP;
float3 TransformClipToWorld(float3 _positionCS)
{
    float4 pos=mul(_Matrix_I_VP,float4(_positionCS,1));
    return pos.xyz/pos.w;
}

float3 TransformObjectToHClipNormal(float3 _normalOS)
{
    return mul((float3x3) GetWorldToHClipMatrix(), TransformObjectToWorldNormal(_normalOS));
}

void TransformHClipToUVDepth(float4 positionCS,out half2 uv,out half depth)
{
    positionCS.xyz /= positionCS.w;
    
    uv= (positionCS.xy + 1) * .5;
    #if UNITY_UV_STARTS_AT_TOP
    uv.y = 1 - uv.y;
    #endif
    
    depth = positionCS.z;
}

//Screen Space Calculations
float3 _FrustumCornersRayBL;
float3 _FrustumCornersRayBR;
float3 _FrustumCornersRayTL;
float3 _FrustumCornersRayTR;

float3 GetViewDirWS(float2 uv){return bilinearLerp(_FrustumCornersRayTL, _FrustumCornersRayTR, _FrustumCornersRayBL, _FrustumCornersRayBR, uv);}
float3 GetPositionWS_Frustum(half2 uv,half depth){ return _WorldSpaceCameraPos + LinearEyeDepth(depth,_ZBufferParams) *  GetViewDirWS(uv);}
float3 GetPositionWS_VP(half2 uv,half depth){ return TransformClipToWorld(TransformUVDepthToClipPosition(uv,depth));}

//Normals
float3 DecodeNormalMap(float3 _normal){return normalize(_normal * 2. - 1.);}

//Refer: @https://blog.selfshadow.com/publications/blending-in-detail/
half3 BlendNormal(half3 _normal1, half3 _normal2, uint _blendMode)
{
    half3 blendNormal=half3(0.h,0.h,1.h);
    [branch]switch (_blendMode)
    {
        default:blendNormal=0.h;break;
        case 0u://Linear
            {
                _normal1=DecodeNormalMap(_normal1);
                _normal2=DecodeNormalMap(_normal2);
                blendNormal= normalize(_normal1 + _normal2);                
            }
        break;
        case 1u://Overlay
            {
                blendNormal =Blend_Overlay(_normal1,_normal2);
                blendNormal= DecodeNormalMap(blendNormal);                
            }
        break;
        case 2u://Partial Derivative
            {
                _normal1=DecodeNormalMap(_normal1);
                _normal2=DecodeNormalMap(_normal2);
                half2 pd=_normal1.xy*_normal2.z+_normal2.xy*_normal1.z;
                blendNormal=half3(pd,_normal1.z*_normal2.z);
            }
        break;
        case 3u://Unreal Developer Network
            {
                _normal1=DecodeNormalMap(_normal1);
                _normal2=DecodeNormalMap(_normal2);
                //blendNormal=half3(_normal1.xy+_normal2.xy,_normal1.z*_normal2.z); //Whiteout
                blendNormal=half3(_normal1.xy+_normal2.xy,_normal1.z);
            }
        break;
        case 4u://Reoriented
            {
                half3 t=_normal1*half3(2.h,2.h,2.h)+half3(-1.h,-1.h,0);
                half3 u=_normal2*half3(-2.h,-2.h,2.h)+half3(1.h,1.h,-1.h);
                blendNormal=t*dot(t,u)-u*t.z;
            }
        break;
    }
    return normalize(blendNormal);
}

//UV Remapping
float2 UVRemap_Triplanar(float3 _positionWS, float3 _normalWS)
{
    return _positionWS.zy * _normalWS.x + _positionWS.xz * _normalWS.y + _positionWS.xy * _normalWS.z;
}

float2 UVRemap_TSR(float2 uv, float2 tilling, float2 offset, float rotateAngle)
{
    const float2 center = float2(.5, .5);
    uv = uv + offset;
    offset += center;
    float2 centerUV = uv - offset;
    return mul( Rotate2x2(rotateAngle), centerUV) * tilling + offset;
}

#include "Library/Noise.hlsl"