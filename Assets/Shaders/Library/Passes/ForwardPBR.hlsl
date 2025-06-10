#include "Assets/Shaders/Library/Additional/Local/AlphaClip.hlsl"
#include "Assets/Shaders/Library/PBR/BRDFInput.hlsl"
#include "Assets/Shaders/Library/PBR/BRDFLighting.hlsl"

v2ff ForwardVertex(a2vf v)
{
	v2ff o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	o.uv = TRANSFORM_TEX_INSTANCE(v.uv,_MainTex);
	o.normalWS = TransformObjectToWorldNormal(v.normalOS);
	o.tangentWS = TransformObjectToWorldDir(v.tangentOS.xyz);
	o.biTangentWS = cross(o.normalWS,o.tangentWS)*v.tangentOS.w;
//Positions
#if defined(GET_POSITION_WS)
	float3 positionWS = GET_POSITION_WS(v,o);
#else
	float3 positionWS=TransformObjectToWorld(v.positionOS);
#endif
	o.positionWS = positionWS;
	o.positionCS = TransformWorldToHClip(positionWS);
	o.viewDirWS = GetViewDirectionWS(o.positionWS);
	o.positionHCS = o.positionCS;
	o.color=v.color;
	LIGHTMAP_TRANSFER(v,o)
	FOG_TRANSFER(o)
	#if defined(V2F_ADDITIONAL_TRANSFER)
		V2F_ADDITIONAL_TRANSFER(v,o)
	#endif
	return o;
}

BRDFInitializeInput InitializeFragmentInput(v2ff i)
{
	BRDFInitializeInput input;
	ZERO_INITIALIZE(BRDFInitializeInput,input);
	input.uv = i.uv;
	input.color = i.color;
	input.positionWS = i.positionWS;
	input.normalWS = normalize(i.normalWS);
	input.viewDirWS = normalize(i.viewDirWS);
	input.tangentWS = normalize(i.tangentWS);
	input.biTangentWS = normalize(i.biTangentWS);
	input.TBNWS = half3x3(i.tangentWS,i.biTangentWS,i.normalWS);
	input.normalTS = half3(0,0,1);
	return input;
}

BRDFSurface InitializeFragmentSurface(BRDFInitializeInput input)
{
	BRDFSurface surface;
	ZERO_INITIALIZE(BRDFSurface, surface)
	
	half4 albedoAlpha = 
#if defined(GET_ALBEDO)
	GET_ALBEDO(input);
#else
	SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv)*INSTANCE(_Color);
#endif
	
#if !defined (_NORMALOFF)
	#if defined(GET_NORMAL)
		input.normalTS = GET_NORMAL(input);
	#else
		input.normalTS = DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,input.uv));
	#endif
	input.normalWS = normalize(mul(transpose(input.TBNWS), input.normalTS));
#endif
	
	half3 emission =0;
#if !defined(_EMISSIONOFF)
	#if defined(GET_EMISSION)
		emission = GET_EMISSION(input); 
	#else
		emission = SAMPLE_TEXTURE2D(_EmissionTex,sampler_EmissionTex,input.uv).rgb*INSTANCE(_EmissionColor).rgb;
	#endif
#endif

float smoothness=0.5,metallic=0,ao =1;
#if !defined(_PBROFF)
	#if defined(GET_PBRPARAM)
		GET_PBRPARAM(input,smoothness,metallic,ao);
	#else
		half3 mix=SAMPLE_TEXTURE2D(_PBRTex,sampler_PBRTex,input.uv).rgb;
		smoothness=mix.r;
		metallic=mix.g;
		ao=mix.b;
	#endif
#endif
    
	half oneMinusReflectivity = DIELETRIC_SPEC.a - metallic * DIELETRIC_SPEC.a;
	half reflectivity = 1.0h - oneMinusReflectivity;

	half3 albedo = albedoAlpha.rgb;
	half alpha = albedoAlpha.a;
	
	surface.alpha = alpha;
	surface.diffuse = albedo * oneMinusReflectivity;
	surface.emission = emission;
	surface.specular = lerp(DIELETRIC_SPEC.rgb, albedo, metallic);
	surface.metallic = metallic;
	surface.ao = ao;
    
	surface.normal = input.normalWS;
	surface.tangent = input.tangentWS;
	surface.biTangent = input.biTangentWS;
	surface.viewDir = input.viewDirWS;
	surface.reflectDir = normalize(reflect(-surface.viewDir, surface.normal));
	surface.NDV = dot(surface.normal,surface.viewDir);
	surface.TDV = dot(surface.tangent,surface.viewDir);
	surface.BDV = dot(surface.biTangent,surface.viewDir);
	
	surface.smoothness = smoothness;
	surface.grazingTerm = saturate(smoothness + reflectivity);
	surface.perceptualRoughness = 1.0h - smoothness;
	surface.roughness = max(HALF_MIN_SQRT, surface.perceptualRoughness * surface.perceptualRoughness);
	surface.roughness2 = max(HALF_MIN, surface.roughness * surface.roughness);
    
	surface.normalTS = input.normalTS;
    
	#if defined(BRDF_SURFACE_ADDITIONAL_TRANSFER)
		BRDF_SURFACE_ADDITIONAL_TRANSFER(input,surface);
	#endif
	
	return surface;
}

half3 BRDFLighting(BRDFSurface surface, Light light)
{
	#if defined(GET_LIGHTING_OUTPUT)
		return GET_LIGHTING_OUTPUT(surface,light);
	#else
		BRDFLightInput input=BRDFLightInput_Ctor(surface,light.direction,light.color,light.shadowAttenuation,light.distanceAttenuation);
		BRDFLight brdfLight=BRDFLight_Ctor(surface,input);
		return BRDFLighting(surface,brdfLight);
	#endif
}

half3 PBRGlobalIllumination(v2ff i,BRDFSurface surface,Light mainLight)
{
	#if defined(GET_GI)
		return GET_GI(i,surface,mainLight)
	#else
		half3 indirectDiffuse = IndirectDiffuse(mainLight, i, surface.normal);
		half3 indirectSpecular = IndirectSpecular(surface.reflectDir, surface.perceptualRoughness, 0);
		return BRDFGlobalIllumination(surface,indirectDiffuse,indirectSpecular);
	#endif
}

f2of ForwardFragment(v2ff i)
{
	UNITY_SETUP_INSTANCE_ID(i);
	f2of o;
	ZERO_INITIALIZE(f2of,o);
	BRDFInitializeInput input = InitializeFragmentInput(i);
#if defined(BRDF_SURFACE_INITIALIZE_ADDITIONAL_TRANSFER)
	BRDF_SURFACE_INITIALIZE_ADDITIONAL_TRANSFER(i,input,o)
#endif
	
	BRDFSurface surface = InitializeFragmentSurface(input);
	float3 positionWS = i.positionWS;
	
	half3 finalCol=0;

	Light mainLight =
	#if defined GET_MAINLIGHT
		GET_MAINLIGHT(i);
	#else
		GetMainLight(TransformWorldToShadowCoord(positionWS),positionWS,unity_ProbesOcclusion);
	#endif
	
	finalCol += PBRGlobalIllumination(i,surface,mainLight);

	#if defined BRDF_MAINLIGHTING
		BRDF_MAINLIGHTING(mainLight,surface);
	#endif
	finalCol+=BRDFLighting(surface,mainLight);

	
	#if _ADDITIONAL_LIGHTS
		uint pixelLightCount = GetAdditionalLightsCount();
		for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
		finalCol+=BRDFLighting(surface, GetAdditionalLight(lightIndex,i.positionWS));
	#endif
	FOG_MIX(i,finalCol);
	finalCol+=surface.emission;

	AlphaClip(surface.alpha);
	o.result = float4(finalCol,surface.alpha);
	
	// BRDFLightInput lightInput=BRDFLightInput_Ctor(surface,mainLight.direction,mainLight.color,mainLight.shadowAttenuation,mainLight.distanceAttenuation);
	// BRDFLight brdfLight=BRDFLight_Ctor(surface,lightInput);
	// o.result = brdfLight.normalDistribution;
	// o.result =  float4(BRDFLighting(surface,brdfLight),1);
	
	#if defined(F2O_TRANSFER)
		F2O_TRANSFER(surface,o)
	#endif
	return o;
}
