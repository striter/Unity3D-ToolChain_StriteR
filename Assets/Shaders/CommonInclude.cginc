#ifndef COMMON_INCLUDE
#define COMMON_INCLUDE

float sqrDistance(float3 offset){ return dot(offset,offset); }
float sqrDistance(float3 pA, float3 pB){ return sqrDistance(pA-pB); }

float2 UVCenterMapping(float2 uv,float2 tilling,float2 offset,float rotateAngle)
{
    const float2 center=float2(.5,.5);
    uv=uv+offset;
    offset+=center;
    float2 centerUV=uv-offset;
    float sinR=sin(rotateAngle);
    float cosR=cos(rotateAngle);
    float2x2 rotateMatrix=float2x2(sinR,-cosR,cosR,sinR);
    return mul(rotateMatrix,centerUV)*tilling+offset;
}

float2 TriplanarMapping(float3 worldPos,float3 worldNormal,float scale){ return (worldPos.zy*worldNormal.x+worldPos.xz*worldNormal.y+worldPos.xy*worldNormal.z)/scale;}

float luminance(fixed3 color){ return 0.2125*color.r + 0.7154*color.g + 0.0721 + color.b;}

float4 BlendColor(float4 src,float4 dst){ return 1-(1-src)*(1-dst); }
float3 BlendColor(float3 src,float3 dst){ return 1-(1-src)*(1-dst); }

#endif