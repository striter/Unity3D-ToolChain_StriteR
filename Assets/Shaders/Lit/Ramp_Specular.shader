Shader "Game/Lit/Ramp_Specular"
{
	Properties
	{
		_MainTex ("Main Tex", 2D) = "white" {}
		_Color("Color",Color)=(1,1,1,1)
		_Ramp("Ramp Tex",2D)="white"{}
		_Lambert("Diffuse Lambert",Range(0,1))=0.5
		[Header(Specular Setting)]
		[Toggle(_SPECULAR)]_EnableSpecular("Enable Specular",float)=1
		_SpecularColor("Specular Color",Color)=(1,1,1,1)
		_SpecularRange("Specular Range",Range(.9,1))=.98
		[Header(Rim Setting)]
		[KeywordEnum(None,Hard,Smooth)]_Rim("Rim Type",float)=1
		_RimColor("Rim Color",Color)=(1,1,1,1)
		_RimRange("Rim Range",Range(0,1))=0.5
	}
	SubShader
	{
		Tags {"RenderType"="Opaque" "Queue" = "Geometry"}
		CGINCLUDE
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "AutoLight.cginc"
		#include "../CommonLightingInclude.cginc"
		#pragma shader_feature _SPECULAR
		#pragma multi_compile _RIM_NONE _RIM_HARD _RIM_SMOOTH
		
		float4 _Color;
		float _Lambert;
		sampler2D _Ramp;
		sampler2D _MainTex;
		half4 _MainTex_ST;
		float4 _SpecularColor;
		float _SpecularRange;
		float4 _RimColor;
		float _RimRange;

		struct appdata
		{
			float4 vertex : POSITION;
			float3 normal:NORMAL;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
			float3 worldNormal:TEXCOORD1;
			float3 worldLightDir:TEXCOORD2;
			float3 worldViewDir:TEXCOORD3;
		};

		v2f vert (appdata v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX( v.uv, _MainTex);
			o.worldNormal=mul(unity_ObjectToWorld, v.normal);
			o.worldLightDir=WorldSpaceLightDir(v.vertex);
			o.worldViewDir=WorldSpaceViewDir(v.vertex);
			return o;
		}

		float4 frag (v2f i) : SV_Target
		{
			float3 normal=normalize(i.worldNormal);
			float3 lightDir=normalize(i.worldLightDir);
			float3 viewDir = normalize(i.worldViewDir);
			
			float3 albedo = tex2D(_MainTex, i.uv).rgb*_Color.rgb;
			float3 finalCol=0;

			float3 ambientCol = UNITY_LIGHTMODEL_AMBIENT.xyz;

			float diffuse = GetDiffuse(normal, lightDir,_Lambert);
			float3 diffuseCol = _LightColor0.rgb*tex2D(_Ramp, float2(diffuse, diffuse)).rgb;
				
			finalCol=albedo*( ambientCol+diffuseCol);
				
			#if _RIM_HARD
			float rim=saturate( dot(viewDir,normal));
			rim=step(rim,_RimRange)*step(0.01,_RimRange);
			finalCol=lerp(finalCol,_RimColor.rgb,rim);
			#elif _RIM_SMOOTH
			float rim=saturate( dot(viewDir,normal));
			rim=1-rim;
			rim*=_RimRange*3;
			finalCol=lerp(finalCol,_RimColor.rgb,rim);
			#endif
			
			#if _SPECULAR
			float specular = GetSpecular(normal,lightDir,viewDir,_SpecularRange);
			specular=1-step(specular,0);
			finalCol = lerp(finalCol,_SpecularColor.rgb,specular) ;
			#endif

			return fixed4(finalCol ,1);
		}
		ENDCG
		
		Pass
		{
			Tags{"LightMode" = "ForwardBase"}
			Cull Back
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			ENDCG
		}
	}
}
