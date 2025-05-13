Shader "Game/Lit/UberPhong"
{
    Properties
    {
        _MainTex("Main Tex",2D)="white"{}
        _Color("Color Tint",Color)=(1,1,1,1)
        _NormalTex("Normal",2D)= "white"{}
        _SpecularTex("Specular",2D)="white"{}
        _EmissionTex("Emission",2D)="black"{}
    	
        [HideInInspector]_ZWrite("Z Write",int)=1
        [HideInInspector]_ZTest("Z Test",int)=2
        [HideInInspector]_Cull("Cull",int)=2
    }
	
	
	//May not be a proper phong lighting?
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
				float3 normalOS:NORMAL;
				float4 tangentOS:TANGENT;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
				float3 positionWS:TEXCOORD1;
				half3 normalWS:TEXCOORD3;
				half3 tangentWS:TEXCOORD4;
				half3 biTangentWS:TEXCOORD5;
				half3 viewDirWS:TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };
			#pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
			TEXTURE2D(_EmissionTex);
			TEXTURE2D(_SpecularTex);
			TEXTURE2D(_NormalTex);
            
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_Color)
                INSTANCING_PROP(float4,_MainTex_ST)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.uv = TRANSFORM_TEX_INSTANCE(v.uv,_MainTex);
				o.positionWS= TransformObjectToWorld(v.positionOS);
				o.positionCS = TransformObjectToHClip(v.positionOS);
				o.normalWS = TransformObjectToWorldNormal(v.normalOS);
				o.tangentWS = normalize(mul((float3x3)unity_ObjectToWorld,v.tangentOS.xyz));
				o.biTangentWS = cross(o.normalWS,o.tangentWS)*v.tangentOS.w;
				o.viewDirWS = GetViewDirectionWS(o.positionWS);
                return o;
            }

			float3 PhongLighting(float3 albedo,float3 specular,float3 normal,float3 viewDir,Light light)
            {
				float ndl=saturate(dot(normal,light.direction));
            	float3 halfDir=normalize(viewDir+light.direction);
            	float ndh = saturate(dot(normal,halfDir));

            	float3 radiance = light.color * light.shadowAttenuation*light.distanceAttenuation; 
            	return radiance*albedo*ndl +  radiance* specular*pow(ndh,150)*5;
            }
            
            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
				float3 positionWS=i.positionWS;
				float3 normalWS=normalize(i.normalWS);
				half3 biTangentWS=normalize(i.biTangentWS);
				half3 tangentWS=normalize(i.tangentWS);
				float3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
				float3 viewDirWS=normalize(i.viewDirWS);
				float2 baseUV=i.uv.xy;
            	float3 albedo = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).rgb * INSTANCE(_Color).rgb;
            	float3 specular = SAMPLE_TEXTURE2D(_SpecularTex,sampler_MainTex,i.uv).rgb;
            	float3 emission = SAMPLE_TEXTURE2D(_EmissionTex,sampler_MainTex,i.uv).rgb;
            	
				normalWS=normalize(mul(transpose(TBNWS), SAMPLE_TEXTURE2D(_NormalTex,sampler_MainTex,baseUV)));
				float3 ambient = IndirectDiffuse(mainLight,i,normalWS);
            	float3 finalCol=0;
				
            	finalCol+=ambient*albedo;
            	
            	Light mainLight = GetMainLight(TransformWorldToShadowCoord(positionWS),positionWS,unity_ProbesOcclusion);
				finalCol+=PhongLighting(albedo,specular,normalWS,viewDirWS,mainLight);
            	#if _ADDITIONAL_LIGHTS
            		uint pixelLightCount = GetAdditionalLightsCount();
				    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
				    {
			    		Light additionalLight = GetAdditionalLight(lightIndex,i.positionWS);
						finalCol+=PhongLighting(albedo,specular,normalWS,viewDirWS,additionalLight);
				    }
            	#endif
            	finalCol += emission;
                return float4(finalCol,1);
            }
            ENDHLSL
        }
        
        USEPASS "Game/Additive/DepthOnly/MAIN"
        USEPASS "Game/Additive/ShadowCaster/MAIN"
    }
}
