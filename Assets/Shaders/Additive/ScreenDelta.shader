Shader "Game/Additive/ScreenDelta"
{
    Properties
    {
        _Color("Color Tint",Color)=(1,1,1,1)
        [Vector2]_ScreenDelta("Delta",Vector)=(10,10,0,0)
        
        [Header(Misc)]
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Enum(Off,0,Front,1,Back,2)]_Cull("Cull",int)=2
		[Header(Stencil)]
		_Stencil("Stencil ID", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("Stencil Comparison", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)]_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
    }
    SubShader
    {
    	Tags { "Queue" = "Transparent" }

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}
        
    	Blend SrcAlpha OneMinusSrcAlpha
		Cull [_Cull]
		ZWrite [_ZWrite]
		ZTest [_ZTest]
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_Color)
                INSTANCING_PROP(float4,_MainTex_ST)
                INSTANCING_PROP(float2,_ScreenDelta)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                float4 positionCS = TransformObjectToHClip(v.positionOS);
                positionCS.xy += _ScreenDelta.xy /_ScreenParams.xy*positionCS.w;
                o.positionCS = positionCS;
                o.uv = TRANSFORM_TEX_INSTANCE(v.uv, _MainTex);
                o.color = TransformHClipToNDC(positionCS).y;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                return INSTANCE(_Color);
            }
            ENDHLSL
        }
        
        USEPASS "Game/Additive/DepthOnly/MAIN"
        USEPASS "Game/Additive/ShadowCaster/MAIN"
    }
}
