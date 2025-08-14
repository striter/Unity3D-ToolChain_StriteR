Shader "Game/Particles/AlphaBlend"
{
    Properties
    {
        _MainTex("Main Tex",2D)="white"{}
        [Toggle(_MASK)]_Mask("Is Mask",int)=0
        [HDR] _Color("Color Tint",Color)=(1,1,1,1)
        
        [Header(Optional)]
        [ToggleTex(_SECONDARY)]_SecondaryTex("Secondary Tex",2D)="white"{}
        [Toggle(_SMOOTHPARTICLE)]_SmoothParticle("SmoothParticle",int) = 0
        [Foldout(_SMOOTHPARTICLE)]_SmoothParticleDistance("SmoothParticleDistance",Range(0,10)) = 2
        
    }
    SubShader
    {
		Tags{"Queue" = "Transparent"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha , Zero OneMinusSrcAlpha
		    ZWrite Off
		    ZTest LEqual
		    Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local_fragment _MASK
            #pragma shader_feature_local_fragment _SECONDARY
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
            TEXTURE2D(_SecondaryTex);SAMPLER(sampler_SecondaryTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_Color)
                INSTANCING_PROP(float4,_MainTex_ST)
                INSTANCING_PROP(float4,_SecondaryTex_ST)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = float4(TRANSFORM_TEX_FLOW_INSTANCE(v.uv, _MainTex),TRANSFORM_TEX_FLOW_INSTANCE(v.uv,_SecondaryTex));
                o.color=v.color;
                o.positionHCS = o.positionCS;
                o.positionWS = TransformObjectToWorld(v.positionOS);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float4 texSample=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv.xy);
                float3 albedo;
                float alpha;
                #if _MASK
                    albedo=texSample.r;
                    alpha=texSample.r;
                #else
                    albedo=texSample.rgb;
                    alpha=texSample.a;
                #endif

                #if _SECONDARY
                    float4 secTexSample=SAMPLE_TEXTURE2D(_SecondaryTex,sampler_SecondaryTex,i.uv.zw);
                    albedo*=secTexSample.rgb;
                    alpha*=secTexSample.a;
                #endif

                float4 color = i.color*INSTANCE(_Color);
                albedo*= color.rgb;
                alpha*=color.a;
                alpha*= SmoothParticleLinear(i.positionHCS,i.positionWS);
                return float4(albedo,saturate(alpha));
            }
            ENDHLSL
        }
    }
}
