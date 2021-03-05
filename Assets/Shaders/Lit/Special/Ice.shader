Shader "Game/Lit/Special/Ice"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		[Header(Normal Mappping)]
		[Toggle(_NORMALMAP)]_EnableNormalMap("Enable Normal Mapping",float)=1
		[NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		[Toggle(_THICK)]_EnableThick("Enable Ice Thick",float)=1
		_Thickness("Ice Thicknesss",Range(0.01,1))=.1

		[Header(Lighting)]
		[Header(_ Diffuse)]
		_Lambert("Lambert",Range(0,1))=.5
		[Header(_ Specular)]
		[Toggle(_SPECULAR)]_EnableSpecular("Enable Specular",float)=1
		_SpecularRange("Specular Range",Range(.9,1))=.98
		
		[Header(Crack)]
		[Toggle(_CRACK)]_EnableCrack("Enable Crack",Range(0,1))=.5
        [NoScaleOffset]_CrackTex("Crack Tex",2D)="white" {}
		[HDR]_CrackColor("Crack Color",Color)=(1,1,1,1)
		[Header(_ CrackTop)]
		[Toggle(_CRACKTOP)]_EnableCrackTop("Enable Crack Top",float)=1
		_CrackTopStrength("Crack Top Strength",Range(0.1,1))=.2
		[Header(_ Parallex)]
		[Toggle(_CRACKPARALLEX)]_CrackParallex("Enable Crack Parallex",float)=1
        [Enum(_4,4,_8,8,_16,16,_32,32,_64,64)]_CrackParallexTimes("Crack Parallex Times",int)=8
		_CrackDistance("Crack Distance",Range(0,1))=.5
		_CrackPow("Crack Pow",Range(0.1,5))=2

		[Header(Opacity)]
		[Toggle(_OPACITY)]_EnableOpacity("Enable Opacity",float)=1
		_BeginOpacity("Begin Opacity",Range(0,1))=.5
		[Header(_ Distort)]
		[Toggle(_DISTORT)]_EnableDistort("Enable Normal Distort",float)=1
		_DistortStrength("Distort Strength",Range(.1,2))=1
		[Header(_ Depth)]
		[Toggle(_DEPTH)]_EnableDepth("Enable Depth",float)=1
		_DepthDistance("Depth Distance",Range(0,3))=1
		_DepthPow("Depth Pow",Range(0.1,5))=2
		[Header(_ Fresnel)]
		[Toggle(_FRESNEL)]_EnableFresnel("Enable Fresnel",float)=1
		_FresnelPow("Fresnel Pow",Range(0.1,10))=1
    }
    SubShader
    {
        Tags { "Queue"="Transparent-1"}
		Blend Off
        Pass
        {
			Tags {"LightMode"="ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile_fwdbase
			#pragma shader_feature _NORMALMAP
			#pragma shader_feature _THICK
			#pragma shader_feature _CRACK
			#pragma shader_feature _CRACKTOP
			#pragma shader_feature _CRACKPARALLEX
			#pragma shader_feature _OPACITY
			#pragma shader_feature _DISTORT
			#pragma shader_feature _DEPTH
			#pragma shader_feature _FRESNEL
			#pragma shader_feature _SPECULAR

            #include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"
			#include "../../CommonInclude.cginc"
			#include "../../CommonLightingInclude.cginc"

            struct appdata
            {
			    float4 vertex : POSITION;
			    float2 uv:TEXCOORD0;
			    float3 normal:NORMAL;
			    float4 tangent:TANGENT;
            };

            struct v2f
            {
			    float4 pos : SV_POSITION;
			    float2 uv:TEXCOORD0;
			    float3 worldPos:TEXCOORD1;
			    float3 lightDir:TEXCOORD2;
			    float3 normal:TEXCOORD3;
			    float3 viewDir:TEXCOORD4;
			    SHADOW_COORDS(5)
				#if _OPACITY
				float4 screenPos:TEXCOORD6;
				#endif
            };
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _NormalTex;
			float _Thickness;
			float _Lambert;
			float _SpecularRange;

			sampler2D _CrackTex;
			float _CrackTopStrength;
			float _CrackDistance;
			float _CrackPow;
			uint _CrackParallexTimes;
			float4 _CrackColor;

			sampler2D _CameraDepthTexture;
			sampler2D _CameraGeometryTexture;
			float _BeginOpacity;
			float _DistortStrength;
			float _DepthDistance;
			float _DepthPow;
			float _FresnelPow;
            v2f vert (appdata v)
            {
                v2f o;
			    o.uv = TRANSFORM_TEX( v.uv,_MainTex);
			    o.pos = UnityObjectToClipPos(v.vertex);
			    o.worldPos = mul(unity_ObjectToWorld, v.vertex);

				float3x3 objectToTangent=float3x3(v.tangent.xyz,cross(v.normal,v.tangent)*v.tangent.w,v.normal);
				o.viewDir=mul(objectToTangent,ObjSpaceViewDir(v.vertex));
				o.lightDir=mul(objectToTangent,ObjSpaceLightDir(v.vertex));
				o.normal=mul(objectToTangent,v.normal);
			    TRANSFER_SHADOW(o);
				#if _OPACITY
				o.screenPos=ComputeScreenPos(o.pos);
				#endif
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				float3 normal=normalize(i.normal);
				float3 lightDir=normalize(i.lightDir);
				float3 viewDir=normalize(i.viewDir);
				float3 albedo=tex2D(_MainTex, i.uv);
				float2 thickOffset=-viewDir.xy/viewDir.z;
				
				#if _NORMALMAP
				float2 normalUV=i.uv;
				#if _THICK
				normalUV+=thickOffset*_Thickness;
				#endif
				normal= DecodeNormalMap(tex2D(_NormalTex,normalUV));
				#endif
				UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos)
				
				float diffuse=saturate( GetDiffuse(normal,lightDir,_Lambert,atten));
				float3 finalCol=_LightColor0.rgb*diffuse*albedo+UNITY_LIGHTMODEL_AMBIENT.xyz;

				#if _CRACK
				float crackAmount=0;
				#if _CRACKTOP
				crackAmount=tex2D(_CrackTex,i.uv)* _CrackColor.a* _CrackTopStrength;
				#endif
				#if _CRACKPARALLEX
				float parallexParam=1.0/_CrackParallexTimes;
				float offsetDistance=_CrackDistance/_CrackParallexTimes;
				float totalParallex=0;
				for(uint index=0u;index<_CrackParallexTimes;index++)
				{
					float distance=_CrackDistance*totalParallex;
					distance+=random2(frac(i.uv))*offsetDistance;
					float2 parallexUV=i.uv+thickOffset*distance;
					crackAmount+=tex2D(_CrackTex,parallexUV)*parallexParam*pow(1-totalParallex,_CrackPow);
					totalParallex+=parallexParam;
				}
				crackAmount=saturate(crackAmount*diffuse*_CrackColor.a);
				#endif
				finalCol= lerp(finalCol,_CrackColor*_LightColor0, crackAmount);
				#endif

				#if _SPECULAR
				float specular = GetSpecular(normal,lightDir,viewDir,_SpecularRange);
				specular*=diffuse;
				finalCol += _LightColor0.rgb*albedo *specular;
				#endif

				#if _OPACITY
				float opacity=0;
				float2 screenUV=i.screenPos.xy/i.screenPos.w;
				#if _DEPTH
				float depthOffset=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screenPos)).r - i.screenPos.w;
				depthOffset=saturate(depthOffset/_DepthDistance);
				depthOffset= pow(1-depthOffset,_DepthPow);
				opacity+=depthOffset;
				#endif
				#if _FRESNEL
				float NDV=dot(normal,viewDir);
				opacity+=pow(NDV,_FresnelPow)*.5;
				#endif
				opacity=lerp(1-_BeginOpacity,1,saturate(opacity));
				#if _DISTORT
				float2 screenDistort=normal.xy*_DistortStrength;
				screenDistort*=1-opacity;
				screenUV+=screenDistort;
				#endif
				float3 geometryTex=tex2D(_CameraGeometryTexture,screenUV);
				finalCol=lerp( finalCol,geometryTex,opacity);
				#endif
				
				return float4(finalCol,1);
            }
            ENDCG
        }

    }
}
