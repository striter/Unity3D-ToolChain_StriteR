
#define MAX_MARCH_STEPS 128
#define FLOAT_EPSILON 0.00001

bool RaymarchSDF(GRay ray,float start,float end,out float distance,out SDFOutput _output)
{
    distance=start;
    for(int i=0;i<MAX_MARCH_STEPS;i++)
    {
        _output=SceneSDF(ray.GetPoint(distance));
        float sdfDistance=_output.distance;
        if(sdfDistance < FLOAT_EPSILON)
            return true;
        distance+=sdfDistance;
        if(distance>=end)
            break;
    }
    return false;
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

    float3 viewDirWS=normalize(TransformNDCToViewDirWS(positionNDC));
    GRay viewRay=GRay_Ctor(GetCameraPositionWS(),viewDirWS);
                
    SDFOutput  output;
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
    color=lerp(color,specular*lightColor*albedo,specular);

    return float4(color,1);
}