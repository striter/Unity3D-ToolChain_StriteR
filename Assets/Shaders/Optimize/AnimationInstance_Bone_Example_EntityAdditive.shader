Shader "Hidden/AnimationInstance_Bone_EntityAdditive"
{
    Properties
    {
        [HDR]_Color ("Color Tine", Color) = (1,1,1,.1)
        [Header(Shape)]
        _NoiseTex("Noise Tex",2D)="white"{}
        _NoiseStrength("Noise Strength",Range(0,5))=1.5
        _NoisePow("Noise Pow",Range(.1,5))=2
        [Header(Flow)]
        _NoiseFlowX("Noise Flow X",Range(-2,2))=.1
        _NoiseFlowY("Noise Flow Y",Range(-2,2))=.1
        
        [Header(Instance)]
        [NoScaleOffset] _InstanceAnimationTex("Animation Texture",2D)="black"{}
        _InstanceFrameBegin("Begin Frame",int)=0
        _InstanceFrameEnd("End Frame",int)=0
        _InstanceFrameInterpolate("Frame Interpolate",Range(0,1))=1
        [KeywordEnum(None,1Bone,2Bone)]_OPTIMIZE("Optimize",float)=0
    }
    SubShader
    {
        Name "Main"
        Tags { "RenderType" ="GeometryAdditive" "DisableBatching"="true" "Queue"="Geometry+100" }
        ZWrite Off
        Blend One One
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "../CommonInclude.hlsl"
            #include "../Optimize/AnimationInstanceInclude.hlsl"

            sampler2D _NoiseTex;
            INSTANCING_BUFFER_START
            INSTANCING_PROP(float4,_NoiseTex_ST)
            INSTANCING_PROP(float,_NoiseStrength)
            INSTANCING_PROP(float,_NoisePow)
            INSTANCING_PROP(float,_NoiseFlowX)
            INSTANCING_PROP(float,_NoiseFlowY)
            INSTANCING_PROP(float4,_Color)
            INSTANCING_BUFFER_END

            struct a2f
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                #if INSTANCING_ON
                float4 boneIndexes:TEXCOORD1;
                float4 boneWeights:TEXCOORD2;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert (a2f v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v,o);
                #if INSTANCING_ON
                SampleBoneInstance(v.boneIndexes,v.boneWeights, v.positionOS);
				#endif
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX_INSTANCE(v.uv,_NoiseTex);
                o.uv+=_Time.y*float2(INSTANCE(_NoiseFlowX),INSTANCE(_NoiseFlowY));
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float3 finalCol=INSTANCE(_Color).rgb*INSTANCE(_Color).a;
                float noise= tex2D(_NoiseTex,i.uv).r*INSTANCE(_NoiseStrength);
                noise=pow(abs(noise),INSTANCE(_NoisePow));

                finalCol*=noise;
                return float4(finalCol,1);           
            }
            ENDHLSL
        }
    }
}
