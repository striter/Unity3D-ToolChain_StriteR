Shader "Game/Particles/Additive"
{
    Properties
    {
        _MainTex("Main Tex",2D)="white"{}
        [HDR] _Color("Color Tint",Color)=(1,1,1,1)
        
        [Header(Mask)]
        _MaskTex("Mask Tex",2D)="white"{}
        
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull",int)=2
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=4

        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255
        _ColorMask("Color Mask", Float) = 15
        [Toggle(_SMOOTHPARTICLE)]_SmoothParticle("SmoothParticle",int) = 0
        [Foldout(_SMOOTHPARTICLE)]_SmoothParticleDistance("SmoothParticleDistance",Range(0,10)) = 2

    }
    SubShader
    {
		Tags{"Queue" = "Transparent"}
        Stencil
        {
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }
        ColorMask[_ColorMask]
        
         Pass
        {
            Blend One One
		    ZWrite Off
		    ZTest [_ZTest]
		    Cull [_Cull]
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local_fragment _SMOOTHPARTICLE

            #include "Assets/Shaders/Library/Particle.hlsl"

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
                float4 uv : TEXCOORD0;
                float3 positionWS:TEXCOORD1;
                float4 positionHCS:TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);SAMPLER(sampler_MaskTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_Color)
                INSTANCING_PROP(float4,_MainTex_ST)
                INSTANCING_PROP(float4,_MaskTex_ST)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = float4(TRANSFORM_TEX_FLOW_INSTANCE(v.uv, _MainTex),TRANSFORM_TEX_FLOW_INSTANCE(v.uv,_MaskTex));
                o.color=v.color;
                o.positionHCS = o.positionCS;
                o.positionWS = TransformObjectToWorld(v.positionOS);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);

                float4 color =SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv.xy)* i.color*INSTANCE(_Color);

                float mask = SAMPLE_TEXTURE2D(_MaskTex,sampler_MaskTex,i.uv.zw).r;
                
                mask*= SmoothParticleLinear(i.positionHCS,i.positionWS);
                return float4(color.rgb*color.a*mask,1);
            }
            ENDHLSL
        }
    }
}
