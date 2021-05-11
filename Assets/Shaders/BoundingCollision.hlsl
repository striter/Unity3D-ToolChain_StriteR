float min(float3 target){ return min( min(target.x,target.y),target.z);}
float max(float3 target){ return max( max(target.x,target.y),target.z);}

//Plane
float PRayDistance(float3 _pNormal,float _pDistance,float3 _rayOrigin,float3 _rayDirection) //return: Dst To Plane
{
    float nrO = dot(_pNormal,_rayOrigin);
    float nrD = dot(_pNormal ,_rayDirection);
    return (_pDistance-nrO)/nrD;
}
float PRayDistance(float3 _pNormal,float3 _pPosition,float3 _rOrigin,float3 _rDirection)
{
    float nrO = dot(_pNormal,_pPosition-_rOrigin);
    float nrD = dot(_pNormal, _rDirection);
    return nrD / nrO;
}
//Axis Aligned Bounding Box
bool AABBPositionInside(float3 boundsMin, float3 boundsMax, float3 pos)
{
	return boundsMin.x <= pos.x && pos.x <= boundsMax.x&& boundsMin.y <= pos.y && pos.y <= boundsMax.y && boundsMin.z <= pos.z && pos.z <= boundsMax.z;
}

bool AABBRayIntersect(float3 boundsMin,float3 boundsMax,float3 rayOrigin,float3 rayDir)
{
    float3 invRayDir=1/rayDir;
    float3 t0=(boundsMin-rayOrigin)*invRayDir;
    float3 t1=(boundsMax-rayOrigin)*invRayDir;
    float3 tmin=min(t0,t1);
    float3 tmax=max(t0,t1);
    return max(tmin)<=min(tmax);
}

float2 AABBRayDistance(float3 boundsMin,float3 boundsMax,float3 rayOrigin,float3 rayDir)    //X: Dst To Box , Y:Dst In Side Box
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

//Bounding Sphere 
float2 BSRayDistance(float3 _bsCenter, float _bsRadius, float3 _rayOrigin, float3 _rayDirection)    //X: Dst To Sphere , Y:Dst In Side Sphere
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
    discriminant = sqrt(discriminant);
    float t0 = -dotOffsetDirection - discriminant;
    float t1 = -dotOffsetDirection + discriminant;
    if (t0 < 0)
        t0 = t1;
    return float2(t0, t1);
}