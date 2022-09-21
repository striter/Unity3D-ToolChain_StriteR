struct a2vf
{
	UNITY_VERTEX_INPUT_INSTANCE_ID
	float3 positionOS : POSITION;
	float3 normalOS:NORMAL;
	float4 tangentOS:TANGENT;
	float4 color:COLOR;
	float2 uv:TEXCOORD0;
	A2V_LIGHTMAP
	#if defined(A2V_ADDITIONAL)
		A2V_ADDITIONAL
	#endif
};

struct v2ff
{
	UNITY_VERTEX_INPUT_INSTANCE_ID
	float4 positionCS : SV_POSITION;
	float4 color:COLOR;
	float3 normalWS:NORMAL;
	float2 uv:TEXCOORD0;
	float3 positionWS:TEXCOORD1;
	float4 positionHCS:TEXCOORD2;
	half3 tangentWS:TEXCOORD3;
	half3 biTangentWS:TEXCOORD4;
	half3 viewDirWS:TEXCOORD5;
	V2F_FOG(6)
	V2F_LIGHTMAP(7)
	#if defined(V2F_ADDITIONAL)
		V2F_ADDITIONAL
	#endif
};

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

float4 ForwardFragment(v2ff i):SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(i);

	float3 positionWS=i.positionWS;
	float3 normalWS=normalize(i.normalWS);
	half3 tangentWS=normalize(i.tangentWS);
	float3 viewDirWS=normalize(i.viewDirWS);
	half3 biTangentWS=normalize(i.biTangentWS);
	half3 normalTS=half3(0,0,1);
	float2 baseUV=i.uv.xy;

	#if !defined (_NORMALOFF)
		#if defined(GET_NORMAL)
			normalTS = GET_NORMAL(i);
		#else
			normalTS=DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,baseUV));
		#endif
		float3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
		normalWS=normalize(mul(transpose(TBNWS), normalTS));
	#endif
	
	#if defined(GET_ALBEDO)
		half3 albedo = GET_ALBEDO(i);
	#else
		half3 albedo = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).rgb*INSTANCE(_Color).rgb*i.color.rgb;
	#endif

	#if defined(GET_EMISSION)
		half3 emission = GET_EMISSION(i); 
	#else
		half3 emission = SAMPLE_TEXTURE2D(_EmissionTex,sampler_EmissionTex,i.uv).rgb*INSTANCE(_EmissionColor).rgb;
	#endif


	half3 mix=SAMPLE_TEXTURE2D(_PBRTex,sampler_PBRTex,baseUV).rgb;
	half glossiness=mix.r;
	half metallic=mix.g;
	half ao=mix.b;

	#if defined(GET_PBRPARAM)
		GET_PBRPARAM(glossiness,metallic,ao)
	#endif
	
	BRDFSurface surface=BRDFSurface_Ctor(albedo,emission,glossiness,metallic,ao,normalWS,tangentWS,biTangentWS,viewDirWS,1);

	half3 finalCol=0;

	Light mainLight =
	#if defined GET_MAINLIGHT
		GET_MAINLIGHT(surface,positionWS)
	#else
		GetMainLight(TransformWorldToShadowCoord(positionWS),positionWS,unity_ProbesOcclusion);
	#endif

	half3 indirectDiffuse= 
	#if defined GET_INDIRECTDIFFUSE
		GET_INDIRECTDIFFUSE(surface)
	#else
		IndirectDiffuse(mainLight,i,normalWS);
	#endif

	half3 indirectSpecular=
	#if defined GET_INDIRECTSPECULAR
		GET_INDIRECTSPECULAR(surface)
	#else
		IndirectSpecular(surface.reflectDir, surface.perceptualRoughness,0);
	#endif
	finalCol+=BRDFGlobalIllumination(surface,indirectDiffuse,indirectSpecular);

	#if defined BRDF_MAINLIGHTING
		BRDF_MAINLIGHTING(mainLight,surface)
	#endif
	finalCol+=BRDFLighting(surface,mainLight);

	#if _ADDITIONAL_LIGHTS
		uint pixelLightCount = GetAdditionalLightsCount();
		for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
		finalCol+=BRDFLighting(surface, GetAdditionalLight(lightIndex,i.positionWS));
	#endif
	FOG_MIX(i,finalCol);
	finalCol+=surface.emission;

	#if defined(GET_FINALCOL)
		finalCol = GET_FINALCOL(finalCol,i,surface);
	#endif
	half alpha = 1.h;
	#if defined(GET_ALPHA)
		alpha = GET_ALPHA(i,surface);
	#endif
	return half4(finalCol,alpha);
}
