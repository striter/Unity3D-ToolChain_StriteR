#define PI 3.1415926535
float sqr(float value) { return value * value; }
float UnpackRoughness(float glossiness)
{
	float roughness=1-glossiness*glossiness;
	return roughness*roughness;
}
//NDF,Normal Distribution Function
float NDF_BlinnPhong(float NDH,float specularPower,float specularGloss)
{
	float distribution=pow(NDH,specularGloss)*specularPower;
	distribution *= (2+specularPower) / (2*PI);
	return distribution;
}
float NDF_Beckmann(float NDH,float roughness)
{	
	float sqrRoughness=dot(roughness,roughness);
	float sqrNDH=dot(NDH,NDH);
	return max(0.000001,(1.0/(PI*sqrRoughness*sqrNDH*sqrNDH))*exp((sqrNDH-1)/(sqrNDH*sqrRoughness)));
}
float NDF_Gaussian(float NDH,float roughness)
{
	float sqrRoughness=dot(roughness,roughness);
	float thetaH=acos(NDH);
	return exp(-thetaH*thetaH/sqrRoughness);
}
float NDF_GGX(float NDH,float roughness)
{
	float sqrRoughness = dot(roughness, roughness);
	float sqrNDH = dot(NDH, NDH);
	float tanSqrNDH = (1 - sqrNDH) / sqrNDH;
	return (1.0 / PI) * sqr(roughness / (sqrNDH * (sqrRoughness + tanSqrNDH)));
}
float NDF_TrowbridgeReitz(float NDH,float roughness)
{
	float sqrRoughness=dot(roughness,roughness);
	float sqrNDH=dot(NDH,NDH);
	float distribution=sqrNDH*(sqrRoughness-1.0)+1.0;
	return sqrRoughness/(PI*distribution*distribution);
}
//Anisotropic NDF
float NDFA_TrowbridgeReitz(float NDH, float HDX, float HDY, float anisotropic, float glossiness)
{
	float aspect = sqrt(1.0h - anisotropic * 0.9h);
	glossiness = sqr(1.0 - glossiness);
	float X = max(.001, glossiness / aspect) * 5;
	float Y = max(.001, glossiness * aspect) * 5;
	return 1.0 / (PI * X * Y * sqr(sqr(HDX / X) + sqr(HDY / Y) + sqr(NDH)));
}
float NDFA_Ward(float NDL, float NDV,float NDH, float HDX, float HDY, float anisotropic, float glossiness)
{
	float aspect = sqrt(1.0h - anisotropic * 0.9h);
	glossiness = sqr(1.0 - glossiness);
	float X = max(.001, glossiness / aspect) * 5;
	float Y = max(.001, glossiness * aspect) * 5;
	float distribution = 1.0 / (4.0 * PI * X * Y * sqrt(NDL * NDV));
	distribution *= exp(-(sqr(HDX/X)+ sqr(HDY/Y))/sqr(NDH));
	return distribution;
}

//GSF Geometric Shadowing Function
float GSF_Implicit(float NDL, float NDV) { return NDL * NDV; }
float GSF_AshikhminShirley(float NDL, float NDV, float LDH) { return NDL * NDV / (LDH * max(NDL, NDV)); }
float GSF_AshikhminPremoze(float NDL, float NDV) { return  NDL * NDV / (NDL + NDV - NDL * NDV); }
//Fresnel
float Fresnel_Schlick(float F0, float NDV) { return F0 + (1 - F0) * pow(1 - NDV, 5); }
float Fresnel_Schlick(float F0, float3 normal, float3 viewDir) { return Fresnel_Schlick(F0, dot(normal, viewDir)); }
