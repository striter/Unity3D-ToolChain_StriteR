Shader "Game/Additive/Outline"
{
    Properties
    {
        _OutlineColor("Color",Color)=(0,0,0,1)
        _OutlineWidth("Width",Range(0,1))=0.1
		[KeywordEnum(Normal,Tangent,UV1,UV2,UV3,UV4,UV5,UV6,UV7)]_NORMALSAMPLE("Source Vector",float)=0
    	[Toggle(_DISTANCEFADE)]_DISTANCEFADE("DistanceFade",int)=10
		[MinMaxRange]_DistanceFade("Range",Range(10,100))=20
		[HideInInspector]_DistanceFadeEnd("",float)=0.15
    	
    	
		[Toggle(_CLIPSPACEADPATION)]_ClipSpaceAdapt("Clip Space Adapting",int)=0
		_AdaptFactor("Adapting Factor(Pixel Multiply)",float)=100
    	
    	[Header(Misc)]
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
		[Header(Stencil)]
		_Stencil("Stencil ID", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("Stencil Comparison", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)]_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+1" }
        Pass 
        {	
			Stencil
			{
				Ref[_Stencil]
				Comp[_StencilComp]
				Pass[_StencilOp]
				ReadMask[_StencilReadMask]
				WriteMask[_StencilWriteMask]
			}
        	
		    Name "OutLine"
            ZWrite [_ZWrite]
        	ZTest [_ZTest]
			Cull Front
        	Blend SrcAlpha OneMinusSrcAlpha
        	
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma shader_feature_local_vertex _CLIPSPACEADPATION
			#pragma shader_feature_local_vertex _DISTANCEFADE
			#pragma multi_compile_local _NORMALSAMPLE_NORMAL _NORMALSAMPLE_TANGENT _NORMALSAMPLE_UV1 _NORMALSAMPLE_UV2 _NORMALSAMPLE_UV3 _NORMALSAMPLE_UV4 _NORMALSAMPLE_UV5  _NORMALSAMPLE_UV6  _NORMALSAMPLE_UV7
			#include "Assets/Shaders/Library/Common.hlsl"

			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_OutlineColor)
				INSTANCING_PROP(float,_OutlineWidth)
				INSTANCING_PROP(float,_AdaptFactor)
				INSTANCING_PROP(float,_DistanceFade)
				INSTANCING_PROP(float,_DistanceFadeEnd)
			INSTANCING_BUFFER_END

			struct a2v
			{
				float3 positionOS : POSITION;
				float3 normalOS:NORMAL;
				float4 tangentOS:TANGENT;
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
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS:SV_POSITION;
				float4 color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f vert(a2v v) {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				
				float3 positionOS=v.positionOS;
				float3 normalOS=0;
			#if _NORMALSAMPLE_NORMAL
				normalOS=normalize(v.normalOS);
			#elif _NORMALSAMPLE_TANGENT
				normalOS=normalize(v.tangentOS.xyz);
			#elif _NORMALSAMPLE_UV1
				normalOS=normalize(v.uv1);
			#elif _NORMALSAMPLE_UV2
				normalOS=normalize(v.uv2);
			#elif _NORMALSAMPLE_UV3
				normalOS=normalize(v.uv3);
			#elif _NORMALSAMPLE_UV4
				normalOS=normalize(v.uv4);
			#elif _NORMALSAMPLE_UV5
				normalOS=normalize(v.uv5);
			#elif _NORMALSAMPLE_UV6
				normalOS=normalize(v.uv6);
			#elif _NORMALSAMPLE_UV7
				normalOS=normalize(v.uv7);
			#endif

			#if _NORMALSAMPLE_UV1||_NORMALSAMPLE_UV1||_NORMALSAMPLE_UV2||_NORMALSAMPLE_UV3||_NORMALSAMPLE_UV4||_NORMALSAMPLE_UV5||_NORMALSAMPLE_UV6||_NORMALSAMPLE_UV7
				float3x3 TBNOS=float3x3(v.tangentOS.xyz,cross(v.normalOS,v.tangentOS.xyz)*v.tangentOS.w,v.normalOS);
				normalOS=mul(normalOS,TBNOS);
			#endif
				
			#if _CLIPSPACEADPATION
				float4 clipPosition=TransformObjectToHClip(positionOS);
				float3 normalCS = mul((float3x3)UNITY_MATRIX_MVP, normalOS);
				float2 screenOffset = normalize(normalCS.xy)/_ScreenParams.xy*clipPosition.w*INSTANCE(_AdaptFactor);
				clipPosition.xy+=screenOffset*INSTANCE(_OutlineWidth);
				o.positionCS= clipPosition;
			#else
				float3 normalWS = normalize(mul((float3x3)unity_ObjectToWorld,normalOS));
				float3 worldPos=TransformObjectToWorld(positionOS);
				worldPos+=normalWS*INSTANCE(_OutlineWidth);
				o.positionCS= TransformWorldToHClip(worldPos);
			#endif

				o.color = 1;
			#if _DISTANCEFADE
				float4 positionVS = TransformObjectToView(v.positionOS);
				o.color.a *= saturate(invlerp(INSTANCE(_DistanceFadeEnd),INSTANCE(_DistanceFade),-positionVS.z));
			#endif
				return o;
			}
			float4 frag(v2f i) :SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				return INSTANCE(_OutlineColor)*i.color;
			}
			ENDHLSL
		}
    }
}
