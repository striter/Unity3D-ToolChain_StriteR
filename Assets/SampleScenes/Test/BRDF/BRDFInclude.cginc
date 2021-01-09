#define PI 3.1415926535
float UnpackRoughness(float glossiness)
{
	float roughness=1-glossiness*glossiness;
	return roughness*roughness;
}

//Fresnel
float Fresnel_Schlick(float F0,float NDV){ return F0+(1-F0)*pow(1-NDV,5);}
float Fresnel_Schlick(float F0,float3 normal,float3 viewDir) { return Fresnel_Schlick(F0,dot(normal,viewDir)); }

//Normal Distribution Function
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
	float sqrRoughness=dot(roughness,roughness);
	float sqrNDH=dot(NDH,NDH);
	float tanSqrNDH=(1-sqrNDH)/sqrNDH;
	return (1.0/PI)*sqrt(roughness/(sqrNDH*(sqrRoughness+tanSqrNDH)));
}
float NDF_TrowbridgeReitz(float NDH,float roughness)
{
	float sqrRoughness=dot(roughness,roughness);
	float sqrNDH=dot(NDH,NDH);
	float distribution=sqrNDH*(sqrRoughness-1.0)+1.0;
	return sqrRoughness/(PI*distribution*distribution);
}

