Shader "Game/Effects/Outline"
{
    Properties
    {
        _OutlineColor("Color",Vector)=(0,0,0,0)
        _OutlineWidth("Width",Range(0,1))=0.1
		[_Header(View Space Adapting)]
		[Toggle(_CLIPSPACEADPATION)]_ClipSpaceAdapt("Clip Space Adapting",float)=0
		_AdaptFactor("Adapting Factor(Pixel Multiply)",float)=100

		[Header(Break Fixing)]
		[Toggle(_BREAKFIXING)]_BreakFixing("Break Normal Fixing",float)=0
        _FixFactor("Fix Factor",Range(0,1))=1
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
			#pragma shader_feature _BREAKFIXING
			#include "UnityCG.cginc"
			float4 _OutlineColor;
			float _OutlineWidth;
			#if _CLIPSPACEADPATION
			float _AdaptFactor;
			#endif
			#if _BREAKFIXING
			float _FixFactor;
			#endif

			float4 vert(float4 vertex:POSITION,float3 normal:NORMAL):SV_POSITION {
				normal=normalize(normal);
				
				#if _BREAKFIXING
				float3 vertexDir = normalize(vertex.xyz);
				vertexDir = vertexDir * sign(dot(normal, vertexDir));
				normal = normal* (1 - _FixFactor)  + vertexDir* _FixFactor ;
				#endif

				#if _CLIPSPACEADPATION
				float4 clipPosition=UnityObjectToClipPos(vertex);
				float3 screenDir=mul((float3x3)UNITY_MATRIX_MVP, normal);
				float2 screenOffset=normalize(screenDir).xy/_ScreenParams.xy*clipPosition.w*_AdaptFactor;
				clipPosition.xy+=screenOffset*_OutlineWidth;
				return clipPosition;
				#else
				normal=mul((float3x3)unity_ObjectToWorld,normal);
				float4 worldPos=mul(unity_ObjectToWorld,vertex);
				worldPos.xyz+=normal*_OutlineWidth;
				return UnityWorldToClipPos(worldPos);
				#endif
			}
			float4 frag(float4 position:SV_POSITION) :COLOR
			{
				return _OutlineColor;
			}
		ENDCG
		}
    }
}
