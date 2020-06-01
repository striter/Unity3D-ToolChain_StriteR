#ifndef COMMONLIGHTING_INCLUDE
#define COMMONLIGHTING_INCLUDE

float _Lambert;
float4 _ShadowColor;
float GetDiffuse(float3 normal, float3 lightDir)
{
	return saturate(dot(normalize(normal), normalize(lightDir)));
}

float3 GetDiffuseBaseColor(float3 albedo, float3 ambient, float3 lightCol, float atten, float diffuse)
{
	atten = atten * _Lambert + (1 - _Lambert);
	float3 diffuseCol = albedo * diffuse*lightCol;
	float3 ambientCol = albedo * ambient;
	return ambientCol+diffuseCol*atten+(1-atten)*_ShadowColor;
}

float3 GetDiffuseAddColor(float3 lightCol,float atten,float diffuse)
{
	return lightCol * diffuse*atten;
}
#endif