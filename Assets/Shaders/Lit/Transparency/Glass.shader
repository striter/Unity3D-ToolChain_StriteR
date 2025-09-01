Shader "Runtime/Lit/Transparecy/Glass"
{
    Properties
    {
        _MainTex("Texture",2D)="white"{}
    	_ColorTint("Color Tint",Color)=(1,1,1,0.5)
        [ToggleTex(_NORMALTEX)][NoScaleOffset]_NormalTex("Normal Tex",2D)="white"{}
    	
    	[Toggle(_REFRACTION)]_Refraction("Refraction",int)=0
    	[Foldout(_REFRACTION)]_DistortStrength("Distort Strength",Range(0,5))=0.05
    	
    	[Header(Fresnel)]
    	[MinMaxRange]_Fresnel("Fresnel",Range(0,1))=0
    	[HideInInspector]_FresnelEnd("",float)=0
    	
    	[Header(Rim)]
    	[HDR]_RimColor("Rim Color",Color)=(1,1,1,1)
    	[MinMaxRange]_Rim("Rim",Range(0,1))=0.1
    	[HideInInspector]_RimEnd("",float)=0.15
    	
    	[Header(Reflection)]
    	_ReflectionOffset("_Visibility",Range(-7,7)) = 8
    	
    	[Header(Specular)]
    	_SpecularStrength("Specular Strength",Range(0,5))=1
    	_SpecularGlossiness("Specular Glossiness",Range(0,1))=0.75
    	
    	[Header(Render Options)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",int)=1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend",int)=0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull",int)=2
    }
    SubShader
    {
		Blend [_SrcBlend] [_DstBlend]
		Cull [_Cull]
		ZWrite [_ZWrite]
		ZTest [_ZTest]
    	
		Tags {"Queue" =  "Transparent"}
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local_fragment _NORMALTEX
            #pragma shader_feature_local_fragment _REFRACTION

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
				float4 positionHCS:TEXCOORD2;
				half3 normalWS:TEXCOORD3;
				half3 tangentWS:TEXCOORD4;
				half3 biTangentWS:TEXCOORD5;
            	float depthDistance:TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

			TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalTex);SAMPLER(sampler_NormalTex);
            TEXTURE2D(_CameraOpaqueTexture);SAMPLER(sampler_CameraOpaqueTexture);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_ColorTint)
				INSTANCING_PROP(float,_Fresnel)
				INSTANCING_PROP(float,_FresnelEnd)

				INSTANCING_PROP(float4,_RimColor)
				INSTANCING_PROP(float,_Rim)
				INSTANCING_PROP(float,_RimEnd)
            
                INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float,_DistortStrength)
				INSTANCING_PROP(float,_SpecularStrength)
				INSTANCING_PROP(float,_SpecularGlossiness)
				INSTANCING_PROP(int,_ReflectionOffset)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionHCS=o.positionCS;
                o.uv = TRANSFORM_TEX_INSTANCE(v.uv, _MainTex);
                o.positionWS=TransformObjectToWorld(v.positionOS);
				o.normalWS=normalize(mul((float3x3)unity_ObjectToWorld,v.normalOS));
				o.tangentWS=normalize(mul((float3x3)unity_ObjectToWorld,v.tangentOS.xyz));
				o.biTangentWS=cross(o.normalWS,o.tangentWS)*v.tangentOS.w;
				o.depthDistance=-TransformWorldToView(o.positionWS).z;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
				float3 normalWS=normalize(i.normalWS);
				half3 biTangentWS=normalize(i.biTangentWS);
				half3 tangentWS=normalize(i.tangentWS);
				half3 normalTS=half3(0,0,1);
				float4 colorSample = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
            	float3 albedo = colorSample.rgb * INSTANCE(_ColorTint.rgb);
            	
            	float3 cameraDirWS=GetCameraRealDirectionWS(i.positionWS);
            	float3 viewDirWS=-cameraDirWS;
            	float3 reflectDirWS=normalize(reflect(cameraDirWS, normalWS));
            	float3 lightDirWS=normalize(_MainLightPosition.xyz);
            	float3 halfDirWS=normalize(lightDirWS+viewDirWS);
                float ndv=dot(viewDirWS,normalWS);

				#if _NORMALTEX
					float3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
					normalTS=DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,i.uv));
					normalWS=normalize(mul(transpose(TBNWS), normalTS));
				#endif
            	
            	float opacityFresnel=saturate(max(invlerp(_FresnelEnd,_Fresnel,ndv),_ColorTint.a*colorSample.a));
            	
            	float3 specularColor=0;
            	float3 indirectDiffuse = IndirectDiffuse_SH(normalWS);
                float3 indirectSpecular = IndirectCubeSpecular(reflectDirWS,1,_ReflectionOffset);
            	specularColor +=  indirectSpecular*albedo;

				float rim = saturate(invlerp(_RimEnd,_Rim,ndv));
            	specularColor += rim * _RimColor.rgb * _RimColor.a * _MainLightColor.rgb;
            	
				float specular= GetSpecular(normalWS,halfDirWS,INSTANCE(_SpecularGlossiness))*INSTANCE(_SpecularStrength);
				specularColor += specular * _MainLightColor.rgb;

            	#if _REFRACTION
            		float2 screenUV=TransformHClipToNDC(i.positionHCS)+normalTS.xy*INSTANCE(_DistortStrength)/i.depthDistance;
            		float3 baseCol = lerp(SAMPLE_TEXTURE2D(_CameraOpaqueTexture,sampler_CameraOpaqueTexture,screenUV).rgb,albedo*indirectDiffuse,opacityFresnel);
	                return float4(baseCol+specularColor,1);
				#else
					return float4(albedo*indirectDiffuse+specularColor,saturate(max(opacityFresnel,specular,rim)));
				#endif
            }
            ENDHLSL
        }
    }
}
