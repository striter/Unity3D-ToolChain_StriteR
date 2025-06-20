#define MAX_MARCH_STEPS 256
#define TOLERANCE 0.001
struct SDFHitInfo
{
    float distance;
    SDFSurface data;
};

bool RaymarchSDF(GRay ray,float start,float end,out SDFHitInfo _hitInfo,int _marchSteps,float _tolerance)
{
    _hitInfo.distance = start;
    for(int i=0;i<_marchSteps&&_hitInfo.distance<end;i++)
    {
        SDFSurface data = SceneSDF(ray.GetPoint(_hitInfo.distance));
        float sdfDistance = data.distance;
        if(sdfDistance < _tolerance)
        {
            _hitInfo.data = data;
            return true;
        }
        _hitInfo.distance+=sdfDistance;
    }
    return false;
}

bool RaymarchSDF(GRay ray,float start,float end,out SDFHitInfo _output)
{
    return RaymarchSDF(ray,start,end,_output,MAX_MARCH_STEPS,TOLERANCE);
}

#define MAX_SHADOW_STEPS 256
#define SHADOW_BIAS 0.1

float RaymarchSDFShadow(float3 _position,float3 _lightPosition,float _bias = SHADOW_BIAS)
{
    float3 direction = _lightPosition - _position;
    GRay shadowRay = GRay_Ctor(_position,normalize(direction));
    float maxMarchLength = length(direction);
    SDFHitInfo output;
    return RaymarchSDF(shadowRay,_bias,maxMarchLength,output,MAX_SHADOW_STEPS,TOLERANCE) ? 0 :1;
}

float RaymarchSDFSoftShadow(float3 _position,float3 _lightPosition,float _softConstant = .1f,float _bias = SHADOW_BIAS,int _maxSteps = MAX_SHADOW_STEPS)
{
    float3 direction = _lightPosition - _position;
    GRay shadowRay = GRay_Ctor(_position,normalize(direction));
    float maxMarchLength = length(direction);
    
    float res = 1.0;
    float t = _bias;
    for( int i=0; i<_maxSteps && t<maxMarchLength; i++ )
    {
        float h = SceneSDF(shadowRay.GetPoint(t)).distance;
        res = min( res, h/(_softConstant*t) );
        t += clamp(h, 0.005, 0.50);
        if( res<-1.0 || t>maxMarchLength ) break;
    }
    res = max(res,-1.0);
    return 0.25*(1.0+res)*(1.0+res)*(2.0-res);
}

float3 RaymarchSDFNormal(float3 marchPos)
{
    return normalize(float3(
        SceneSDF(float3(marchPos.x+TOLERANCE,marchPos.y,marchPos.z)).distance- SceneSDF(float3(marchPos.x-TOLERANCE,marchPos.y,marchPos.z)).distance,
        SceneSDF(float3(marchPos.x,marchPos.y+TOLERANCE,marchPos.z)).distance- SceneSDF(float3(marchPos.x,marchPos.y-TOLERANCE,marchPos.z)).distance,
        SceneSDF(float3(marchPos.x,marchPos.y,marchPos.z+TOLERANCE)).distance- SceneSDF(float3(marchPos.x,marchPos.y,marchPos.z-TOLERANCE)).distance
    ));
}