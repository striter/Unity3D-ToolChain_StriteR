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
};

v2ff ForwardVertex(a2vf v)
{
	v2ff o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	o.uv = TRANSFORM_TEX_INSTANCE(v.uv,_MainTex);
	float3 positionWS=TRANSFER_POSITION_WS(v);
	o.positionWS = positionWS;
	o.positionCS = TransformWorldToHClip(positionWS);
	o.positionHCS = o.positionCS;
	o.normalWS = TransformObjectToWorldNormal(v.normalOS);
	o.tangentWS = normalize(mul((float3x3)unity_ObjectToWorld,v.tangentOS.xyz));
	o.biTangentWS = cross(o.normalWS,o.tangentWS)*v.tangentOS.w;
	o.viewDirWS = GetViewDirectionWS(o.positionWS);
	o.color=v.color;
	LIGHTMAP_TRANSFER(v,o)
	FOG_TRANSFER(o)
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

	#if _NORMALMAP
		float3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
		normalTS=DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,baseUV));
		normalWS=normalize(mul(transpose(TBNWS), normalTS));
	#endif

	half3 albedo = GET_ALBEDO(i);
	half3 emission= GET_EMISSION(i); 

	half glossiness=INSTANCE(_Glossiness);
	half metallic=INSTANCE(_Metallic);
	half ao=1.h;
	half anisotropic=1;
	#if _PBRMAP
		half3 mix=SAMPLE_TEXTURE2D(_PBRTex,sampler_PBRTex,baseUV).rgb;
		glossiness=1.h-mix.r;
		metallic=mix.g;
		ao=mix.b;
	#endif

	BRDFSurface surface=BRDFSurface_Ctor(albedo,emission,glossiness,metallic,ao,normalWS,tangentWS,biTangentWS,viewDirWS,anisotropic);

	half3 finalCol=0;
	Light mainLight=GetMainLight(TransformWorldToShadowCoord(positionWS),positionWS,unity_ProbesOcclusion);

	half3 indirectDiffuse= IndirectDiffuse(mainLight,i,normalWS);
	half3 indirectSpecular=IndirectSpecular(surface.reflectDir, surface.perceptualRoughness,i.positionHCS,normalTS);
	finalCol+=BRDFGlobalIllumination(surface,indirectDiffuse,indirectSpecular);

	finalCol+=BRDFLighting(surface,mainLight);

	#if _ADDITIONAL_LIGHTS
	uint pixelLightCount = GetAdditionalLightsCount();
	for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
		finalCol+=BRDFLighting(surface, GetAdditionalLight(lightIndex,i.positionWS));
	#endif
	FOG_MIX(i,finalCol);
	finalCol+=surface.emission;
	return half4(finalCol,1.h);
}
