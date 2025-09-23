Shader "Runtime/Surface/DissolveStencilWriter"
{
    Properties
    {
        _DissolveMask("DissolveMask",2D)="white"{}
        _Dissolve("Dissolve Progress",Range(0,1))=0.5
        
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull",int)=2
        
		[Header(Stencil)]
        _Stencil ("Stencil ID", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("Stencil Comparison", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)]_StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
    }
    SubShader
    {
		Tags{"Queue" = "Transparent+1"}
        Blend Off
		ZWrite Off
        ZTest LEqual
		Cull [_Cull]
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

		HLSLINCLUDE


            #include "Assets/Shaders/Library/Common.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
                float4 color:COLOR;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float4 color:COLOR;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_DissolveMask);SAMPLER(sampler_DissolveMask);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_DissolveMask_ST)
                INSTANCING_PROP(float,_Dissolve)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX_FLOW_INSTANCE(v.uv,_DissolveMask);
                o.color=v.color;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);

                float dissolveProgress = INSTANCE(_Dissolve);
                float dissolveSample = SAMPLE_TEXTURE2D(_DissolveMask,sampler_DissolveMask,i.uv).r;

                float dissolveComparer = dissolveProgress - dissolveSample;
                clip(dissolveComparer);
                return 1;
            }
        ENDHLSL
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
        Pass
        {
            Tags {"LightMode" = "DepthOnly"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}
