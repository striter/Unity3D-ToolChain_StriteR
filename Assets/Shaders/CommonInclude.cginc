#ifndef COMMON_INCLUDE
#define COMMON_INCLUDE

float sqrDistance(float3 offset){ return dot(offset,offset); }
float sqrDistance(float3 pA, float3 pB){ return sqrDistance(pA-pB); }

//Apply Scale/Offset With Rotation , center:float2(.5,.5)
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

float luminance(fixed3 color){ return 0.2125*color.r + 0.7154*color.g + 0.0721 + color.b;}

float4 BlendColor(float4 src,float4 dst){ return 1-(1-src)*(1-dst); }
float3 BlendColor(float3 src,float3 dst){ return 1-(1-src)*(1-dst); }

float min(float3 target){ return min( min(target.x,target.y),target.z);}
float max(float3 target){ return max( max(target.x,target.y),target.z);}

bool AABBRayHitted(float3 boundsMin,float3 boundsMax,float3 rayOrigin,float3 rayDir)
{
    float3 invRayDir=1/rayDir;
    float3 t0=(boundsMin-rayOrigin)*invRayDir;
    float3 t1=(boundsMax-rayOrigin)*invRayDir;
    float3 tmin=min(t0,t1);
    float3 tmax=max(t0,t1);
    return max(tmin)<=min(tmax);
}

//return X: Dst To Box , Y:Dst In Side Box
float2 AABBRayDistance(float3 boundsMin,float3 boundsMax,float3 rayOrigin,float3 rayDir)
{
	float3 invRayDir=1/rayDir;
	float3 t0=(boundsMin-rayOrigin)*invRayDir;
	float3 t1=(boundsMax-rayOrigin)*invRayDir;
	float3 tmin=min(t0,t1);
	float3 tmax=max(t0,t1);

	float dstA=max(max(tmin.x,tmin.y),tmin.z);
	float dstB=min(tmax.x,min(tmax.y,tmax.z));

	float dstToBox=max(0,dstA);
	float dstInsideBox=max(0,dstB-dstToBox);
	return float2(dstToBox,dstInsideBox);
}


float2 BSRayDistance(float3 _bsCenter, float _bsRadius, float3 _rayOrigin, float3 _rayDirection)
{
    float3 offset = _rayOrigin - _bsCenter;
    float dotOffsetDirection = dot(_rayDirection, offset);
    if (dotOffsetDirection > 0)
        return -1;

    float dotOffset = dot(offset, offset);
    float sqrRadius = _bsRadius * _bsRadius;
    float discriminant = dotOffsetDirection * dotOffsetDirection - dotOffset + sqrRadius;
    if (discriminant < 0)
        return -1;
    else if (discriminant < 0.00001)
        float2(-dotOffsetDirection, -dotOffsetDirection);
    else
    {
        discriminant = sqrt(discriminant);
        float t0 = -dotOffsetDirection - discriminant;
        float t1 = -dotOffsetDirection + discriminant;
        if (t0 < 0)
            t0 = t1;
        return float2(t0, t1);
    }
    return -1;
}

bool PosInsideBox(float3 boundsMin, float3 boundsMax, float3 pos)
{
	return boundsMin.x <= pos.x && pos.x <= boundsMax.x&& boundsMin.y <= pos.y && pos.y <= boundsMax.y && boundsMin.z <= pos.z && pos.z <= boundsMax.z;
}

#endif