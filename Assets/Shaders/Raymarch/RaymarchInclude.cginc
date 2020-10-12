#ifndef RARYMARCH_INCLUDE
#define RARYMARCH_INCLUDE
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

#endif