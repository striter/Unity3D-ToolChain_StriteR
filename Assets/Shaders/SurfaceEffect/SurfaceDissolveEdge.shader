Shader "Runtime/Surface/DissolveStencilEdge"
{
    Properties
    {
        _DissolveMask("DissolveMask",2D)="white"{}
        _Dissolve("Dissolve Progress",Range(0,1))=0.5
        _DissolveWidth("Dissolve Width",Range(0,0.5))=0.1
        [HDR]_DissolveEdgeColor("Dissolve Color",Color)=(1,1,1,1)
        
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
		Tags{"Queue" = "Transparent"}
        Pass
        {
            Blend One One
		    ZWrite Off
		    ZTest Less
		    Cull [_Cull]
            
            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
                INSTANCING_PROP(float,_DissolveWidth)
                INSTANCING_PROP(float4,_DissolveEdgeColor)
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

                float dissolveWidth = INSTANCE(_DissolveWidth);
                float dissolveProgress = INSTANCE(_Dissolve);
                float dissolveSample = SAMPLE_TEXTURE2D(_DissolveMask,sampler_DissolveMask,i.uv).r;

                dissolveProgress = lerp(-dissolveWidth - 0.01,1.01,dissolveProgress);
                float dissolveComparer = dissolveProgress + dissolveWidth - dissolveSample;
                clip(dissolveComparer);
                float dissolve = step(0,dissolveComparer) ;
                float edge = step(dissolveComparer,dissolveWidth) * dissolve;
                return edge;
            }
            ENDHLSL
        }
    }
}
