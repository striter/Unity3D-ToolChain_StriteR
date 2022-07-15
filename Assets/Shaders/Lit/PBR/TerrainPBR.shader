Shader "Game/Lit/TerrainPBR"
{
	Properties
	{
		[Header(Source Glossiness is Albedos alpha)]
		_Splat0("Albedo 0",2D) = "white"{}
		_Color0("Color 0",Color) = (1,1,1,1)
		[NoScaleOffset]_Normal0("Nomral 0",2D)="white"{}
		_Glossiness0("Glossiness 0",Range(0,1))=1
        _Metallic0("Metalness 0",Range(0,1))=0
		_NormalIntensity0("Normal Strength 0",Range(0,10))=1
		
		[ToggleTex(_TEX1)]_Splat1("Albedo 1",2D) = "white"{}
		[Foldout(_TEX1)]_Color1("Color 1",Color) = (1,1,1,1)
		[Foldout(_TEX1)][NoScaleOffset]_Normal1("Nomral 1",2D)="white"{}
		[Foldout(_TEX1)]_Glossiness1("Glossiness 1",Range(0,1))=1
        [Foldout(_TEX1)]_Metallic1("Metalness 1",Range(0,1))=0
		[Foldout(_TEX1)]_NormalIntensity1("Normal Strength 1",Range(0,10))=1
		
		[ToggleTex(_TEX2)]_Splat2("Albedo 2",2D) = "white"{}
		[Foldout(_TEX2)]_Color2("Color 2",Color) = (1,1,1,1)
		[Foldout(_TEX2)][NoScaleOffset]_Normal2("Nomral 2",2D)="white"{}
		[Foldout(_TEX2)]_Glossiness2("Glossiness 2",Range(0,1))=1
        [Foldout(_TEX2)]_Metallic2("Metalness 2",Range(0,1))=0
		[Foldout(_TEX2)]_NormalIntensity2("Normal Strength 2",Range(0,10))=1
		
		[ToggleTex(_TEX3)]_Splat3("Albedo 3",2D) = "white"{}
		[Foldout(_TEX3)]_Color3("Color 3",Color) = (1,1,1,1)
		[Foldout(_TEX3)][NoScaleOffset]_Normal3("Nomral 3",2D)="white"{}
		[Foldout(_TEX3)]_Glossiness3("Glossiness 3",Range(0,1))=1
        [Foldout(_TEX3)]_Metallic3("Metalness 3",Range(0,1))=0
		[Foldout(_TEX3)]_NormalIntensity3("Normal Strength 3",Range(0,10))=1
		
		[Header(Detail Tex)]
		_Control("Control",2D)="white"{}
		[ToggleTex(_MATCAP)] [NoScaleOffset]_Matcap("Mat Cap",2D)="white"{}		
		[Foldout(_MATCAP)][HDR]_MatCapColor("MatCap Color",Color)=(1,1,1,1)
		
		[Header(Render Options)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",int)=1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend",int)=0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull",int)=2
		[Foldout(LIGHTMAP_CUSTOM,LIGHTMAP_INTERPOLATE)]_LightmapST("CLightmap UV",Vector)=(1,1,1,1)
		[Foldout(LIGHTMAP_CUSTOM,LIGHTMAP_INTERPOLATE)]_LightmapIndex("CLightmap Index",int)=0
	}
	SubShader
	{
		Tags { "Queue" = "Geometry" }
		Blend [_SrcBlend] [_DstBlend]
		Cull [_Cull]
		ZWrite [_ZWrite]
		ZTest [_ZTest]
		
		HLSLINCLUDE
			#include "Assets/Shaders/Library/Common.hlsl"
			#pragma multi_compile_instancing
			#pragma shader_feature_local _MATCAP
			#pragma shader_feature_local _TEX1
			#pragma shader_feature_local _TEX2
			#pragma shader_feature_local _TEX3

			TEXTURE2D(_Splat0); SAMPLER(sampler_Splat0);
			TEXTURE2D(_Normal0); SAMPLER(sampler_Normal0);
			TEXTURE2D(_Splat1); SAMPLER(sampler_Splat1);
			TEXTURE2D(_Normal1); SAMPLER(sampler_Normal1);
			TEXTURE2D(_Splat2); SAMPLER(sampler_Splat2);
			TEXTURE2D(_Normal2); SAMPLER(sampler_Normal2);
			TEXTURE2D(_Splat3); SAMPLER(sampler_Splat3);
			TEXTURE2D(_Normal3); SAMPLER(sampler_Normal3);
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_Splat0_ST)
				INSTANCING_PROP(float3,_Color0)
				INSTANCING_PROP(float,_Glossiness0)
				INSTANCING_PROP(float,_Metallic0)
				INSTANCING_PROP(float,_NormalIntensity0)

				INSTANCING_PROP(float4,_Splat1_ST)
				INSTANCING_PROP(float3,_Color1)
				INSTANCING_PROP(float,_Glossiness1)
				INSTANCING_PROP(float,_Metallic1)
				INSTANCING_PROP(float,_NormalIntensity1)
		
				INSTANCING_PROP(float4,_Splat2_ST)
				INSTANCING_PROP(float3,_Color2)
				INSTANCING_PROP(float,_Glossiness2)
				INSTANCING_PROP(float,_Metallic2)
				INSTANCING_PROP(float,_NormalIntensity2)
		
				INSTANCING_PROP(float4,_Splat3_ST)
				INSTANCING_PROP(float3,_Color3)
				INSTANCING_PROP(float,_Glossiness3)
				INSTANCING_PROP(float,_Metallic3)
				INSTANCING_PROP(float,_NormalIntensity3)
		
				INSTANCING_PROP(float3,_MatCapColor)
				
			    INSTANCING_PROP(float4,_LightmapST)
			    INSTANCING_PROP(float,_LightmapIndex)
			INSTANCING_BUFFER_END

			TEXTURE2D(_Matcap);SAMPLER(sampler_Matcap);
			TEXTURE2D(_Control);SAMPLER(sampler_Control);
			#include "Assets/Shaders/Library/Lighting.hlsl"

			struct Output
			{
				float3 albedo;
				float3 normalTS;
				float glossiness;
				float metallic;
			};
		
			Output FragmentOutput(float2 controlUV,float4 uv0,float4 uv1)
			{
				half4 mask=SAMPLE_TEXTURE2D(_Control,sampler_Control,controlUV);
				
				float4 sample0=SAMPLE_TEXTURE2D(_Splat0,sampler_Splat0,uv0.xy);
				float3 albedo=sample0.rgb;
				float4 nonDecodedNormalTS=SAMPLE_TEXTURE2D(_Normal0,sampler_Normal0,uv0.xy);
				float3 color=INSTANCE(_Color0);
				float glossiness=sample0.a*INSTANCE(_Glossiness0)*mask.r;
				float metallic=INSTANCE(_Metallic0)*mask.r;
				float normalStrength = INSTANCE(_NormalIntensity0);

				#if _TEX1
					float4 sample1=SAMPLE_TEXTURE2D(_Splat1,sampler_Splat0,uv0.xy);
					albedo=lerp(albedo,sample1.rgb,mask.g);
					nonDecodedNormalTS=lerp(nonDecodedNormalTS,SAMPLE_TEXTURE2D(_Normal1,sampler_Normal1,uv0.zw),mask.g);
					glossiness=lerp(glossiness,sample1.a*INSTANCE(_Glossiness1),mask.g);
					metallic=lerp(metallic,INSTANCE(_Metallic1),mask.g);
					normalStrength = lerp(normalStrength,INSTANCE(_NormalIntensity1),mask.g);
					color=lerp(color,INSTANCE(_Color1),mask.g);
				#endif
				
				#if _TEX2
					float4 sample2=SAMPLE_TEXTURE2D(_Splat2,sampler_Splat0,uv0.xy);
					albedo=lerp(albedo,sample2.rgb,mask.b);
					nonDecodedNormalTS=lerp(nonDecodedNormalTS,SAMPLE_TEXTURE2D(_Normal2,sampler_Normal2,uv1.xy),mask.b);
					glossiness=lerp(glossiness,sample2.a*INSTANCE(_Glossiness2),mask.b);
					metallic=lerp(metallic,INSTANCE(_Metallic2),mask.b);
					normalStrength = lerp(normalStrength,INSTANCE(_NormalIntensity2),mask.g);
					color=lerp(color,INSTANCE(_Color2),mask.g);
				#endif
				
				#if _TEX3
					float4 sample3=SAMPLE_TEXTURE2D(_Splat3,sampler_Splat0,uv0.xy);
					albedo=lerp(albedo,sample3.rgb,mask.a);
					nonDecodedNormalTS=lerp(nonDecodedNormalTS,SAMPLE_TEXTURE2D(_Normal3,sampler_Normal0,uv1.zw),mask.a);
					glossiness=lerp(glossiness,sample3.a*INSTANCE(_Glossiness3),mask.a);
					metallic=lerp(metallic,INSTANCE(_Metallic3),mask.a);
					normalStrength = lerp(normalStrength,INSTANCE(_NormalIntensity3),mask.b);
					color=lerp(color,INSTANCE(_Color3),mask.b);
				#endif

				Output o;
				o.albedo=albedo*color;
				float3 normalTS=DecodeNormalMap(nonDecodedNormalTS);
				normalTS.xy*=normalStrength;
				o.normalTS=normalTS;
				o.glossiness=glossiness;
				o.metallic=metallic;
				return o;
			}
			#define V2F_UV  float4 uv0:TEXCOORD0; float4 uv1:TEXCOORD1; float2 controlUV:TEXCOORD2;
			#define UV_TRANSFER(v,o)  o.controlUV=v.uv; o.uv0 = float4(TRANSFORM_TEX_INSTANCE(v.uv,_Splat0),TRANSFORM_TEX_INSTANCE(v.uv,_Splat1)); o.uv1 = float4(TRANSFORM_TEX_INSTANCE(v.uv,_Splat2),TRANSFORM_TEX_INSTANCE(v.uv,_Splat3)); 
			struct a2f
			{
				float3 positionOS : POSITION;
				float3 normalOS:NORMAL;
				float4 tangentOS:TANGENT;
				float2 uv:TEXCOORD0;
				A2V_LIGHTMAP
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				V2F_UV
				float3 positionWS:TEXCOORD3;
				float4 positionHCS:TEXCOORD4;
				half3 normalWS:TEXCOORD5;
				half3 tangentWS:TEXCOORD6;
				half3 biTangentWS:TEXCOORD7;
				V2F_FOG(8)
				V2F_LIGHTMAP(9)
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f vert(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UV_TRANSFER(v,o)
				o.positionWS=TransformObjectToWorld(v.positionOS);
				o.positionCS = TransformObjectToHClip(v.positionOS);
				o.positionHCS = o.positionCS;
				o.normalWS=normalize(mul((float3x3)unity_ObjectToWorld,v.normalOS));
				o.tangentWS=normalize(mul((float3x3)unity_ObjectToWorld,v.tangentOS.xyz));
				o.biTangentWS=cross(o.normalWS,o.tangentWS)*v.tangentOS.w;
				LIGHTMAP_TRANSFER(v,o)
				FOG_TRANSFER(o)
				return o;
			}
		ENDHLSL
		Pass
		{
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Assets/Shaders/Library/BRDF/BRDFMethods.hlsl"
			#include "Assets/Shaders/Library/BRDF/BRDFInput.hlsl"
			
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON LIGHTMAP_CUSTOM LIGHTMAP_INTERPOLATE
			
            #pragma multi_compile_fog
            #pragma target 3.5

			float GetGeometryShadow(BRDFSurface surface,BRDFLightInput lightSurface)
			{
				return max(0., lightSurface.NDL);
			}

			float GetNormalDistribution(BRDFSurface surface,BRDFLightInput lightSurface)
			{
				half sqrRoughness=surface.roughness2;
				half NDH=max(0., lightSurface.NDH);
				
				half normalDistribution=NDF_CookTorrance(NDH,sqrRoughness);
				normalDistribution=clamp(normalDistribution,0,100.h);
				return normalDistribution;
			}
			
			float GetNormalizationTerm(BRDFSurface surface,BRDFLightInput lightSurface)
			{
				float LDH=max(0., lightSurface.LDH);
				return InvVF_GGX(LDH,surface.roughness);
			}
			
			#include "Assets/Shaders/Library/BRDF/BRDFLighting.hlsl"

			float4 frag(v2f i) :SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				
				float3 positionWS=i.positionWS;
				float3 normalWS=normalize(i.normalWS);
				half3 biTangentWS=normalize(i.biTangentWS);
				half3 tangentWS=normalize(i.tangentWS);
				float3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
				float3 viewDirWS=-GetCameraRealDirectionWS(positionWS);

				Output o = FragmentOutput(i.controlUV,i.uv0,i.uv1);
				normalWS=mul(transpose(TBNWS), o.normalTS);
				BRDFSurface surface=BRDFSurface_Ctor( o.albedo,0,o.glossiness,o.metallic,1,normalWS,tangentWS,biTangentWS,viewDirWS,0);
				half3 finalCol=0;
				Light mainLight=GetMainLight(TransformWorldToShadowCoord(positionWS),positionWS,unity_ProbesOcclusion);
				half3 indirectDiffuse = IndirectDiffuse(mainLight,i,normalWS);
				half3 indirectSpecular=IndirectSpecular(surface.reflectDir, surface.perceptualRoughness,i.positionHCS,o.normalTS);
				finalCol+=BRDFGlobalIllumination(surface,indirectDiffuse,indirectSpecular);
				
				#if _MATCAP
					float2 matcapUV=float2(dot(UNITY_MATRIX_V[0].xyz,normalWS),dot(UNITY_MATRIX_V[1].xyz,normalWS));
					matcapUV=matcapUV*.5h+.5h;
					mainLight.color=SAMPLE_TEXTURE2D(_Matcap,sampler_Matcap,matcapUV).rgb*INSTANCE(_MatCapColor).rgb;
					mainLight.distanceAttenuation = 1;
				#endif
				
				finalCol+=BRDFLighting(surface,mainLight);
		
				#if _ADDITIONAL_LIGHTS
            	uint pixelLightCount = GetAdditionalLightsCount();
			    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
					finalCol+=BRDFLighting(surface, GetAdditionalLight(lightIndex,i.positionWS));
            	#endif
				FOG_MIX(i,finalCol); 
				return half4(finalCol,1);
			}
			ENDHLSL
		}

		Pass
		{
            Name "Meta"
            Tags{"LightMode" = "Meta"}

            Cull Back

            HLSLPROGRAM
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
            #pragma vertex Vertex
            #pragma fragment Fragment
			struct a2vm
			{
			    float4 positionOS:POSITION;
			    // float3 normalOS:NORMAL;
			    float2 uv:TEXCOORD0;
			    float2 uv1:TEXCOORD1;
			    float2 uv2:TEXCOORD2;
			};

			struct v2fm
			{
			    float4 positionCS:SV_POSITION;
				V2F_UV
			};

			v2fm Vertex(a2vm v)
			{
			    v2fm o;
			    o.positionCS=MetaVertexPosition(v.positionOS,v.uv1,v.uv2,unity_LightmapST,unity_DynamicLightmapST);
				UV_TRANSFER(v,o);
			    return o;
			}

			float4 Fragment(v2fm i):SV_TARGET
			{
				Output o = FragmentOutput(i.controlUV,i.uv0,i.uv1);
			    half3 albedo = o.albedo;
			    half3 emission = 0;
			    half4 res = 0;
			    if (unity_MetaFragmentControl.x)
			    {
			        res = half4(albedo, 1.0);
			        res.rgb = clamp(PositivePow(res.rgb, saturate(unity_OneOverOutputBoost)), 0, unity_MaxOutputValue);
			    }
			    if (unity_MetaFragmentControl.y)
			    {
			        if (!unity_UseLinearSpace)
			            emission = LinearToSRGB(emission);

			        res = half4(emission, 1.0);
			    }
			    return res;
			}
            ENDHLSL
		}
		
		USEPASS "Game/Additive/DepthOnly/MAIN"
		USEPASS "Game/Additive/ShadowCaster/MAIN"
	}
}