Shader "Game/Effects/Outline"
{
    Properties
    {
        _OutlineColor("Color",Color)=(0,0,0,0)
        _OutlineWidth("Width",Range(0,1))=0.1
		[KeywordEnum(Normal,Tangent,UV1,UV2,UV3,UV4,UV5,UV6,UV7)]_NORMALSAMPLE("Source Vector",float)=0
		[Header(View Space Adapting)]
		[Toggle(_CLIPSPACEADPATION)]_ClipSpaceAdapt("Clip Space Adapting",float)=0
		_AdaptFactor("Adapting Factor(Pixel Multiply)",float)=100
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+1" }
        Pass 
        {	
		    Name "OutLine"
            ZWrite On
			Cull Front
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature _CLIPSPACEADPATION
			#pragma multi_compile _NORMALSAMPLE_NORMAL _NORMALSAMPLE_TANGENT _NORMALSAMPLE_UV1 _NORMALSAMPLE_UV2 _NORMALSAMPLE_UV3 _NORMALSAMPLE_UV4 _NORMALSAMPLE_UV5  _NORMALSAMPLE_UV6  _NORMALSAMPLE_UV7
			#include "UnityCG.cginc"
			float4 _OutlineColor;
			float _OutlineWidth;
			#if _CLIPSPACEADPATION
			float _AdaptFactor;
			#endif

			struct a2v
			{
				float4 vertex : POSITION;
				float3 normal:NORMAL;
				float4 tangent:TANGENT;
				#if _NORMALSAMPLE_UV1
				float3 uv1:TEXCOORD1;
				#elif _NORMALSAMPLE_UV2
				float3 uv2:TEXCOORD2;
				#elif _NORMALSAMPLE_UV3
				float3 uv3:TEXCOORD3;
				#elif _NORMALSAMPLE_UV4
				float3 uv4:TEXCOORD4;
				#elif _NORMALSAMPLE_UV5
				float3 uv5:TEXCOORD5;
				#elif _NORMALSAMPLE_UV6
				float3 uv6:TEXCOORD6;
				#elif _NORMALSAMPLE_UV7
				float3 uv7:TEXCOORD7;
				#endif
			};

			float4 vert(a2v v):SV_POSITION {
				float4 vertex=v.vertex;

				float3 normal=0;
				#if _NORMALSAMPLE_NORMAL
				normal=normalize(v.normal);
				#elif _NORMALSAMPLE_TANGENT
				normal=normalize(v.tangent);
				#elif _NORMALSAMPLE_UV1
				normal=normalize(v.uv1);
				#elif _NORMALSAMPLE_UV2
				normal=normalize(v.uv2);
				#elif _NORMALSAMPLE_UV3
				normal=normalize(v.uv3);
				#elif _NORMALSAMPLE_UV4
				normal=normalize(v.uv4);
				#elif _NORMALSAMPLE_UV5
				normal=normalize(v.uv5);
				#elif _NORMALSAMPLE_UV6
				normal=normalize(v.uv6);
				#elif _NORMALSAMPLE_UV7
				normal=normalize(v.uv7);
				#endif

				#if _NORMALSAMPLE_UV1||_NORMALSAMPLE_UV1||_NORMALSAMPLE_UV2||_NORMALSAMPLE_UV3||_NORMALSAMPLE_UV4||_NORMALSAMPLE_UV5||_NORMALSAMPLE_UV6||_NORMALSAMPLE_UV7
				float3x3 objectToTangent=float3x3(v.tangent.xyz,cross(v.normal,v.tangent)*v.tangent.w,v.normal);
				normal=mul(normal,objectToTangent);
				#endif


				#if _CLIPSPACEADPATION
				float4 clipPosition=UnityObjectToClipPos(vertex);
				float3 screenDir=mul((float3x3)UNITY_MATRIX_MVP, normal);
				float2 screenOffset=normalize(screenDir).xy/_ScreenParams.xy*clipPosition.w*_AdaptFactor;
				clipPosition.xy+=screenOffset*_OutlineWidth;
				return  clipPosition;
				#else
				normal=mul((float3x3)unity_ObjectToWorld,normal);
				float4 worldPos=mul(unity_ObjectToWorld,vertex);
				worldPos.xyz+=normal*_OutlineWidth;
				return UnityWorldToClipPos(worldPos);
				#endif
			}
			float4 frag() :SV_TARGET
			{
				return _OutlineColor;
			}
		ENDCG
		}
    }
}
