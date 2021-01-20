Shader "Game/Effects/Depth/Ocean"
{
	Properties
	{
		[NoScaleOffset]_MainTex("Color UV TEX",2D) = "white"{}
		[NoScaleOffset]_DistortTex("Distort Texure",2D) = "white"{}
		_TexUVScale("Main Tex UV Scale",float)=10

		_Color("Color Tint",Color) = (1,1,1,1)
		_SpecularRange("Specular Range",Range(.90,1)) = 1
		[Header(Wave Settings)]
		_WaveFrequency("Wave Frequency",float)=10
		_WaveStrength("Wave Strength",float)=20
		_WaveDirection("Wave Direction",Vector)=(1,1,1,1)
		_FresnelParam("Fresnel: X | Base Y| Max Z| Scale ",Vector)=(1,1,1,1)
		_FoamColor("Foam Color",Color)=(1,1,1,1)
		_FoamDepthParam("Depth/Foam: X| FoamWidth Y | DepthStart Z | Depth Width",Vector)=(.2,.5,1.5,1)
	}
	SubShader
	{
		Tags {  "Queue"="Transparent-1"  "PreviewType"="Plane" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite On
		Cull Back
		CGINCLUDE
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "../../CommonLightingInclude.cginc"
		ENDCG
		Pass		//Base Pass
		{
			Tags{"LightMode" = "ForwardBase" }
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

			sampler2D _CameraDepthTexture;
			sampler2D _MainTex;
			float _TexUVScale;
			sampler2D _DistortTex;
			float4 _Color;
			float4 _FoamColor;

			float _WaveFrequency;
			float _WaveStrength;
			float4 _WaveDirection;
			float Wave(float3 worldPos)
			{
				return  sin(worldPos.xz*_WaveDirection +_Time.y*_WaveFrequency)*_WaveStrength;
			}


			float4 _FresnelParam;
			float FresnelOpacity(float3 normal, float3 viewDir) {
				return lerp( _FresnelParam.x ,_FresnelParam.y, saturate(  _FresnelParam.z* (1 - dot(normal, viewDir))));
			}
			
			float _SpecularRange;
			float Specular(float3 normal, float3 viewDir, float3 lightDir)
			{
				return GetSpecular(normal,lightDir,viewDir,_SpecularRange);
			}

			float4 _FoamDepthParam;
			float Foam(float depthOffset) {
				//return step( depthOffset, _FoamDepthParam.x);		//Toonic
				return smoothstep(_FoamDepthParam.x, 0, depthOffset);		//Realistic
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

				float fresnelOpacity = FresnelOpacity(normal, viewDir);

				float foam = Foam(linearDepthOffset) * _FoamColor.a;
				float specular =lerp(Specular(normal, viewDir, lightDir),0,foam);

				float3 color=tex2D(_MainTex, i.uv)*_Color;
				return float4( lerp(lerp( color,_LightColor0,specular), _FoamColor.rgb, foam),1);
			}
			ENDCG
		}
	}

}
