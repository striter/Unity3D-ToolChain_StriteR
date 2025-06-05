Shader "Hidden/VolumeShadowCasterPasses"
{
    Properties
    {
        _MainTex("Main Tex",2D)="white"{}
        _Color("Color Tint",Color)=(1,1,1,1)
    }
    SubShader
    {
        HLSLINCLUDE

            #include "Assets/Shaders/Library/Common.hlsl"
            float4 _ShadowVolumeParams;

            struct a2v
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
			    UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
			    UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            
            v2f vert (a2v v)
            {
                v2f o;
			    UNITY_SETUP_INSTANCE_ID(v);
			    UNITY_TRANSFER_INSTANCE_ID(v, o);
                float3 positionWS = TransformObjectToWorld(v.positionOS);
                float3 normalWS = TransformObjectToWorldNormal(v.normalOS);
                float3 lightDirWS = -normalize(_MainLightPosition);
                float potentialSilhouetteEdge = step(0,dot(normalWS,lightDirWS));
                positionWS += (potentialSilhouetteEdge * 100 + _ShadowVolumeParams.x ) * lightDirWS + potentialSilhouetteEdge * normalWS * _ShadowVolumeParams.y;
                o.positionCS = TransformWorldToHClip(positionWS);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
			    UNITY_SETUP_INSTANCE_ID(i);
                return 1;
            }
            
        ENDHLSL
        
        Pass
        {
            Name "PerObjectFront"
            Stencil {
                Comp Always
                Pass IncrWrap
            }
            Cull Back
            ZWrite Off
            ColorMask 0
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
        Pass
        {
            Name "PerObjectBack"
            Stencil {
                Comp Always
                Pass DecrWrap
            }
            Cull Front
            ColorMask 0
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            ENDHLSL
        }
        Pass
        {      
            Name "VolumeSample"
            Stencil {
                Ref 1
                Comp LEqual
                Pass Keep
            }
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex vertSample
            #pragma fragment fragSample


            struct a2vSample
            {
                float3 positionOS : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2fSample
            {
                float4 positionCS : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_Color)
            INSTANCING_BUFFER_END
            
            v2fSample vertSample (a2vSample v)
            {
                v2fSample o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = float4(v.positionOS.xyz, 1.0);
                #if UNITY_UV_STARTS_AT_TOP
                    o.positionCS.y *= -1;
                #endif
                return o;
            }

            float4 fragSample (v2fSample i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                return INSTANCE(_Color);
            }
            ENDHLSL
        }
    }
}
