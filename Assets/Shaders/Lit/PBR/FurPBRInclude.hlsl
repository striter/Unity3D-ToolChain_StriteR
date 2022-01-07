			#define SHELLDELTA (SHELLINDEX+1)/SHELLCOUNT

			float GetGeometryShadow(BRDFSurface surface,BRDFLightInput lightSurface)
			{
				return saturate(invlerp(-SHELLDELTA*INSTANCE(_FurScattering),1,lightSurface.NDL));
			}

			float GetNormalDistribution(BRDFSurface surface,BRDFLightInput lightSurface)
			{
				half sqrRoughness=surface.roughness2;
				half NDH=max(0., lightSurface.NDH);
							
				half normalDistribution = NDF_CookTorrance(NDH,sqrRoughness);
				normalDistribution=clamp(normalDistribution,0,100.h);

				// normalDistribution+=pow5(1.0-surface.NDV)*SHELLDELTA*20;
				return normalDistribution;
			}
						
			float GetNormalizationTerm(BRDFSurface surface,BRDFLightInput lightSurface)
			{
				return InvVF_GGX(max(0., lightSurface.LDH),surface.roughness);
			}
						
			#include "Assets/Shaders/Library/BRDF/BRDFLighting.hlsl"

			struct a2f
			{
				float3 positionOS : POSITION;
				float3 normalOS:NORMAL;
				float4 tangentOS:TANGENT;
				float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float4 uv:TEXCOORD0;
				float3 positionWS:TEXCOORD1;
				float4 positionHCS:TEXCOORD2;
				float3 normalWS:TEXCOORD3;
				half3 tangentWS:TEXCOORD4;
				half3 biTangentWS:TEXCOORD5;
				FOG_COORD(6)
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f vert(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.normalWS=normalize(mul((float3x3)unity_ObjectToWorld,v.normalOS));
				o.tangentWS=normalize(mul((float3x3)unity_ObjectToWorld,v.tangentOS.xyz));
				o.biTangentWS=cross(o.normalWS,o.tangentWS)*v.tangentOS.w;
				o.positionWS=TransformObjectToWorld(v.positionOS);
				o.uv = float4( TRANSFORM_TEX_INSTANCE(v.uv,_MainTex),TRANSFORM_TEX_INSTANCE(v.uv,_FurTex));
				float delta=SHELLDELTA;
				float amount=delta+=(delta- delta*delta);
				
				o.positionWS+=o.normalWS*INSTANCE(_FurLength)*amount;
				// o.positionWS+=dot(o.normalWS,float3(0,1,0))*amount*float3(0,INSTANCE(_FurGravity),0);
				o.uv.yw+=amount*INSTANCE(_FURUVDelta);
				o.positionCS = TransformWorldToHClip(o.positionWS);
				o.positionHCS = o.positionCS;
				FOG_TRANSFER(o)
				return o;
			}
			

			float4 frag(v2f i,out float depth:SV_DEPTH) :SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				
				float3 positionWS=i.positionWS;
				float3 normalWS=normalize(i.normalWS);
				half3 biTangentWS=normalize(i.biTangentWS);
				half3 tangentWS=normalize(i.tangentWS);
				float3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
				float3 viewDirWS=-GetCameraRealDirectionWS(positionWS);
				half3 normalTS=half3(0,0,1);
				float2 baseUV=i.uv.xy;
				depth=i.positionCS.z;

				half4 color=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,baseUV);
				half3 albedo=color.rgb;
				
				half glossiness=INSTANCE(_Glossiness);
				half metallic=INSTANCE(_Metallic);
				half ao=1.h;
				#if _PBRMAP
					half3 mix=SAMPLE_TEXTURE2D(_PBRTex,sampler_PBRTex,baseUV).rgb;
					glossiness=1.h-mix.r;
					metallic=mix.g;
					ao=mix.b;
				#endif

				float delta= SHELLDELTA;
				albedo*=lerp(INSTANCE(_RootColor),INSTANCE(_EdgeColor),delta);
				ao=saturate(ao-(1-delta)*INSTANCE(_FurShadow));
				float furSample=SAMPLE_TEXTURE2D(_FurTex,sampler_FurTex,i.uv.zw).r;
				clip(furSample-delta*delta*INSTANCE(_FurAlphaClip));
				
				#if _NORMALMAP
					normalTS=DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,baseUV));
					normalWS=normalize(mul(transpose(TBNWS), normalTS));
				#endif
				
				BRDFSurface surface=BRDFSurface_Ctor(albedo,0,glossiness,metallic,ao,normalWS,tangentWS,viewDirWS,0);
				
				half3 finalCol=0;
				Light mainLight=GetMainLight(TransformWorldToShadowCoord(positionWS),positionWS,unity_ProbesOcclusion);
				half3 indirectDiffuse= IndirectBRDFDiffuse(mainLight,i.lightmapUV,normalWS);
				half3 indirectSpecular=IndirectBRDFSpecular(surface.reflectDir, surface.perceptualRoughness,i.positionHCS,normalTS);
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