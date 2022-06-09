Shader "Game/Lit/PBR/GrassSurface"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Tint",Color)=(1,1,1,1)
		_Glossiness("Glossiness",Range(0,1))=1
        _Metallic("Metalness",Range(0,1))=0
        [NoScaleOffset]_FlowTex ("Flow",2D)="white"{}
        _Scale0("Scale0",float)=10
        [Vector2]_Flow0("Flow0",Vector)=(0,1,0,0)
        _Scale1("Scale1",float)=10
        [Vector2]_Flow1("Flow1",Vector)=(1,0,0,0)
        
        _NoiseTex ("Noise",2D)="white"{}
    	_FlowDistort("Flow Distort",Range(0,1))=0.1
        _MaskTex("Mask",2D)="black"{}
    	_MaskBegin("Mask Begin",Range(0,1))=0.1
    	_MaskWidth("Mask Width",Range(0,1))=0.1
        _MaskStrength("Strength",Range(0,2))=1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"
            #include "Assets/Shaders/Library/BRDF/BRDFMethods.hlsl"
			#include "Assets/Shaders/Library/BRDF/BRDFInput.hlsl"
			
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON

            float GetGeometryShadow(BRDFSurface surface,BRDFLightInput lightSurface)
			{
				return lightSurface.NDL;
			}

			float GetNormalDistribution(BRDFSurface surface,BRDFLightInput lightSurface)
			{
				half sqrRoughness=surface.roughness2;
				half NDH=lightSurface.NDH;
				half normalDistribution= NDF_CookTorrance(NDH,sqrRoughness);
				normalDistribution=clamp(normalDistribution,0,100.h);
				return normalDistribution;
			}
			
			float GetNormalizationTerm(BRDFSurface surface,BRDFLightInput lightSurface)
			{
				return InvVF_GGX(lightSurface.LDH,surface.roughness);
			}
			
			#include "Assets/Shaders/Library/BRDF/BRDFLighting.hlsl"
            
            struct a2v
            {
                float3 positionOS : POSITION;
                float3 normalOS:NORMAL;
            	A2V_LIGHTMAP
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS:TEXCOORD1;
                float3 positionWS:TEXCOORD2;
                float4 uv:TEXCOORD3;
                float4 flowUV : TEXCOORD4;
                V2F_FOG(5)
            	V2F_LIGHTMAP(6)
            };

            TEXTURE2D( _MainTex);SAMPLER(sampler_MainTex);
            TEXTURE2D(_FlowTex);SAMPLER(sampler_FlowTex);
            TEXTURE2D(_NoiseTex);SAMPLER(sampler_NoiseTex);
            TEXTURE2D(_MaskTex);SAMPLER(sampler_MaskTex);
            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float _Glossiness;
			float _Metallic;
            
            float4 _MainTex_ST;
            float4 _FlowTex_ST;
            float4 _NoiseTex_ST;
            float4 _MaskTex_ST;
            
            float _Scale0;
            float2 _Flow0;
            float _Scale1;
            float2 _Flow1;

			float _FlowDistort;
            
            float _MaskBegin;
            float _MaskWidth;
            float _MaskStrength;

            CBUFFER_END

            v2f vert (a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionWS=TransformObjectToWorld(v.positionOS);
                o.normalWS=TransformObjectToWorldNormal(v.normalOS);
                float2 baseUV=o.positionWS.xz;
                o.uv= float4(TRANSFORM_TEX(baseUV,_MainTex),baseUV);
                o.flowUV = float4((baseUV+_Time.y*_Flow0)*rcp(_Scale0),(baseUV+_Time.y*_Flow1)*rcp(_Scale1));
            	FOG_TRANSFER(o)
            	LIGHTMAP_TRANSFER(v,o)
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float3 normalWS=normalize(i.normalWS);
                float3 viewDirWS = -GetCameraRealDirectionWS(i.positionWS);
            	float3 positionWS=i.positionWS;
                
                float3 albedo=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv.xy).rgb*_Color.rgb;
				float glossiness=_Glossiness;
				float metallic=_Metallic;
				float ao=1;
            	
                float3 flow1Sample = SAMPLE_TEXTURE2D(_FlowTex,sampler_FlowTex, i.flowUV.xy).xyz;
                float3 flow2Sample = SAMPLE_TEXTURE2D(_FlowTex, sampler_FlowTex, i.flowUV.zw).xyz;
                float3 flowSample=flow1Sample*flow2Sample;
                float2 flowUV=flowSample.xy*2-1;

                float2 texUV=i.uv.zw;
                float2 noiseUV=texUV+flowUV*_FlowDistort;
                float3 noise=SAMPLE_TEXTURE2D(_NoiseTex,sampler_NoiseTex,TRANSFORM_TEX(noiseUV,_NoiseTex)).rgb;
            	float2 maskUV1=texUV+_Time.y*_Flow0;
            	float2 maskUV2=texUV+_Time.y*_Flow1;
            	
                float3 mask=SAMPLE_TEXTURE2D(_MaskTex,sampler_MaskTex,TRANSFORM_TEX(maskUV1,_MaskTex)).rgb;

            	normalWS=normalize(normalWS+(noise*2-1)*mask.r*flowSample.b);
				
				BRDFSurface surface=BRDFSurface_Ctor(albedo,0,glossiness,metallic,ao,normalWS,0,0,viewDirWS,0);
		
				Light mainLight=GetMainLight(TransformWorldToShadowCoord(positionWS),positionWS,unity_ProbesOcclusion);

				half3 finalCol=0;
				half3 indirectDiffuse= IndirectDiffuse(mainLight,i,normalWS);
				half3 indirectSpecular=IndirectSpecular(surface.reflectDir, surface.perceptualRoughness,0,0);
				finalCol+=BRDFGlobalIllumination(surface,indirectDiffuse,indirectSpecular);

            	BRDFLightInput input=BRDFLightInput_Ctor(surface,mainLight);
            	BRDFLight light=BRDFLight_Ctor(surface,input);
            	light.normalDistribution=max(light.normalDistribution*.1,saturate(invlerp(_MaskBegin,_MaskBegin+_MaskWidth,mask.b))*_MaskStrength*20);
				finalCol+=BRDFLighting(surface,light);

            	#if _ADDITIONAL_LIGHTS
            	uint pixelLightCount = GetAdditionalLightsCount();
			    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
					finalCol+=BRDFLighting(surface, GetAdditionalLight(lightIndex,i.positionWS));
            	#endif
				FOG_MIX(i,finalCol);
                return float4(finalCol,1);
            }
            ENDHLSL
        }
        USEPASS "Hidden/DepthOnly/MAIN"
    }
}
