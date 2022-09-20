// #pragma shader_feature _CLOUDSHADOW
float SampleCloudShadow(TEXTURE2D_PARAM(shadowTex,sampler_ShadowTex),float4 _param1,float4 _param2,float3 _lightDirWS,float3 _positionWS)
{
    float NDL=dot(_lightDirWS,float3(0,1,0));
    float distance=(_param1.z-_positionWS.y)*rcp(NDL);
    float3 projectPosition=_positionWS+_lightDirWS*distance;
    float2 uv=projectPosition.xz;
    float2 cloudUV=(uv+_param2.zw*_Time.y)/_param1.y;
    float cloudSample= SAMPLE_TEXTURE2D_LOD(shadowTex,sampler_ShadowTex,cloudUV,0).r;
    cloudSample=smoothstep(_param2.x,_param2.y,cloudSample);
    cloudSample=lerp(_param1.x,1,cloudSample);
    return cloudSample;
}

TEXTURE2D(_CloudShadowTexture);SAMPLER(sampler_CloudShadowTexture);
float4 _CloudParam1;        //x strength y scale z planeDistance
float4 _CloudParam2;        //xy SmoothStep zw Flow
TEXTURE2D(_CloudShadowTexture_Interpolate);SAMPLER(sampler_CloudShadowTexture_Interpolate);
float4 _CloudParam1_Interpolate;
float4 _CloudParam2_Interpolate;

float CloudShadowAttenuation(float3 _positionWS,float3 _lightDirWS)
{
    #if defined(_CLOUDSHADOW)
        #if ENVIRONMENT_INTERPOLATE
            return lerp(SampleCloudShadow(TEXTURE2D_ARGS(_CloudShadowTexture,sampler_CloudShadowTexture),_CloudParam1,_CloudParam2,_lightDirWS,_positionWS),
                SampleCloudShadow(TEXTURE2D_ARGS(_CloudShadowTexture_Interpolate,sampler_CloudShadowTexture_Interpolate),_CloudParam1_Interpolate,_CloudParam2_Interpolate,_lightDirWS,_positionWS),
                _EnvironmentInterpolate);
        #endif
        
        return SampleCloudShadow(TEXTURE2D_ARGS(_CloudShadowTexture,sampler_CloudShadowTexture),_CloudParam1,_CloudParam2,_lightDirWS,_positionWS);
    #endif

    return 1;
}
