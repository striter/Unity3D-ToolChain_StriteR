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
        Tags { "RenderType" ="GeometryAdditive" "Queue"="Geometry+100" }
        ZWrite Off
        Blend One One
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "../Optimize/AnimationInstanceInclude.cginc"

            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            float _NoiseStrength;
            float _NoisePow;
            float  _NoiseFlowX;
            float  _NoiseFlowY;

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                #if INSTANCING_ON
                float4 boneIndexes:TEXCOORD1;
                float4 boneWeights:TEXCOORD2;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                #if INSTANCING_ON
                SampleBoneInstance(v.boneIndexes,v.boneWeights, v.vertex);
				#endif
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv,_NoiseTex);
                o.uv+=_Time.y*float2(_NoiseFlowX,_NoiseFlowY);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 finalCol=_Color*_Color.a;
                float noise= tex2D(_NoiseTex,i.uv).r*_NoiseStrength;
                noise=pow(noise,_NoisePow);

                finalCol*=noise;
                return float4(finalCol,1);           
            }
            ENDCG
        }
    }
}
