// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Valak/Character/Cel_V2"
{
	Properties
	{
		_BaseColor("Base Color", Color) = (1,1,1,0)
		_Base("Base", 2D) = "white" {}
		_SSSColor("SSS Color", Color) = (1,1,1,0)
		_SSS("SSS", 2D) = "white" {}
		_ILM("ILM", 2D) = "white" {}
		[Toggle(_MATCAP_ON)] _Matcap("Matcap", Float) = 0
		_MatcapTex("Matcap Tex", 2D) = "gray" {}
		_MatcapIntensity("Matcap Intensity ", Range( 0 , 5)) = 1
		_MatcapExp("Matcap Exp", Range( 0 , 10)) = 3
		[Toggle]_ShadowMap("Shadow Map", Float) = 1
		[Toggle]_VCShadowMap("VC Shadow Map", Float) = 1
		_Shadow1Min("Shadow 1 Min", Range( 0 , 1)) = 0.25
		_Shadow1Max("Shadow 1 Max", Range( 0 , 1)) = 0.3
		_Shadow2Min("Shadow 2 Min", Range( 0 , 1)) = 0.25
		_Shadow2Max("Shadow 2 Max", Range( 0 , 1)) = 0.3
		[Toggle(_DYNAMICHIGHLIGHT_ON)] _DynamicHighlight("Dynamic Highlight", Float) = 0
		_SpecularColor("Specular Color", Color) = (1,1,1,0)
		_Specular("Specular", Range( 0 , 1)) = 0
		_Gloss("Gloss", Range( 0 , 1)) = 0
		_SpecularStepMin("Specular Step Min", Range( 0 , 10)) = 0
		_SpecularStepMax("Specular Step Max", Range( 0 , 10)) = 1
		_SpecularIntensity("Specular Intensity ", Range( 0 , 10)) = 1
		_InsidelineColor("Inside line Color", Color) = (0.5377358,0.5377358,0.5377358,0)
		_OutlineColor("Outline Color", Color) = (0.3207547,0.3207547,0.3207547,0)
		_OutlineWidth("Outline Width", Float) = 0.5
		_OutlineZOffset("Outline Z Offset", Float) = 0
		_OutlineKeep("Outline Keep", Float) = 10
		[Toggle(_COMICLINE_ON)] _Comicline("Comic line", Float) = 0
		_Lineangle("Line angle", Float) = 135
		_Linedensity("Line density", Float) = 200
		[Toggle(_AMBIENTLIGHT_ON)] _Ambientlight("Ambient light", Float) = 0
		[Toggle(_WRAPLIGHT_ON)] _WrapLight("Wrap Light", Float) = 1
		[Toggle(_RIMLIGHT_ON)] _RimLight("Rim Light", Float) = 0
		[HDR]_RimLightColor("Rim Light Color", Color) = (1,1,1,0)
		_RimPowerExp("Rim Power Exp", Range( 0 , 10)) = 0.5
		_RimOffset("Rim Offset", Float) = 0.24
		[KeywordEnum(World,View,Custom)] _Lightingmode("Lighting mode", Float) = 0
		_LightDir("Light Dir", Vector) = (0,0,0,0)
		_CustomAttachLightColor("Custom Attach Light Color", Color) = (1,1,1,0)
		_CustomAttachLightIntensity("Custom Attach Light Intensity", Range( 0 , 1)) = 0
		[Toggle]_Hit("Hit", Float) = 0
		[HDR]_HitColor("Hit Color", Color) = (1,1,1,0)
		_Hitstepmin("Hit step min", Float) = 0.5
		_Hitstepmax("Hit step max", Float) = 0.6
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ }
		Cull Front
		CGPROGRAM
		#pragma target 3.0
		#pragma surface outlineSurf Outline  keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap nometa noforwardadd vertex:outlineVertexDataFunc 
		void outlineVertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float temp_output_58_0 = distance( ase_worldPos , _WorldSpaceCameraPos );
			float clampResult446 = clamp( temp_output_58_0 , 1.0 , _OutlineKeep );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 worldToObj89 = mul( unity_WorldToObject, float4( _WorldSpaceCameraPos, 1 ) ).xyz;
			float3 normalizeResult87 = normalize( ( ase_vertex3Pos - worldToObj89 ) );
			float3 outlineVar = ( ( ase_vertexNormal * ( v.color.g * ( _OutlineWidth * 0.001 ) * clampResult446 ) ) + ( normalizeResult87 * ( _OutlineZOffset * 0.001 * temp_output_58_0 ) * v.color.b ) );
			v.vertex.xyz += outlineVar;
		}
		inline half4 LightingOutline( SurfaceOutput s, half3 lightDir, half atten ) { return half4 ( 0,0,0, s.Alpha); }
		void outlineSurf( Input i, inout SurfaceOutput o )
		{
			float2 uv_Base = i.uv_texcoord * _Base_ST.xy + _Base_ST.zw;
			half4 tex2DNode35 = tex2D( _Base, uv_Base );
			float4 Base226 = tex2DNode35;
			o.Emission = ( Base226 * _OutlineColor ).rgb;
		}
		ENDCG
		

		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature _RIMLIGHT_ON
		#pragma shader_feature _WRAPLIGHT_ON
		#pragma multi_compile _LIGHTINGMODE_WORLD _LIGHTINGMODE_VIEW _LIGHTINGMODE_CUSTOM
		#pragma shader_feature _DYNAMICHIGHLIGHT_ON
		#pragma shader_feature _MATCAP_ON
		#pragma shader_feature _AMBIENTLIGHT_ON
		#pragma shader_feature _COMICLINE_ON
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			half2 uv_texcoord;
			float4 vertexColor : COLOR;
			float4 screenPos;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform half _Hit;
		uniform half4 _HitColor;
		uniform half _Hitstepmin;
		uniform half _Hitstepmax;
		uniform half4 _BaseColor;
		uniform sampler2D _Base;
		uniform float4 _Base_ST;
		uniform half4 _SSSColor;
		uniform sampler2D _SSS;
		uniform float4 _SSS_ST;
		uniform half _Shadow1Min;
		uniform half _Shadow1Max;
		uniform half3 _LightDir;
		uniform half _ShadowMap;
		uniform sampler2D _ILM;
		uniform float4 _ILM_ST;
		uniform half _VCShadowMap;
		uniform half _RimOffset;
		uniform half _RimPowerExp;
		uniform half4 _RimLightColor;
		uniform half4 _InsidelineColor;
		uniform half _Shadow2Min;
		uniform half _Shadow2Max;
		uniform half _SpecularStepMin;
		uniform half _SpecularStepMax;
		uniform half _Gloss;
		uniform half _Specular;
		uniform half _SpecularIntensity;
		uniform half4 _SpecularColor;
		uniform sampler2D _MatcapTex;
		uniform half _MatcapIntensity;
		uniform half _MatcapExp;
		uniform half _Linedensity;
		uniform half _Lineangle;
		uniform half4 _CustomAttachLightColor;
		uniform half _CustomAttachLightIntensity;
		uniform half _OutlineWidth;
		uniform half _OutlineKeep;
		uniform half _OutlineZOffset;
		uniform half4 _OutlineColor;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 Outline302 = 0;
			v.vertex.xyz += Outline302;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			float2 uv_Base = i.uv_texcoord * _Base_ST.xy + _Base_ST.zw;
			half4 tex2DNode35 = tex2D( _Base, uv_Base );
			float2 uv_SSS = i.uv_texcoord * _SSS_ST.xy + _SSS_ST.zw;
			half4 tex2DNode37 = tex2D( _SSS, uv_SSS );
			float4 temp_output_41_0 = ( _BaseColor * tex2DNode35 * _SSSColor * tex2DNode37 );
			float4 temp_output_40_0 = ( _BaseColor * tex2DNode35 );
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = Unity_SafeNormalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float3 viewToWorldDir475 = normalize( mul( UNITY_MATRIX_I_V, float4( _LightDir, 0 ) ).xyz );
			float3 normalizeResult479 = normalize( _LightDir );
			#if defined(_LIGHTINGMODE_WORLD)
				float3 staticSwitch476 = ase_worldlightDir;
			#elif defined(_LIGHTINGMODE_VIEW)
				float3 staticSwitch476 = viewToWorldDir475;
			#elif defined(_LIGHTINGMODE_CUSTOM)
				float3 staticSwitch476 = normalizeResult479;
			#else
				float3 staticSwitch476 = ase_worldlightDir;
			#endif
			half3 ase_worldNormal = WorldNormalVector( i, half3( 0, 0, 1 ) );
			half3 ase_normWorldNormal = normalize( ase_worldNormal );
			float2 uv_ILM = i.uv_texcoord * _ILM_ST.xy + _ILM_ST.zw;
			half4 tex2DNode7 = tex2D( _ILM, uv_ILM );
			float3 temp_output_13_0 = ( ase_normWorldNormal * lerp(1.0,tex2DNode7.g,_ShadowMap) * lerp(1.0,i.vertexColor.r,_VCShadowMap) );
			float dotResult2 = dot( staticSwitch476 , temp_output_13_0 );
			float smoothstepResult104 = smoothstep( _Shadow1Min , _Shadow1Max , dotResult2);
			float temp_output_250_0 = ( ( ( 1.0 - smoothstepResult104 ) + ase_lightAtten ) * ( ase_lightAtten * smoothstepResult104 ) * ase_lightAtten );
			float temp_output_288_0 = saturate( (-1.0 + (( 1.0 - (dotResult2*0.5 + 0.5) ) - 0.0) * (1.0 - -1.0) / (1.0 - 0.0)) );
			float temp_output_289_0 = ( temp_output_288_0 + temp_output_250_0 );
			#ifdef _WRAPLIGHT_ON
				float staticSwitch291 = temp_output_289_0;
			#else
				float staticSwitch291 = temp_output_250_0;
			#endif
			float4 lerpResult42 = lerp( temp_output_41_0 , temp_output_40_0 , staticSwitch291);
			float dotResult103 = dot( staticSwitch476 , ase_normWorldNormal );
			half3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float dotResult201 = dot( ase_worldNormal , ase_worldViewDir );
			float4 Base226 = tex2DNode35;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float4 RimLight230 = ( saturate( ( ( dotResult103 * ase_lightAtten ) * pow( ( 1.0 - saturate( ( dotResult201 + _RimOffset ) ) ) , _RimPowerExp ) * Base226.a ) ) * ( Base226 * ase_lightColor ) * _RimLightColor );
			float4 lerpResult419 = lerp( ( RimLight230 + temp_output_41_0 ) , ( RimLight230 + temp_output_40_0 ) , staticSwitch291);
			#ifdef _RIMLIGHT_ON
				float4 staticSwitch307 = lerpResult419;
			#else
				float4 staticSwitch307 = lerpResult42;
			#endif
			float4 Insideline333 = ( ( Base226 * ( 1.0 - tex2DNode7.a ) * _InsidelineColor ) + tex2DNode7.a );
			float smoothstepResult15 = smoothstep( _Shadow1Min , _Shadow1Max , dotResult103);
			float smoothstepResult67 = smoothstep( _Shadow2Min , _Shadow2Max , tex2DNode7.g);
			float clampResult70 = clamp( ( smoothstepResult15 + smoothstepResult67 ) , 0.5 , 1.0 );
			float3 Lightingmode477 = staticSwitch476;
			float3 normalizeResult4_g3 = normalize( ( ase_worldViewDir + ase_worldlightDir ) );
			#ifdef _DYNAMICHIGHLIGHT_ON
				float3 staticSwitch115 = normalizeResult4_g3;
			#else
				float3 staticSwitch115 = Lightingmode477;
			#endif
			float dotResult116 = dot( temp_output_13_0 , staticSwitch115 );
			float smoothstepResult137 = smoothstep( _SpecularStepMin , _SpecularStepMax , ( pow( dotResult116 , exp2( (0.0 + (_Gloss - 0.0) * (11.0 - 0.0) / (1.0 - 0.0)) ) ) * (0.0 + (_Specular - 0.0) * (30.0 - 0.0) / (1.0 - 0.0)) * tex2DNode7.b ));
			float4 Specular304 = ( temp_output_250_0 * ( Base226 * smoothstepResult137 * _SpecularIntensity * _SpecularColor * tex2DNode7.r ) );
			half4 temp_cast_5 = (_MatcapExp).xxxx;
			float4 Matcap327 = pow( ( tex2D( _MatcapTex, ( ( mul( UNITY_MATRIX_V, half4( ase_worldNormal , 0.0 ) ).xyz * 0.5 ) + 0.5 ).xy ) * tex2DNode37.a * _MatcapIntensity ) , temp_cast_5 );
			#ifdef _MATCAP_ON
				float4 staticSwitch329 = Matcap327;
			#else
				float4 staticSwitch329 = float4( 0,0,0,0 );
			#endif
			half3 temp_cast_6 = (1.0).xxx;
			UnityGI gi254 = gi;
			float3 diffNorm254 = ase_worldNormal;
			gi254 = UnityGI_Base( data, 1, diffNorm254 );
			float3 indirectDiffuse254 = gi254.indirect.diffuse + diffNorm254 * 0.0001;
			#ifdef _AMBIENTLIGHT_ON
				float3 staticSwitch426 = ( indirectDiffuse254 * 2.0 );
			#else
				float3 staticSwitch426 = temp_cast_6;
			#endif
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float cos369 = cos( ( _Lineangle * ( 6.28318548202515 / 360.0 ) ) );
			float sin369 = sin( ( _Lineangle * ( 6.28318548202515 / 360.0 ) ) );
			float2 rotator369 = mul( ( (float2( 0,0 ) + ((ase_screenPos).xy - float2( -1,-1 )) * (float2( 1,1 ) - float2( 0,0 )) / (float2( 1,1 ) - float2( -1,-1 ))) * _Linedensity ) - float2( 0.5,0.5 ) , float2x2( cos369 , -sin369 , sin369 , cos369 )) + float2( 0.5,0.5 );
			float temp_output_381_0 = (frac( rotator369 )).x;
			#ifdef _COMICLINE_ON
				float staticSwitch412 = round( saturate( ( ( saturate( (-1.0 + (temp_output_381_0 - 0.0) * (1.0 - -1.0) / (1.0 - 0.0)) ) + saturate( (-1.0 + (temp_output_381_0 - 1.0) * (1.0 - -1.0) / (0.0 - 1.0)) ) ) - (1.0 + (( (0.0 + (temp_output_288_0 - 0.0) * (5.0 - 0.0) / (1.0 - 0.0)) + ( (0.0 + (dotResult2 - 0.0) * (6.0 - 0.0) / (1.0 - 0.0)) * ase_lightAtten ) ) - 0.0) * (-1.0 - 1.0) / (1.0 - 0.0)) ) ) );
			#else
				float staticSwitch412 = 1.0;
			#endif
			float4 temp_output_297_0 = ( ( ( staticSwitch307 * Insideline333 * clampResult70 ) + Specular304 + staticSwitch329 ) * half4( staticSwitch426 , 0.0 ) * half4( ase_lightColor.rgb , 0.0 ) * staticSwitch412 );
			float4 temp_output_460_0 = ( _CustomAttachLightColor * ( temp_output_297_0 + staticSwitch291 ) );
			float4 lerpResult485 = lerp( temp_output_297_0 , temp_output_460_0 , _CustomAttachLightIntensity);
			c.rgb = lerpResult485.rgb;
			c.a = 1;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
			half4 temp_cast_0 = (0.0).xxxx;
			float3 ase_worldPos = i.worldPos;
			half3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			half3 ase_worldNormal = WorldNormalVector( i, half3( 0, 0, 1 ) );
			float fresnelNdotV428 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode428 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV428, 0.5 ) );
			float smoothstepResult429 = smoothstep( _Hitstepmin , _Hitstepmax , fresnelNode428);
			o.Emission = lerp(temp_cast_0,( _HitColor * smoothstepResult429 ),_Hit).rgb;
		}

		ENDCG
		CGPROGRAM
		#pragma exclude_renderers xbox360 xboxone ps4 psp2 n3ds wiiu 
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows exclude_path:deferred nolightmap  nodynlightmap nodirlightmap noforwardadd vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 screenPos : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.screenPos = ComputeScreenPos( o.pos );
				o.color = v.color;
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.screenPos = IN.screenPos;
				surfIN.vertexColor = IN.color;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16301
1921;1;1918;1016;2761.006;-3817.75;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;23;-2862.909,218.3032;Float;False;1822.407;1012.815;Comment;22;476;475;25;69;15;67;68;66;477;104;16;103;19;2;13;1;4;11;14;479;483;484;Dark side;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;43;-3036.751,-1033.203;Float;False;1265.847;1138.55;Comment;15;307;42;220;419;339;338;40;231;41;39;36;37;226;35;7;Texture;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;198;-3190.208,3289.042;Float;False;507.201;385.7996;Comment;3;201;200;199;N . V;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector3Node;25;-2844.455,563.6366;Float;False;Property;_LightDir;Light Dir;38;0;Create;True;0;0;False;0;0,0,0;16.36,19.6,35.6;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;14;-2804.364,915.0237;Float;False;Constant;_Float0;Float 0;4;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;7;-2997.451,-137.2376;Float;True;Property;_ILM;ILM;4;0;Create;True;0;0;False;0;None;c3beaa94b5b042940820a6131187a1ee;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;11;-2811.338,1004.05;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldNormalVector;4;-2807.553,724.8979;Float;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TransformDirectionNode;475;-2582.021,410.3053;Float;False;View;World;True;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalizeNode;479;-2529.489,587.6569;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;1;-2816.645,264.1791;Float;False;True;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;182;-2609.414,3275.523;Float;False;1822.463;545.2106;;18;230;195;193;194;190;227;192;202;188;189;186;187;185;184;183;418;449;448;Rim Light;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldNormalVector;199;-3142.208,3337.041;Float;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;200;-3132.208,3494.041;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ToggleSwitchNode;484;-2592.595,1004.142;Float;False;Property;_VCShadowMap;VC Shadow Map;10;0;Create;True;0;0;False;0;1;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;483;-2590.595,839.142;Float;False;Property;_ShadowMap;Shadow Map;9;0;Create;True;0;0;False;0;1;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;201;-2838.208,3417.041;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;183;-2529.414,3547.522;Float;False;Property;_RimOffset;Rim Offset;35;0;Create;True;0;0;False;0;0.24;0.6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;476;-2314.857,438.4073;Float;False;Property;_Lightingmode;Lighting mode;37;0;Create;True;0;0;False;0;1;0;0;True;;KeywordEnum;3;World;View;Custom;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-2315.904,714.4147;Float;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;411;-6151.756,192.1731;Float;False;3184.534;1163.287;Comment;30;413;412;399;397;385;390;346;408;389;388;387;344;383;407;410;345;381;376;369;368;357;374;363;375;366;355;361;362;358;469;Comic line;1,1,1,1;0;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;358;-6101.756,281.5878;Float;False;1;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;290;-1916.245,1492.314;Float;False;1118.24;252.0819;Comment;8;445;444;288;286;273;284;289;468;Warp Light;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;35;-3031.574,-771.025;Float;True;Property;_Base;Base;1;0;Create;True;0;0;False;0;None;84118ffe9b0e5b540b67543854089f14;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;184;-2321.414,3435.522;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;2;-2057.923,735.8172;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;361;-5834.476,764.7153;Float;False;Constant;_Float7;Float 7;33;0;Create;True;0;0;False;0;360;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;284;-1868.203,1528.166;Float;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;253;-2826.33,1480.155;Float;False;826.1912;358.0029;Comment;5;249;250;247;248;196;Shadows;1,1,1,1;0;0
Node;AmplifyShaderEditor.TauNode;362;-5824.05,641.0952;Float;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;226;-2602.43,-412.0369;Float;False;Base;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;355;-5862.056,283.1763;Float;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;185;-2161.415,3435.522;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;150;-3191.417,1969.38;Float;False;2608.184;553.0682;Comment;21;223;304;252;143;225;137;144;146;123;138;224;125;117;116;124;122;120;115;118;114;478;Specular;1,1,1,1;0;0
Node;AmplifyShaderEditor.DotProductOpNode;103;-2051.754,357.0641;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;321;-3191.865,2674.435;Float;False;1968.236;405.7325;Matcap;12;327;326;325;322;320;319;318;316;317;315;314;337;Matcap;1,1,1,1;0;0
Node;AmplifyShaderEditor.LightAttenuation;196;-2784.03,1644.759;Float;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;448;-2132.152,3671.065;Float;False;226;Base;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;187;-1985.416,3435.522;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;186;-2167.415,3557.522;Float;False;Property;_RimPowerExp;Rim Power Exp;34;0;Create;True;0;0;False;0;0.5;0.24;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;363;-5630.944,684.2871;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;273;-1647.914,1534.76;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;374;-5635.826,242.1732;Float;False;5;0;FLOAT2;0,0;False;1;FLOAT2;-1,-1;False;2;FLOAT2;1,1;False;3;FLOAT2;0,0;False;4;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;477;-2032.175,617.8566;Float;False;Lightingmode;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;375;-5643.353,493.0045;Float;False;Property;_Linedensity;Line density;29;0;Create;True;0;0;False;0;200;500;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;366;-5839.898,540.553;Float;False;Property;_Lineangle;Line angle;28;0;Create;True;0;0;False;0;135;135;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-1902.91,522.303;Float;False;Property;_Shadow1Max;Shadow 1 Max;12;0;Create;True;0;0;False;0;0.3;0.3;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;114;-3140.104,2104.21;Float;False;Blinn-Phong Half Vector;-1;;3;91a149ac9d615be429126c95e20753ce;0;0;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;315;-3141.865,2818.569;Float;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ViewMatrixNode;314;-3061.677,2724.435;Float;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;368;-5363.925,559.054;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;118;-2964.565,2277.67;Float;False;Property;_Gloss;Gloss;18;0;Create;True;0;0;False;0;0;0.086;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;286;-1487.919,1564.396;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;449;-1854.971,3575.139;Float;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.PowerNode;189;-1793.417,3435.522;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;188;-1809.417,3323.523;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;478;-3062.673,2019.682;Float;False;477;Lightingmode;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;357;-5413.724,287.6212;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-1902.91,410.3031;Float;False;Property;_Shadow1Min;Shadow 1 Min;11;0;Create;True;0;0;False;0;0.25;0.25;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;317;-2903.134,2754.22;Float;False;2;2;0;FLOAT4x4;0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;316;-2913.951,2882.595;Float;False;Constant;_Float6;Float 6;-1;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;120;-2618.618,2230.495;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;11;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;369;-5160.427,499.046;Float;True;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LightColorNode;192;-1573.143,3595.66;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;227;-1600.732,3524.415;Float;False;226;Base;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;288;-1288.152,1532.85;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;104;-1522.971,532.7139;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;115;-2730.224,2044.49;Float;False;Property;_DynamicHighlight;Dynamic Highlight;15;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;190;-1553.417,3403.522;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;468;-1261.925,1550.46;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;193;-1354.951,3498.307;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;249;-2725.977,1528.414;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;376;-4844.405,503.8114;Float;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;124;-2359.395,2335.591;Float;False;Property;_Specular;Specular;17;0;Create;True;0;0;False;0;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;418;-1380.066,3630.708;Float;False;Property;_RimLightColor;Rim Light Color;33;1;[HDR];Create;True;0;0;False;0;1,1,1,0;0,1.397576,2.118547,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;194;-1361.418,3403.522;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;116;-2381.882,2033.464;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;318;-2735.882,2790.667;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Exp2OpNode;122;-2377.173,2233.639;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;125;-2009.451,2303.415;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;30;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;381;-4594.501,503.2673;Float;True;True;False;False;False;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightAttenuation;345;-5079.618,1248.307;Float;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;117;-2176.922,2036.134;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;319;-2549.897,2833.627;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;248;-2499.055,1530.155;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;268;-438.4719,4102.588;Float;False;853.8689;472.1417;Comment;6;333;267;262;332;264;270;Inside line;1,1,1,1;0;0
Node;AmplifyShaderEditor.TFHCRemapNode;410;-5100.746,975.5924;Float;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;6;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;469;-5200.42,934.1182;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;195;-1169.419,3403.522;Float;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;247;-2510.457,1698.511;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;92;-3365.968,4112.561;Float;False;1202.085;496.5967;Outline Z offset;10;89;84;90;76;86;87;91;74;85;452;Outline Z offset;1,1,1,1;0;0
Node;AmplifyShaderEditor.TFHCRemapNode;407;-5107.626,749.1841;Float;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;344;-4684.593,1035.86;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;332;-396.8679,4169.315;Float;False;226;Base;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;383;-4311.047,418.0193;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;270;-416.5528,4351.747;Float;False;Property;_InsidelineColor;Inside line Color;22;0;Create;True;0;0;False;0;0.5377358,0.5377358,0.5377358,0;0.5377356,0.5377356,0.5377356,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;138;-1950.508,2220.469;Float;False;Property;_SpecularStepMax;Specular Step Max;20;0;Create;True;0;0;False;0;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;320;-2380.727,2776.46;Float;True;Property;_MatcapTex;Matcap Tex;6;0;Create;True;0;0;False;0;None;e17407f65cf3c62438cfb1268a0430d1;True;0;False;gray;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;123;-1858.64,2015.485;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;36;-2988.751,-985.2029;Float;False;Property;_BaseColor;Base Color;0;0;Create;True;0;0;False;0;1,1,1,0;1,1,1,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;264;-402.5721,4257.419;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;250;-2300.745,1551.721;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;230;-1002.133,3400.247;Float;False;RimLight;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;224;-1951.905,2136.704;Float;False;Property;_SpecularStepMin;Specular Step Min;19;0;Create;True;0;0;False;0;0;0.5;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;387;-4307.03,588.3881;Float;False;5;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;39;-3000.972,-560.0387;Float;False;Property;_SSSColor;SSS Color;2;0;Create;True;0;0;False;0;1,1,1,0;1,1,1,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;37;-3009.04,-385.1136;Float;True;Property;_SSS;SSS;3;0;Create;True;0;0;False;0;None;ce5bdb788fb93ca4690a1261cbc3e788;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;65;-2078.684,4103.059;Float;False;1545.714;530.3143;Comment;17;302;49;212;75;219;228;83;82;60;62;58;64;51;99;446;453;487;Outline;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;337;-2345.772,2984.718;Float;False;Property;_MatcapIntensity;Matcap Intensity ;7;0;Create;True;0;0;False;0;1;1.14;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;146;-1544.493,2303.616;Float;False;Property;_SpecularColor;Specular Color;16;0;Create;True;0;0;False;0;1,1,1,0;1,1,1,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;225;-1358.154,2042.188;Float;False;226;Base;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-2607.856,-857.4335;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-2589.231,-596.8903;Float;False;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;322;-2035.906,2779.604;Float;True;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;338;-2619.583,-954.4448;Float;False;230;RimLight;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;289;-1136.803,1550.303;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;99;-2056.681,4427.356;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SaturateNode;389;-4073.401,584.5897;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;68;-1895.22,723.0435;Float;False;Property;_Shadow2Min;Shadow 2 Min;13;0;Create;True;0;0;False;0;0.25;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;262;-116.8527,4231.589;Float;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;326;-2057.218,2994.365;Float;False;Property;_MatcapExp;Matcap Exp;8;0;Create;True;0;0;False;0;3;3.41;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;408;-4347.788,951.1954;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;388;-4079.024,428.6412;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;137;-1542.322,2034.193;Float;False;3;0;FLOAT;0;False;1;FLOAT;0.3;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos;85;-3339.593,4443.303;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;144;-1545.44,2195.009;Float;False;Property;_SpecularIntensity;Specular Intensity ;21;0;Create;True;0;0;False;0;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;231;-2654.731,-738.8726;Float;False;230;RimLight;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;66;-1895.22,835.0435;Float;False;Property;_Shadow2Max;Shadow 2 Max;14;0;Create;True;0;0;False;0;0.3;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;339;-2384.169,-917.6523;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;220;-2391.824,-718.3685;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;15;-1518.909,346.3031;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;325;-1736.317,2778.666;Float;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;346;-4036.908,971.8971;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;267;54.39929,4236.6;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;67;-1502.909,714.303;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;143;-1178.396,2118.321;Float;False;5;5;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;COLOR;0,0,0,0;False;4;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DistanceOpNode;58;-1839.544,4428.356;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;390;-3879.89,493.7452;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;291;-822.2664,1347.52;Float;False;Property;_WrapLight;Wrap Light;31;0;Create;True;0;0;False;0;0;1;1;True;;Toggle;2;Key0;Key1;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;335;-1615.691,-325.5499;Float;False;665.8931;418.3937;Comment;6;328;334;329;305;109;292;Combine;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;327;-1442.114,2775.385;Float;False;Matcap;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;42;-2187.373,-878.3295;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;453;-1722.149,4545.198;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;385;-3750.404,492.9981;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;84;-3326.94,4230.036;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;333;186.5772,4233.114;Float;False;Insideline;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;69;-1294.909,538.303;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;252;-984.2453,2092.747;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;419;-2186.965,-697.5607;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;64;-2052.078,4330.261;Float;False;Constant;_Float3;Float 3;12;0;Create;True;0;0;False;0;0.001;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;-2056.326,4259.895;Float;False;Property;_OutlineWidth;Outline Width;24;0;Create;True;0;0;False;0;0.5;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TransformPositionNode;89;-3031.318,4418.273;Float;False;World;Object;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;487;-1873.903,4558.557;Float;False;Property;_OutlineKeep;Outline Keep;26;0;Create;True;0;0;False;0;10;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;336;-914.6305,-46.54591;Float;False;844.1368;742.8821;Comment;13;461;460;457;297;426;257;424;425;254;299;459;485;486;Ambient light;1,1,1,1;0;0
Node;AmplifyShaderEditor.StaticSwitch;307;-2026.049,-787.8979;Float;False;Property;_RimLight;Rim Light;32;0;Create;True;0;0;False;0;0;0;1;True;;Toggle;2;Key0;Key1;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;70;-1155.505,535.0222;Float;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;90;-2753.539,4482.097;Float;False;Constant;_Float5;Float 5;12;0;Create;True;0;0;False;0;0.001;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.IndirectDiffuseLighting;254;-908.0306,92.05409;Float;False;World;1;0;FLOAT3;0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;299;-834.877,168.2581;Float;False;Constant;_Float1;Float 1;28;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;62;-1825.915,4283.553;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;328;-1565.691,-22.39603;Float;False;327;Matcap;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;397;-3587.537,481.6651;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;452;-2524.433,4539.017;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;86;-2735.672,4253.561;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;446;-1651.17,4470.79;Float;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;76;-2745.761,4377.229;Float;False;Property;_OutlineZOffset;Outline Z Offset;25;0;Create;True;0;0;False;0;0;20;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;334;-1562.766,-258.2184;Float;False;333;Insideline;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;304;-813.6015,2089.048;Float;False;Specular;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;305;-1334.802,-139.1514;Float;False;304;Specular;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;413;-3441.291,344.6234;Float;False;Constant;_Float8;Float 8;36;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;329;-1322.318,-45.15621;Float;False;Property;_Matcap;Matcap;5;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-1592.697,4317.086;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;424;-665.2502,118.3678;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;109;-1328.135,-275.5499;Float;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;91;-2524.865,4377.568;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;425;-844.9505,13.36784;Float;False;Constant;_Float10;Float 10;28;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;82;-1604.194,4162.584;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalizeNode;87;-2526.273,4280.161;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RoundOpNode;399;-3428.801,477.1835;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;292;-1104.798,-274.1344;Float;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;441;-1666.225,-1069.905;Float;False;1171.359;603.5595;Comment;8;442;443;430;431;439;427;429;428;Hit;1,1,1,1;0;0
Node;AmplifyShaderEditor.StaticSwitch;426;-540.1501,67.56786;Float;False;Property;_Ambientlight;Ambient light;30;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LightColorNode;257;-640.3001,232.2038;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.ColorNode;219;-1408.81,4382.621;Float;False;Property;_OutlineColor;Outline Color;23;0;Create;True;0;0;False;0;0.3207547,0.3207547,0.3207547,0;0.3207547,0.3207547,0.3207547,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-1371.845,4243.646;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-2306.94,4314.668;Float;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;412;-3201.964,446.3223;Float;False;Property;_Comicline;Comic line;27;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;228;-1379.647,4162.656;Float;False;226;Base;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;212;-1147.991,4268.609;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;75;-1132.325,4422.903;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;431;-1499.689,-578.746;Float;False;Property;_Hitstepmax;Hit step max;45;0;Create;True;0;0;False;0;0.6;0.6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;297;-270.0627,54.83368;Float;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;430;-1506.489,-665.0454;Float;False;Property;_Hitstepmin;Hit step min;44;0;Create;True;0;0;False;0;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;428;-1616.225,-897.1068;Float;True;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;429;-1228.881,-838.1004;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;459;-878.6904,406.8426;Float;False;Property;_CustomAttachLightColor;Custom Attach Light Color;40;0;Create;True;0;0;False;0;1,1,1,0;0.8490566,0.5347772,0.3484333,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;457;-574.0452,354.4161;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;427;-1227.571,-1019.906;Float;False;Property;_HitColor;Hit Color;43;1;[HDR];Create;True;0;0;False;0;1,1,1,0;1,1,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OutlineNode;49;-978.8374,4268.957;Float;False;2;False;None;0;0;Front;3;0;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;302;-761.9833,4268.872;Float;False;Outline;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;486;-593.1477,611.1644;Float;False;Property;_CustomAttachLightIntensity;Custom Attach Light Intensity;41;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;443;-898.222,-999.1614;Float;False;Constant;_Float11;Float 11;42;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;460;-490.3073,491.6186;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;439;-954.7665,-907.914;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;444;-959.1999,1542.818;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;445;-1251.16,1660.538;Float;False;Constant;_WarpLightIntensity;Warp Light Intensity ;44;0;Create;True;0;0;False;0;0.5;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;223;-1789.787,2326.659;Float;False;Constant;_Float4;Float 4;27;0;Create;True;0;0;False;0;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;202;-1690.425,3714.514;Float;False;Property;_RimLightIntensity;Rim Light Intensity;36;0;Create;True;0;0;False;0;1;4.47;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;461;-352.4383,295.7473;Float;False;Property;_CustomAttachLight1;Custom Attach Light1;39;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ToggleSwitchNode;442;-708.7581,-937.1234;Float;False;Property;_Hit;Hit;42;0;Create;True;0;0;False;0;0;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;303;-287.8802,890.6688;Float;False;302;Outline;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;485;-239.1477,488.1644;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,7.228886;Half;False;True;2;Half;ASEMaterialInspector;0;0;CustomLighting;Valak/Character/Cel_V2;False;False;False;False;False;False;True;True;True;False;False;True;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;True;True;True;True;True;True;True;False;False;False;False;False;False;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;True;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;475;0;25;0
WireConnection;479;0;25;0
WireConnection;484;0;14;0
WireConnection;484;1;11;1
WireConnection;483;0;14;0
WireConnection;483;1;7;2
WireConnection;201;0;199;0
WireConnection;201;1;200;0
WireConnection;476;1;1;0
WireConnection;476;0;475;0
WireConnection;476;2;479;0
WireConnection;13;0;4;0
WireConnection;13;1;483;0
WireConnection;13;2;484;0
WireConnection;184;0;201;0
WireConnection;184;1;183;0
WireConnection;2;0;476;0
WireConnection;2;1;13;0
WireConnection;284;0;2;0
WireConnection;226;0;35;0
WireConnection;355;0;358;0
WireConnection;185;0;184;0
WireConnection;103;0;476;0
WireConnection;103;1;4;0
WireConnection;187;0;185;0
WireConnection;363;0;362;0
WireConnection;363;1;361;0
WireConnection;273;0;284;0
WireConnection;374;0;355;0
WireConnection;477;0;476;0
WireConnection;368;0;366;0
WireConnection;368;1;363;0
WireConnection;286;0;273;0
WireConnection;449;0;448;0
WireConnection;189;0;187;0
WireConnection;189;1;186;0
WireConnection;188;0;103;0
WireConnection;188;1;196;0
WireConnection;357;0;374;0
WireConnection;357;1;375;0
WireConnection;317;0;314;0
WireConnection;317;1;315;0
WireConnection;120;0;118;0
WireConnection;369;0;357;0
WireConnection;369;2;368;0
WireConnection;288;0;286;0
WireConnection;104;0;2;0
WireConnection;104;1;19;0
WireConnection;104;2;16;0
WireConnection;115;1;478;0
WireConnection;115;0;114;0
WireConnection;190;0;188;0
WireConnection;190;1;189;0
WireConnection;190;2;449;3
WireConnection;468;0;288;0
WireConnection;193;0;227;0
WireConnection;193;1;192;0
WireConnection;249;0;104;0
WireConnection;376;0;369;0
WireConnection;194;0;190;0
WireConnection;116;0;13;0
WireConnection;116;1;115;0
WireConnection;318;0;317;0
WireConnection;318;1;316;0
WireConnection;122;0;120;0
WireConnection;125;0;124;0
WireConnection;381;0;376;0
WireConnection;117;0;116;0
WireConnection;117;1;122;0
WireConnection;319;0;318;0
WireConnection;319;1;316;0
WireConnection;248;0;249;0
WireConnection;248;1;196;0
WireConnection;410;0;2;0
WireConnection;469;0;468;0
WireConnection;195;0;194;0
WireConnection;195;1;193;0
WireConnection;195;2;418;0
WireConnection;247;0;196;0
WireConnection;247;1;104;0
WireConnection;407;0;469;0
WireConnection;344;0;410;0
WireConnection;344;1;345;0
WireConnection;383;0;381;0
WireConnection;320;1;319;0
WireConnection;123;0;117;0
WireConnection;123;1;125;0
WireConnection;123;2;7;3
WireConnection;264;0;7;4
WireConnection;250;0;248;0
WireConnection;250;1;247;0
WireConnection;250;2;196;0
WireConnection;230;0;195;0
WireConnection;387;0;381;0
WireConnection;40;0;36;0
WireConnection;40;1;35;0
WireConnection;41;0;36;0
WireConnection;41;1;35;0
WireConnection;41;2;39;0
WireConnection;41;3;37;0
WireConnection;322;0;320;0
WireConnection;322;1;37;4
WireConnection;322;2;337;0
WireConnection;289;0;288;0
WireConnection;289;1;250;0
WireConnection;389;0;387;0
WireConnection;262;0;332;0
WireConnection;262;1;264;0
WireConnection;262;2;270;0
WireConnection;408;0;407;0
WireConnection;408;1;344;0
WireConnection;388;0;383;0
WireConnection;137;0;123;0
WireConnection;137;1;224;0
WireConnection;137;2;138;0
WireConnection;339;0;338;0
WireConnection;339;1;40;0
WireConnection;220;0;231;0
WireConnection;220;1;41;0
WireConnection;15;0;103;0
WireConnection;15;1;19;0
WireConnection;15;2;16;0
WireConnection;325;0;322;0
WireConnection;325;1;326;0
WireConnection;346;0;408;0
WireConnection;267;0;262;0
WireConnection;267;1;7;4
WireConnection;67;0;7;2
WireConnection;67;1;68;0
WireConnection;67;2;66;0
WireConnection;143;0;225;0
WireConnection;143;1;137;0
WireConnection;143;2;144;0
WireConnection;143;3;146;0
WireConnection;143;4;7;1
WireConnection;58;0;99;0
WireConnection;58;1;85;0
WireConnection;390;0;388;0
WireConnection;390;1;389;0
WireConnection;291;1;250;0
WireConnection;291;0;289;0
WireConnection;327;0;325;0
WireConnection;42;0;41;0
WireConnection;42;1;40;0
WireConnection;42;2;291;0
WireConnection;453;0;58;0
WireConnection;385;0;390;0
WireConnection;385;1;346;0
WireConnection;333;0;267;0
WireConnection;69;0;15;0
WireConnection;69;1;67;0
WireConnection;252;0;250;0
WireConnection;252;1;143;0
WireConnection;419;0;220;0
WireConnection;419;1;339;0
WireConnection;419;2;291;0
WireConnection;89;0;85;0
WireConnection;307;1;42;0
WireConnection;307;0;419;0
WireConnection;70;0;69;0
WireConnection;62;0;51;0
WireConnection;62;1;64;0
WireConnection;397;0;385;0
WireConnection;452;0;453;0
WireConnection;86;0;84;0
WireConnection;86;1;89;0
WireConnection;446;0;58;0
WireConnection;446;2;487;0
WireConnection;304;0;252;0
WireConnection;329;0;328;0
WireConnection;60;0;11;2
WireConnection;60;1;62;0
WireConnection;60;2;446;0
WireConnection;424;0;254;0
WireConnection;424;1;299;0
WireConnection;109;0;307;0
WireConnection;109;1;334;0
WireConnection;109;2;70;0
WireConnection;91;0;76;0
WireConnection;91;1;90;0
WireConnection;91;2;452;0
WireConnection;87;0;86;0
WireConnection;399;0;397;0
WireConnection;292;0;109;0
WireConnection;292;1;305;0
WireConnection;292;2;329;0
WireConnection;426;1;425;0
WireConnection;426;0;424;0
WireConnection;83;0;82;0
WireConnection;83;1;60;0
WireConnection;74;0;87;0
WireConnection;74;1;91;0
WireConnection;74;2;11;3
WireConnection;412;1;413;0
WireConnection;412;0;399;0
WireConnection;212;0;228;0
WireConnection;212;1;219;0
WireConnection;75;0;83;0
WireConnection;75;1;74;0
WireConnection;297;0;292;0
WireConnection;297;1;426;0
WireConnection;297;2;257;1
WireConnection;297;3;412;0
WireConnection;429;0;428;0
WireConnection;429;1;430;0
WireConnection;429;2;431;0
WireConnection;457;0;297;0
WireConnection;457;1;291;0
WireConnection;49;0;212;0
WireConnection;49;1;75;0
WireConnection;302;0;49;0
WireConnection;460;0;459;0
WireConnection;460;1;457;0
WireConnection;439;0;427;0
WireConnection;439;1;429;0
WireConnection;444;0;289;0
WireConnection;444;1;445;0
WireConnection;461;1;297;0
WireConnection;461;0;460;0
WireConnection;442;0;443;0
WireConnection;442;1;439;0
WireConnection;485;0;297;0
WireConnection;485;1;460;0
WireConnection;485;2;486;0
WireConnection;0;2;442;0
WireConnection;0;13;485;0
WireConnection;0;11;303;0
ASEEND*/
//CHKSM=711D1F8A30CB8F6783877EB8D1D3B0DCA075E58C