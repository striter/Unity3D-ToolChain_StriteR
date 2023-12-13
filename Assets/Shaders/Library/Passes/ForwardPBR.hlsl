
v2ff ForwardVertex(a2vf v)
{
	v2ff o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	o.uv = TRANSFORM_TEX_INSTANCE(v.uv,_MainTex);
	o.normalWS = TransformObjectToWorldNormal(v.normalOS);
	#if !defined (_NORMALOFF)	//TBN Matrix
		o.tangentWS = TransformObjectToWorldDir(v.tangentOS.xyz);
		o.biTangentWS = cross(o.normalWS,o.tangentWS)*v.tangentOS.w;
	#endif
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

half3 CustomGlobalIllumination(BRDFSurface surface,
	half3 bakedGI, half occlusion, float3 positionWS,
	half3 normalWS, half3 viewDirectionWS, float2 normalizedScreenSpaceUV)
{
	half3 reflectVector = reflect(-viewDirectionWS, normalWS);
	half NoV = saturate(dot(normalWS, viewDirectionWS));
	half fresnelTerm = Pow4(1.0 - NoV);

	half3 indirectDiffuse = bakedGI;
	half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, positionWS, surface.perceptualRoughness, 1.0h, normalizedScreenSpaceUV);

	half3 color =  indirectDiffuse * surface.diffuse;
	float surfaceReduction = 1.0 / (surface.roughness2 + 1.0);
	half3 brdfSpecular = half3(surfaceReduction * lerp(surface.specular, surface.grazingTerm, fresnelTerm));
	color += indirectSpecular *brdfSpecular;

	if (IsOnlyAOLightingFeatureEnabled())
		color = half3(1,1,1); // "Base white" for AO debug lighting mode

	return color * occlusion;
}

f2of ForwardFragment(v2ff i)
{
	f2of o;
	
	UNITY_SETUP_INSTANCE_ID(i);
	#if defined(FRAGMENT_SETUP)
		FRAGMENT_SETUP(i)
	#endif

	float3 positionWS=i.positionWS;
	float3 normalWS=normalize(i.normalWS);
	float3 viewDirWS=normalize(i.viewDirWS);
	half3 normalTS=half3(0,0,1);
	#if defined(GET_ALBEDO)
		half3 albedo = GET_ALBEDO(i);
	#else
		half3 albedo = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).rgb*INSTANCE(_Color).rgb;
	#endif
	float2 baseUV=i.uv.xy;

	half3 tangentWS = 0;
	half3 biTangentWS = 0;
	#if !defined (_NORMALOFF)
		tangentWS=normalize(i.tangentWS);
		biTangentWS=normalize(i.biTangentWS);
		#if defined(GET_NORMAL)
			normalTS = GET_NORMAL(i);
		#else
			normalTS=DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,baseUV));
		#endif
		float3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
		normalWS=normalize(mul(transpose(TBNWS), normalTS));
	#endif
	
	half3 emission =0;
	#if !defined(_EMISSIONOFF)
		#if defined(GET_EMISSION)
			emission = GET_EMISSION(i); 
		#else
			emission = SAMPLE_TEXTURE2D(_EmissionTex,sampler_EmissionTex,i.uv).rgb*INSTANCE(_EmissionColor).rgb;
		#endif
	#endif


	half smoothness=0.5,metallic=0,ao =1;

	#if !defined(_PBROFF)
		#if defined(GET_PBRPARAM)
			GET_PBRPARAM(i,smoothness,metallic,ao);
		#else
			half3 mix=SAMPLE_TEXTURE2D(_PBRTex,sampler_PBRTex,baseUV).rgb;
			smoothness=mix.r;
			metallic=mix.g;
			ao=mix.b;
		#endif
	#endif
	
	BRDFSurface surface=BRDFSurface_Ctor(albedo,emission,smoothness,metallic,ao,normalWS,tangentWS,biTangentWS,viewDirWS,1);

	#if defined(BRDFSURFACE_OVERRIDE)
		BRDFSURFACE_OVERRIDE(i,surface);
	#endif
	
	half3 finalCol=0;

	Light mainLight =
	#if defined GET_MAINLIGHT
		GET_MAINLIGHT(i);
	#else
		GetMainLight(TransformWorldToShadowCoord(positionWS),positionWS,unity_ProbesOcclusion);
	#endif

	half3 indirectDiffuse;
	half3 indirectSpecular;
	#if defined GET_GI
		GET_GI(indirectDiffuse,indirectSpecular,i,surface,mainLight)
	#else
		indirectDiffuse = IndirectDiffuse(mainLight,i,normalWS);
		indirectSpecular = IndirectSpecular(surface.reflectDir, surface.perceptualRoughness,0);
	#endif
	
	finalCol+=BRDFGlobalIllumination(surface,indirectDiffuse,indirectSpecular);

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
	// finalCol+=surface.emission;

	half alpha = 1.h;
	#if defined(GET_ALPHA)
		alpha = GET_ALPHA(i,surface);
	#endif

	o.result = float4(finalCol,1);
	// o.result = 1;
	#if defined(F2O_TRANSFER)
		F2O_TRANSFER(o)
	#endif
	return o;
}
