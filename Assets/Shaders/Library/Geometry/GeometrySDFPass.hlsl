
#define MAX_MARCH_STEPS 256
#define FLOAT_EPSILON 0.001
            #include "Assets/Shaders/Library/Lighting.hlsl"

bool RaymarchSDF(GRay ray,float start,float end,out float distance,out SDFOutput _output,int _marchSteps,float _tolerance)
{
    distance = start;
    for(int i=0;i<_marchSteps&&distance<end;i++)
    {
        _output=SceneSDF(ray.GetPoint(distance));
        float sdfDistance = _output.distance;
        if(sdfDistance < _tolerance)
            return true;
        distance+=sdfDistance;
    }
    return false;
}

bool RaymarchSDF(GRay ray,float start,float end,out float distance,out SDFOutput _output)
{
    return RaymarchSDF(ray,start,end,distance,_output,MAX_MARCH_STEPS,FLOAT_EPSILON);
}

#define MAX_SHADOW_STEPS 256
float RaymarchSDFShadow(float3 _position,float3 _lightPosition,float _bias = 0.1f)
{
    float3 direction = _lightPosition - _position;
    GRay shadowRay = GRay_Ctor(_position,normalize(direction));
    float maxMarchLength = length(direction);
    float distance= 0;
    SDFOutput _output;
    return RaymarchSDF(shadowRay,_bias,maxMarchLength,distance,_output,MAX_SHADOW_STEPS,FLOAT_EPSILON) ? 0 :1;
}

float RaymarchSDFSoftShadow(float3 _position,float3 _lightPosition,float _bias = 0.1f,float _softConstant = 32)
{
    float3 direction = _lightPosition - _position;
    GRay shadowRay = GRay_Ctor(_position,normalize(direction));
    float maxMarchLength = length(direction);
    
    float res = 1.0;
    float t = _bias;
    for( int i=0; i<MAX_SHADOW_STEPS && t<maxMarchLength; i++ )
    {
        SDFOutput h = SceneSDF(shadowRay.GetPoint(t));
        if( h.distance<0.001 )
            return 0.0;
        res = min( res, _softConstant*h.distance/t );
        t += h.distance;
    }
    return res;
}

float3 RaymarchSDFNormal(float3 marchPos)
{
    return normalize(float3(
        SceneSDF(float3(marchPos.x+FLOAT_EPSILON,marchPos.y,marchPos.z)).distance- SceneSDF(float3(marchPos.x-FLOAT_EPSILON,marchPos.y,marchPos.z)).distance,
        SceneSDF(float3(marchPos.x,marchPos.y+FLOAT_EPSILON,marchPos.z)).distance- SceneSDF(float3(marchPos.x,marchPos.y-FLOAT_EPSILON,marchPos.z)).distance,
        SceneSDF(float3(marchPos.x,marchPos.y,marchPos.z+FLOAT_EPSILON)).distance- SceneSDF(float3(marchPos.x,marchPos.y,marchPos.z-FLOAT_EPSILON)).distance
    ));
}
            

struct a2vSDF
{
    float3 positionOS : POSITION;
};

struct v2fSDF
{
    float4 positionCS:SV_POSITION;
    float4 positionHCS:TEXCOORD0;
};
v2fSDF vertSDF (a2vSDF v)
{
    v2fSDF o;
    o.positionCS = TransformObjectToHClip(v.positionOS);
    o.positionHCS=o.positionCS;
    return o;
}
            
float4 fragSDF (v2fSDF i) : SV_Target
{
    float2 positionNDC=TransformHClipToNDC(i.positionHCS);

    float3 viewDirWS=normalize(TransformNDCToFrustumCornersRay(positionNDC));
    GRay viewRay=GRay_Ctor(GetCameraPositionWS(),viewDirWS);
                
    SDFOutput output;
    float distance;
    if(!RaymarchSDF(viewRay,_ProjectionParams.y,_ProjectionParams.z,distance,output))
        return 0;
    viewDirWS=-viewDirWS;
    float3 positionWS=viewRay.GetPoint(distance);
    float3 normalWS=RaymarchSDFNormal(positionWS);
    float3 lightDirWS=normalize(_MainLightPosition.xyz);
    float3 halfDirWS=normalize(lightDirWS+viewDirWS);
    float NDL=saturate(dot(normalWS,lightDirWS));
    float NDV=saturate(dot(normalWS,viewDirWS));
    float NDH=saturate(dot(normalWS,halfDirWS));
    float3 albedo=output.color;
    float3 lightColor=_MainLightColor.rgb;
    float diffuse=saturate(NDL)*.5+.5;
    float3 color=albedo*diffuse*lightColor;

    float specular=pow(NDH,PI);
    color=IndirectDiffuse_SH(normalWS)+ lerp(color,specular*lightColor*albedo,specular);

    return float4(color,1);
}