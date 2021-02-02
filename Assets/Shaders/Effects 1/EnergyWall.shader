Shader "Game/Effects/EnergyShield"
{
	Properties
	{
		[Header(Color)]
	    _LineColor("Line Color", Color) =(0,0,1,1)
		_InsideColor("Inside Color",Color)=(0,0,.5,.5)
		_MaskTex("Mask Texture",2D)="white"{}

		[Header(Inner Glow)]
		[Toggle(_LineGLOW)]_EnableLineGlow("Enable Line Glow",float)=1
		[HDR]_LineGlow("Line Glow",Color)=(1,1,1,1)
		_LineGlowFrequency("Line Glow Frequency",Range(0,20))=5
		_LineGlowClip("Line Glow Clip",Range(0,1))=.5
		_LineGlowSpeed("Line Glow Speed",Range(0,5))=1

		[Header(Shield Break)]
		_BreakAmount("Break Amount",Range(0,1))=.5
		_BreakTex("Break Mask",2D)="white"{}

		[Header(Vertex Random Distort)]
		[Toggle(_VERTEXRANDOMDISTORT)]_EnableRandomDistort("Enable Random Distort",float)=1
		_DistortStrength("Distort Strength",Range(0,0.2))=0.01
		_DistortFrequency("Distort Frequency",Range(1,144))=30

		[Header(Shield Shadow)]
		_ShadowOffset("Shadow Offset",Vector)=(0.1,0.1,0.1,0)
		_ShadowStrength("Shadow Strength",Range(0,1))=1
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" "PreviewType" = "Plane"}
		ZWrite Off
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		CGINCLUDE
			#include "UnityCG.cginc"
			#include "../CommonInclude.cginc"
			#pragma shader_feature _VERTEXRANDOMDISTORT
			sampler2D _MaskTex;
			float4 _MaskTex_ST;
			float4 _InsideColor;
		    float4 _LineColor;
		    float _RimWidth;
		    float _EdgeMultiplier;
			
			float4 _LineGlow;
			float _LineGlowFrequency;
			float _LineGlowClip;
			float _LineGlowSpeed;

			sampler2D _BreakTex;
			float4 _BreakTex_ST;
			float _BreakAmount;

			#if _VERTEXRANDOMDISTORT
			float _DistortStrength;
			float _DistortFrequency;
			#endif

			
		ENDCG
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			float4 _ShadowOffset;
			float _ShadowStrength;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv:TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 uv:TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				#if _VERTEXRANDOMDISTORT
				v.vertex*=lerp(1-_DistortStrength,1+_DistortStrength,random3(v.vertex+floor(_Time.y*_DistortFrequency%_DistortFrequency)));
				#endif
				v.vertex+=_ShadowOffset;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv=float4(TRANSFORM_TEX(v.uv,_MaskTex),TRANSFORM_TEX(v.uv,_BreakTex));
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float alpha=step(_BreakAmount,tex2D(_BreakTex,i.uv.zw).r);
				alpha*=tex2D(_MaskTex,i.uv.xy).a*_ShadowStrength;
				return float4(0,0,0,alpha);
			}
			ENDCG
		}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature _LineGLOW
			
			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal:NORMAL;
				float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 pos:TEXCOORD0;
				float4 uv:TEXCOORD1;
				float3 viewDir:TEXCOORD2;
				float3 normal:TEXCOORD3;
				#if _DEPTHOFFSET
				float4 screenPos:TEXCOORD4;
				#endif
			};

			v2f vert (appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				#if _VERTEXRANDOMDISTORT
				v.vertex*=lerp(1-_DistortStrength,1+_DistortStrength,random3(v.vertex+floor(_Time.y*_DistortFrequency%_DistortFrequency)));
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.pos=v.vertex;
				o.uv=float4(TRANSFORM_TEX(v.uv,_MaskTex),TRANSFORM_TEX(v.uv,_BreakTex));
				o.viewDir=ObjSpaceViewDir(v.vertex);
				o.normal=v.normal;
				#if _DEPTHOFFSET
				o.screenPos=ComputeScreenPos(o.vertex);
				#endif
				return o;
			}
			fixed4 frag (v2f i) : SV_Target
			{
				float mask=tex2D(_MaskTex,i.uv.xy).a;
				float3 insideCol=_InsideColor;
				float3 lineCol=_LineColor;
				#if _LineGLOW
				float glowParam=abs(lerp(-1,1,frac(i.pos.y*_LineGlowFrequency+_Time.y*_LineGlowSpeed)));
			 	glowParam=smoothstep(_LineGlowClip,1,glowParam);
				lineCol=lerp(lineCol,_LineGlow,glowParam);
				#endif

				float breakAmount=step(_BreakAmount,tex2D(_BreakTex,i.uv.zw).r);
				mask*=breakAmount;
				return float4(lerp(insideCol,lineCol,mask),mask);
			}
			ENDCG
		}

	}
}
