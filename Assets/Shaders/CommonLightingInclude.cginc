#ifndef COMMONLIGHTING_INCLUDE
#define COMMONLIGHTING_INCLUDE

//Diffuse-Lambert
float GetDiffuse(float3 normal,float3 lightDir)
{
	return  dot(normal, lightDir);
}

float GetDiffuse(float3 normal, float3 lightDir,float lambert,float atten)
{
	float diffuse= saturate(GetDiffuse(normal, lightDir));
	diffuse*=atten;
	return lambert+(1-lambert)*diffuse;
}

//Blinn-Phong Specular Optimized, range 0.9-1
float GetSpecular(float3 normal,float3 halfDir,float range)
{
	float specular = dot(normal,halfDir);
	return smoothstep(range, 1, specular);
}

float GetSpecular(float3 normal,float3 lightDir,float3 viewDir,float range)
{
	return GetSpecular(normal,normalize(lightDir+viewDir),range);
}


//Kajiya-Kay Anisotropic Lighting Model
float StrandSpecular(float3 T,float3 N,float3 H,float exponent,float3 shift)
{
    T=normalize(T+shift*N);
    float dotTH=dot(T,H);
    float sinTH=sqrt(1.0-dotTH*dotTH);
    float dirAtten=smoothstep(-1,0,dotTH);
    return dirAtten*pow(sinTH,exponent);
}

float3 DecodeNormalMap(float3 normal)
{
	return normal*2-1;
}
#endif