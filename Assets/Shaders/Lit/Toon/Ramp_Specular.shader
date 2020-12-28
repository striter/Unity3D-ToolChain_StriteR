Shader "Game/Lit/Toon/Ramp_Specular"
{
	Properties
	{
		_MainTex ("Main Tex", 2D) = "white" {}
		_Color("Color",Color)=(1,1,1,1)
		[Header(Diffuse Setting)]
		[NoScaleOffset]_Ramp("Ramp Tex",2D)="white"{}
		[Toggle(_RAMP_RIM_V)]_RampRimV("2D Ramp(Rim as V)",float)=0
		[Header(Specular Setting)]
		[Toggle(_SPECULAR)]_EnableSpecular("Enable Specular",float)=1
		_SpecularColor("Specular Color",Color)=(1,1,1,1)
		_SpecularRange("Specular Range",Range(.9,1))=.98
		[Header(Rim Setting)]
		[KeywordEnum(None,Hard,Smooth)]_Rim("Rim Type",float)=1
		_RimRange("Rim Range",Range(0,1))=0.5
	}
	SubShader
	{
		Tags {"RenderType"="Opaque" "Queue" = "Geometry"}
		Cull Back
		Blend Off
		ZWrite On
		ZTest LEqual
		
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

		USEPASS "Hidden/ShadowCaster/MAIN"

		CGINCLUDE
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "AutoLight.cginc"
		#include "../../CommonLightingInclude.cginc"
		#pragma shader_feature _SPECULAR
		#pragma shader_feature _RAMP_RIM_V
		#pragma multi_compile _RIM_NONE _RIM_HARD _RIM_SMOOTH
		
		float4 _Color;
		sampler2D _Ramp;
		sampler2D _MainTex;
		half4 _MainTex_ST;
		float4 _SpecularColor;
		float _SpecularRange;
		float _RimRange;

		struct appdata
		{
			float4 vertex : POSITION;
			float3 normal:NORMAL;
			float3 tangent:TANGENT;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
			float3 normal:TEXCOORD1;
			float3 lightDir:TEXCOORD2;
			float3 viewDir:TEXCOORD3;
			float3 worldPos:TEXCOORD4;
			SHADOW_COORDS(5)
		};


		v2f vert (appdata v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX( v.uv, _MainTex);
			o.normal=v.normal;
			o.lightDir=ObjSpaceLightDir(v.vertex);
			o.viewDir=ObjSpaceViewDir(v.vertex);
			o.worldPos=mul(unity_ObjectToWorld,v.vertex);
			TRANSFER_SHADOW(o);
			return o;
		}
			

		float4 frag (v2f i) : SV_Target
		{
			float3 normal=normalize(i.normal);
			float3 lightDir=normalize(i.lightDir);
			float3 viewDir = normalize(i.viewDir);
			UNITY_LIGHT_ATTENUATION(atten,i,i.worldPos)
			
			float3 albedo = tex2D(_MainTex, i.uv).rgb*_Color.rgb+UNITY_LIGHTMODEL_AMBIENT.rgb;
			float diffuse = GetDiffuse(normal, lightDir);
			diffuse*=atten;
			float rim=dot(normal,viewDir);
			float2 rampUV=diffuse;
			#if _RAMP_RIM_V
			rampUV.y=rim;
			#endif
			float3 diffuseCol = _LightColor0.rgb*tex2D(_Ramp,rampUV).rgb;
			float3 finalCol=albedo*(diffuseCol+ UNITY_LIGHTMODEL_AMBIENT.xyz);

			#if _RIM_HARD
			rim=step(rim,_RimRange)*step(0.01,_RimRange);
			finalCol=lerp(finalCol,_LightColor0.rgb,rim);
			#elif _RIM_SMOOTH
			rim=(1-rim)*_RimRange*3;
			finalCol+= _LightColor0.rgb*rim;
			#endif
			
			#if _SPECULAR
			float specular = GetSpecular(normal,lightDir,viewDir,_SpecularRange);
			specular*=atten;
			specular=1-step(specular,0);
			finalCol += specular*_LightColor0.rgb ;
			#endif
			return fixed4(finalCol ,1);
		}
		ENDCG
	}
}
