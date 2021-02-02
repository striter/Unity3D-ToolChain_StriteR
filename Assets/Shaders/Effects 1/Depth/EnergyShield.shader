Shader "Game/Effects/EnergyShield"
{
	Properties
	{
		[Header(Color)]
	    [HDR]_RimColor("Rim Color", Color) =(1,1,1,1)
	    _RimWidth("Rim Width", Range(0.2,20.0)) = 3.0
	    _EdgeMultiplier("Rim Glow Multiplier", Range(0.0,9.0)) = 1.0
		_MaskTex("Mask Texture",2D)="white"{}
		[HDR]_InnerColor("Inner Color",Color)=(.5,.5,.5,.5)

		[Header(Inner Glow)]
		[Toggle(_INNERGLOW)]_EnableInnerGlow("Enable Inner Glow",float)=1
		[HDR]_InnerGlow("Inner Glow",Color)=(1,1,1,1)
		_InnerGlowFrequency("Inner Glow Frequency",Range(0,20))=5
		_InnerGlowClip("Inner Glow Clip",Range(0,1))=.5
		_InnerGlowSpeed("Inner Glow Speed",Range(0,5))=1

		[Header(Vertical Smoothen)]
		[Toggle(_VERTICALSMOOTHEN)]_EnableVerticalSmoothen("Enable Vertical Smoothen",float)=1
		_VerticalSmoothenStart("Vertical Smoothen Start",Range(0,1))=.48
		_VerticalSmoothenDistance("Vertical Smoothen Distance",Range(0,.5))=.1

		[Header(Depth)]
		[Toggle(_DEPTHOFFSET)]_EnableDepthOffset("Enable Depth Offset",float)=1
		_DepthMultiplier("Depth Multiplier",Range(0,1))=.5

		[Header(Vertex Random Distort)]
		[Toggle(_VERTEXRANDOMDISTORT)]_EnableRandomDistort("Enable Random Distort",float)=1
		_DistortStrength("Distort Strength",Range(0,0.2))=0.01
		_DistortFrequency("Distort Frequency",Range(1,144))=30
	}
	SubShader
	{	
		Tags { "RenderType"="EnergyShield" "Queue"="Transparent" }
		Pass
		{
			Name "MAIN"
			ZWrite Off
			Cull Back
			Blend One One
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature _DEPTHOFFSET
			#pragma shader_feature _VERTICALSMOOTHEN
			#pragma shader_feature _INNERGLOW
			#pragma shader_feature _VERTEXRANDOMDISTORT
			#include "UnityCG.cginc"
			#include "../../CommonInclude.cginc"
			
			sampler2D _MaskTex;
			float4 _MaskTex_ST;
			float4 _InnerColor;
		    float4 _RimColor;
		    float _RimWidth;
		    float _EdgeMultiplier;
			
			float4 _InnerGlow;
			float _InnerGlowFrequency;
			float _InnerGlowClip;
			float _InnerGlowSpeed;

			#if _VERTICALSMOOTHEN
			float _VerticalSmoothenStart;
			float _VerticalSmoothenDistance;
			#endif

			#if _DEPTHOFFSET
			sampler2D _CameraDepthTexture;
			float _DepthMultiplier;
			#endif

			#if _VERTEXRANDOMDISTORT
			float _DistortStrength;
			float _DistortFrequency;
			#endif

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
				float2 uv:TEXCOORD1;
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
				o.uv=TRANSFORM_TEX(v.uv,_MaskTex);
				o.viewDir=ObjSpaceViewDir(v.vertex);
				o.normal=v.normal;
				#if _DEPTHOFFSET
				o.screenPos=ComputeScreenPos(o.vertex);
				#endif
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 finalCol=0;
				float3 outerCol=_RimColor;
				float3 innerCol=_InnerColor;
				#if _INNERGLOW
				float glowParam=abs(lerp(-1,1,frac(i.pos.y*_InnerGlowFrequency+_Time.y*_InnerGlowSpeed)));
			 	glowParam=smoothstep(_InnerGlowClip,1,glowParam);
				innerCol=lerp(innerCol,_InnerGlow,glowParam);
				#endif

				innerCol*=tex2D(_MaskTex,i.uv).r;

				float outerRim = pow(1-dot(normalize(i.viewDir),normalize(i.normal)),_RimWidth)*_EdgeMultiplier;

				#if _DEPTHOFFSET
				float worldDepthDst=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screenPos)).r-i.screenPos.w;

				float depthOffset=pow(1-worldDepthDst*PI,_RimWidth)*_EdgeMultiplier*_DepthMultiplier;
				outerRim=max(outerRim,depthOffset);
				#endif

				outerRim=saturate(outerRim);
				finalCol= lerp(innerCol,outerCol, outerRim);
				
				#if _VERTICALSMOOTHEN
				float verticalParam=abs(i.pos.y);
				verticalParam=saturate(invlerp(_VerticalSmoothenStart,_VerticalSmoothenStart+_VerticalSmoothenDistance,verticalParam));
				finalCol=lerp(finalCol,outerCol,verticalParam);
				#endif
				return float4(finalCol,1);
			}
			ENDCG
		}
	}
}
