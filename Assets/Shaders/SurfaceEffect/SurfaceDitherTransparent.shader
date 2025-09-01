Shader "Runtime/Surface/DitherTransparent"
{
    Properties
    {
        _Scale("Scale",Range(1,10))=1
        _Transparency("Transparency",Range(0,1))=.5
		[Header(Stencil)]
        _Stencil ("Stencil ID", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("Stencil Comparison", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)]_StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
    }
    SubShader
    {
		Tags{"Queue" = "Geometry-1"}
        Pass
        {
            Blend Off
		    ZWrite Off
		    ZTest Less
		    Cull Back
            
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
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
            };

            INSTANCING_BUFFER_START
                INSTANCING_PROP(half,_Transparency)
                INSTANCING_PROP(half,_Scale)
            INSTANCING_BUFFER_END
            v2f vert (a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 screenPos = i.positionCS.xy/_Scale;
                clip(_Transparency-dither01(screenPos));
                return 1;
            }
            ENDHLSL
        }
    }
}
