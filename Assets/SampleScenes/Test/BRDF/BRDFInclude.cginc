#define PI 3.1415926535
float UnpackSmoothness(float glossiness)
{
	float roughness=1-glossiness*glossiness;
	return roughness*roughness;
}

//Fresnel
	//Schlick
	float Fresnel_Schlick(float F0,float NDV){ return F0+(1-F0)*pow(1-NDV,5);}
	float Fresnel_Schlick(float F0,float3 normal,float3 viewDir) { return Fresnel_Schlick(F0,dot(normal,viewDir)); }

//Normal Distribution Function
	float NDF_BlinnPhong(float NDH,float specularPower,float specularGloss)
	{
		float distribution=pow(NDH,specularGloss)*specularPower;
		distribution *= (2+specularPower) / (2*3.1415926535);
		return distribution;
	}
