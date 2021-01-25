#define PI 3.1415926535
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

float2 TriplanarMapping(float3 worldPos,float3 worldNormal){ return (worldPos.zy*worldNormal.x+worldPos.xz*worldNormal.y+worldPos.xy*worldNormal.z);}

float luminance(fixed3 color){ return 0.2125*color.r + 0.7154*color.g + 0.0721 + color.b;}

float4 BlendColor(float4 src,float4 dst){ return 1-(1-src)*(1-dst); }
float3 BlendColor(float3 src,float3 dst){ return 1-(1-src)*(1-dst); }

float invlerp(float a,float b,float value){ return (value-a)/(b-a); }

float remap (float value, float from1, float to1, float from2, float to2) {   return lerp(from2,to2, invlerp(from1,to1,value));  }

float random2(float2 value){return frac(sin(dot(value,float2(12.9898,78.233)))*43758.543123);}
float random3(float3 value){return frac(sin(dot(value,float3(12.9898,78.233,53.539)))*43758.543123);}

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
