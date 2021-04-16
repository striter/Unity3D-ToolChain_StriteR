#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
float2 TransformTex(float2 uv, float4 st) {return uv * st.xy + st.zw;}
#define PI_HALF 1.5707963267949
#define INSTANCING_BUFFER_START UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
#define INSTANCING_PROP(type,param) UNITY_DEFINE_INSTANCED_PROP(type,param)
#define INSTANCING_BUFFER_END UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)
#define INSTANCE(param) UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,param)
#define TRANSFORM_TEX_INSTANCE(uv,tex) TransformTex(uv,INSTANCE(tex##_ST));

float3 TransformObjectToHClipNormal(float3 normalOS){  return mul((float3x3) GetWorldToHClipMatrix(), TransformObjectToWorldNormal(normalOS));}

float sqrDistance(float3 offset){ return dot(offset,offset); }
float sqrDistance(float3 pA, float3 pB){ return sqrDistance(pA-pB); }


float4 Blend_Screen(float4 src,float4 dst){ return 1-(1-src)*(1-dst); }
float3 Blend_Screen(float3 src,float3 dst){ return 1-(1-src)*(1-dst); }
float3 Blend_Alpha(float3 src, float3 dst,float srcAlpha){return src * srcAlpha + dst * (1 - srcAlpha);}

float invlerp(float a,float b,float value){ return (value-a)/(b-a); }
float quinterp(float f){ return f * f * f * (f * (f*6-15)+10); }
float remap (float value, float from1, float to1, float from2, float to2) {   return lerp(from2,to2, invlerp(from1,to1,value));  }
float max(float max1, float max2, float max3, float max4)
{
    float final = max(max1, max2);
    final = max(final, max3);
    final = max(final, max4);
    return final;
}

float2 TriplanarMapping(float3 worldPos,float3 worldNormal){ return (worldPos.zy*worldNormal.x+worldPos.xz*worldNormal.y+worldPos.xy*worldNormal.z);}
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
    return float2x2(cosAngle,-sinAngle,sinAngle,cosAngle);
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

float random01(float value){ return frac(sin(value*12.9898) * 43758.543123);}
float random01(float2 value){return frac(sin(dot(value,float2(12.9898,78.233)))*43758.543123);}
float random01(float3 value){return frac(sin(dot(value,float3(12.9898,78.233,53.539)))*43758.543123);}

float randomUnit(float value){return random01(value) * 2 - 1;}
float randomUnit(float2 value){ return random01(value)*2-1;}
float randomUnit(float3 value){return random01(value)*2-1;}
float2 randomUnitQuad(float2 value){return float2(randomUnit(value.xy), randomUnit(value.yx));}
float2 randomUnitCircle(float2 value)
{
    float theta = 2 * PI * random01(value);
    return float2(cos(theta), sin(theta));
}

float random01Perlin(float2 value)
{
    float2 pos00 = floor(value);
    float2 pos10 = pos00 + float2(1.0f, 0.0f);
    float2 pos01 = pos00 + float2(0.0f, 1.0f);
    float2 pos11 = pos00 + float2(1.0f, 1.0f);

    float2 rand00 = randomUnitCircle(pos00);
    float2 rand10 = randomUnitCircle(pos10);
    float2 rand01 = randomUnitCircle(pos01);
    float2 rand11 = randomUnitCircle(pos11);
    
    float dot00 = dot(rand00, pos00 - value);
    float dot01 = dot(rand01, pos01 - value);
    float dot10 = dot(rand10, pos10 - value);
    float dot11 = dot(rand11, pos11 - value);
    
    float2 d = frac(value);
    float interpolate = quinterp(d.x);
    float x1 = lerp(dot00, dot10, interpolate);
    float x2 = lerp(dot01, dot11, interpolate);
    return lerp(x1, x2, quinterp(d.y));
}

float randomUnitPerlin(float2 value){ return random01Perlin(value)*2-1;}