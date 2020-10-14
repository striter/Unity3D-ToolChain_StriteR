#ifndef COMMON_INCLUDE
#define COMMON_INCLUDE

float sqrdistance(float3 pA, float3 pB)
{
	float3 offset = pA - pB;
	return dot(offset, offset);
}


fixed luminance(fixed3 color)
{
	return 0.2125*color.r + 0.7154*color.g + 0.0721 + color.b;
}

//return X: Dst To Box , Y:Dst In Side Box
float2 RayBoxDistance(float3 boundsMin,float3 boundsMax,float3 rayOrigin,float3 rayDir)	
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

bool PosInsideBox(float3 boundsMin, float3 boundsMax, float3 pos)
{
	return boundsMin.x <= pos.x && pos.x <= boundsMax.x&& boundsMin.y <= pos.y && pos.y <= boundsMax.y && boundsMin.z <= pos.z && pos.z <= boundsMax.z;
}

#endif