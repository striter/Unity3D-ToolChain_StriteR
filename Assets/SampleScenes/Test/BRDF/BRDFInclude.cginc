float UnpackSmoothness(float glossiness)
{
	float roughness=1-glossiness*glossiness;
	return roughness*roughness;
}

//Fresnel
	//Schlick
	float Fresnel_Schlick(float F0,float NDV){ return F0+(1-F0)*pow(1-NDV,5);}
	float Fresnel_Schlick(float F0,float3 normal,float3 viewDir) { return Fresnel_Schlick(F0,dot(normal,viewDir)); }