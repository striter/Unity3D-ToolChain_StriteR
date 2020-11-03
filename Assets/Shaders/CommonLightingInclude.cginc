#ifndef COMMONLIGHTING_INCLUDE
#define COMMONLIGHTING_INCLUDE

float GetDiffuse(float3 normal,float3 lightDir)
{
	return  dot(normal, lightDir);
}

float GetDiffuse(float3 normal, float3 lightDir,float lambert)
{
	float diffuse= GetDiffuse(normal, lightDir);
	return lambert*diffuse+(1-lambert);
}

//range 0.9-1
float GetSpecular(float3 normal,float3 lightDir,float3 viewDir,float range)
{
	float specular = dot(normalize(normal), normalize(viewDir + lightDir));
	specular = smoothstep(range, 1, specular);
	return specular;
}
#endif