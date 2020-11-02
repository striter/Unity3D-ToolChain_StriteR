Shader "Game/Effects/Depth/River"
{
	Properties
	{
		[NoScaleOffset]_MainTex("Color UV TEX",2D) = "white"{}
		_TexUVScale("Main Tex UV Scale",float)=10
		_Color("Color Tint",Color) = (1,1,1,1)
		[NoScaleOffset]_DistortTex("Distort Texure",2D) = "white"{}
		_SpecularRange("Specular Range",Range(.90,1)) = 1
		_WaveParam("Wave: X|Strength Y|Frequency ZW|Direction",Vector) = (1,1,1,1)
		_DistortParam("Distort: X|Refraction Distort Y|Frequency Z|Specular Distort",Vector) = (1,1,1,1)
		_FresnelParam("Fresnel: X | Base Y| Max Z| Scale ",Vector)=(1,1,1,1)
		_FoamColor("Foam Color",Color)=(1,1,1,1)
		_FoamDepthParam("Depth/Foam: X| FoamWidth Y | DepthStart Z | Depth Width",Vector)=(.2,.5,1.5,1)
	}
	SubShader
	{
		Tags {  "Queue"="Transparent-1"  }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Cull Back
		CGINCLUDE
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "../../CommonLightingInclude.cginc"
		ENDCG
		Pass		//Base Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal:NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv:TEXCOORD0;
				float3 worldPos:TEXCOORD1;
				float4 screenPos:TEXCOORD2;
				float3 worldNormal:TEXCOORD3;
			};

			sampler2D _CameraGeometryTexture;
			sampler2D _CameraDepthTexture;
			sampler2D _MainTex;
			float _TexUVScale;
			sampler2D _DistortTex;
			float4 _Color;
			float4 _FoamColor;

			float4 _WaveParam;
			float Wave(float3 worldPos)
			{
				return  sin(worldPos.xz* _WaveParam.zw +_Time.y*_WaveParam.y)*_WaveParam.x;
			}

			float4 _DistortParam;
			float2 Distort(float2 uv)
			{
				return tex2D(_DistortTex, uv+ _WaveParam.zw *_Time.y*_DistortParam.y).rg*_DistortParam.x;
			}

			float4 _FresnelParam;
			float FresnelOpacity(float3 normal, float3 viewDir) {
				return lerp( _FresnelParam.x ,_FresnelParam.y, saturate(  _FresnelParam.z* (1 - dot(normal, viewDir))));
			}
			
			float _SpecularRange;
			float Specular(float2 distort, float3 normal, float3 viewDir, float3 lightDir)
			{
				return GetSpecular(normal,lightDir,viewDir,_SpecularRange)*distort.x*_DistortParam.z;
			}

			float4 _FoamDepthParam;
			float Foam(float depthOffset) {
				//return step( depthOffset, _FoamDepthParam.x);		//Toonic
				return smoothstep(_FoamDepthParam.x, 0, depthOffset);		//Realistic
			}

			float DepthOpacity(float depthOffset)
			{
				return smoothstep(_FoamDepthParam.y, _FoamDepthParam.y+ _FoamDepthParam.z,depthOffset);
			}

			float2 DepthDistort(float depthOffset, float2 distort)
			{
				return step(_FoamDepthParam.y, depthOffset) * distort;
			}

			v2f vert (appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.worldPos += float3(0, Wave(o.worldPos),0);
				o.uv = o.worldPos.xz / _TexUVScale;
				o.pos = UnityWorldToClipPos(o.worldPos);
				o.screenPos= ComputeScreenPos(o.pos);
				o.worldNormal =UnityObjectToWorldNormal(v.normal) ;
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				float3 normal = normalize(i.worldNormal);
				float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				float3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
				float linearDepthOffset = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screenPos)).r - i.screenPos.w;

				float2 distort = Distort(i.uv);

				float fresnelOpacity = FresnelOpacity(normal, viewDir);
				float depthOpacity = DepthOpacity(linearDepthOffset);
				float totalOpacity = saturate( fresnelOpacity + depthOpacity);

				float foam = Foam(linearDepthOffset) * _FoamColor.a;
				float4 foamColor = float4(_FoamColor.rgb * foam, 0);

				float specular =lerp(Specular(distort, normal, viewDir, lightDir),0,foam);
				float4 specularColor = float4(_LightColor0.rgb * specular, 0);

				float4 transparentTexture = tex2D(_CameraGeometryTexture, i.screenPos.xy / i.screenPos.w + DepthDistort(linearDepthOffset, distort) );
				float4 albedo = float4((tex2D(_MainTex, i.uv+distort)*_Color).rgb,1);
				return lerp(transparentTexture, albedo, totalOpacity)+ foamColor + specularColor;
			}
			ENDCG
		}
	}

}
