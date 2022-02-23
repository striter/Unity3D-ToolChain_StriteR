Shader "Game/Particles/AlphaBlend"
{
    Properties
    {
        _MainTex("Main Tex",2D)="white"{}
        [Toggle(_MASK)]_Mask("Is Mask",int)=0
        [HDR] _Color("Color Tint",Color)=(1,1,1,1)
        [Vector2]_MainTexFlow("Flow",Vector)=(0,0,1,1)
        
        [Header(Optional)]
        [ToggleTex(_SECONDARY)]_SecondaryTex("Secondary Tex",2D)="white"{}
        [Vector2]_SecondaryTexFlow("Flow",Vector)=(1,1,1,1)
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull",int)=2
    }
    SubShader
    {
		Tags{"Queue" = "Transparent"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
		    ZWrite Off
		    ZTest Less
		    Cull [_Cull]
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local_fragment _MASK
            #pragma shader_feature_local_fragment _SECONDARY

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
                float4 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            TEXTURE2D(_SecondaryTex);SAMPLER(sampler_SecondaryTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_Color)
                INSTANCING_PROP(float4,_MainTex_ST)
                INSTANCING_PROP(float2,_MainTexFlow)
                INSTANCING_PROP(float4,_SecondaryTex_ST)
                INSTANCING_PROP(float2,_SecondaryTexFlow)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = float4(TRANSFORM_TEX_INSTANCE(v.uv, _MainTex)+_Time.y*INSTANCE(_MainTexFlow),TRANSFORM_TEX_INSTANCE(v.uv,_SecondaryTex)+_Time.y*INSTANCE(_SecondaryTexFlow));
                o.color=v.color;
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

                return float4(albedo,saturate(alpha));
            }
            ENDHLSL
        }
    }
}
