#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Library/ValueMapping.hlsl"
float2 TransformTex(float2 uv, float4 st) {return uv * st.xy + st.zw;}
#define PI_HALF 1.5707963267949
#define PI_TWO 6.2831853071796
#define PI_ONEDIVIDE 0.31830988618379
#define PI_ONEDIVDETWO 0.15915494309189
#define INSTANCING_BUFFER_START UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
#define INSTANCING_PROP(type,param) UNITY_DEFINE_INSTANCED_PROP(type,param)
#define INSTANCING_BUFFER_END UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)
#define INSTANCE(param) UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,param)
#define TRANSFORM_TEX_INSTANCE(uv,tex) TransformTex(uv,INSTANCE(tex##_ST));

#if UNITY_REVERSED_Z
#define Z_Multiply -1.h
#else
#define Z_Multiply 1.h
#endif

float3 TransformObjectToHClipNormal(float3 _normalOS)
{
    return mul((float3x3) GetWorldToHClipMatrix(), TransformObjectToWorldNormal(_normalOS));
}

float sqrDistance(float3 _offset)
{
    return dot(_offset, _offset);
}
float sqrDistance(float3 _pA, float3 _pB)
{
    return sqrDistance(_pA - _pB);
}

half Blend_Overlay(half _src,half _dst)
{
    return _src<.5h?2.h*_src*_dst:1.h-2.h*(1.h-_src)*(1.h-_dst);
}
half3 Blend_Overlay(half3 _src,half3 _dst)
{
    return half3(Blend_Overlay(_src.x,_dst.x),Blend_Overlay(_src.y,_dst.y),Blend_Overlay(_src.z,_dst.z));
}
float4 Blend_Screen(float4 _src, float4 _dst)
{
    return 1 - (1 - _src) * (1 - _dst);
}
float3 Blend_Screen(float3 _src, float3 _dst)
{
    return 1 - (1 - _src) * (1 - _dst);
}

float3 DecodeNormalMap(float3 _normal)
{
    return normalize(_normal * 2. - 1.);
}

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

float2 TriplanarMapping(float3 worldPos, float3 worldNormal)
{
    return worldPos.zy * worldNormal.x + worldPos.xz * worldNormal.y + worldPos.xy * worldNormal.z;
}

float2 UVCenterMapping(float2 uv, float2 tilling, float2 offset, float rotateAngle)
{
    const float2 center = float2(.5, .5);
    uv = uv + offset;
    offset += center;
    float2 centerUV = uv - offset;
    float sinR = sin(rotateAngle);
    float cosR = cos(rotateAngle);
    float2x2 rotateMatrix = float2x2(sinR, -cosR, cosR, sinR);
    return mul(rotateMatrix, centerUV) * tilling + offset;
}

float2x2 Rotate2x2(float angle)
{
    float sinAngle, cosAngle;
    sincos(angle, sinAngle, cosAngle);
    return float2x2(cosAngle, -sinAngle, sinAngle, cosAngle);
}

float3x3 AngleAxis3x3(float angle, float3 axis)
{
    float s, c;
    sincos(angle, s, c);

    float t = 1 - c;
    float x = axis.x;
    float y = axis.y;
    float z = axis.z;

    return float3x3(t * x * x + c, t * x * y - s * z, t * x * z + s * y,
        t * x * y + s * z, t * y * y + c, t * y * z - s * x,
        t * x * z - s * y, t * y * z + s * x, t * z * z + c);
}

float3 _FrustumCornersRayBL;
float3 _FrustumCornersRayBR;
float3 _FrustumCornersRayTL;
float3 _FrustumCornersRayTR;

float3 GetViewDirWS(float2 uv)
{
    return bilinearLerp(_FrustumCornersRayTL, _FrustumCornersRayTR, _FrustumCornersRayBL, _FrustumCornersRayBR, uv);
}

#include "Library/Noise.hlsl"