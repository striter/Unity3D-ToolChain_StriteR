Shader "Raymarch/BoxVolumetricFog"
{
    Properties
    {
        _Color("Color",Color)=(1,1,1,1)
        _MarchDistance("March Distance",Range(0,50))=5
        [KeywordEnum(T16,T32,T64,T128,T256,T512)]_March("March Times",float)=5
        _Noise("Noise 3D",3D)="white"{}
        _NoiseDensity("Noise Density",Range(0,5))=1
        _NoiseScale("Noise Scale",Vector)=(1,1,1,1)
        _NoiseFlow("Noise Flow",Vector)=(0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderQueue"="Transparent" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _MARCH_T16 _MARCH_T32 _MARCH_T64 _MARCH_T128 _MARCH_T256 _MARCH_T512

            #include "UnityCG.cginc"
            #include "RaymarchInclude.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float3 worldPos:TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float3 viewDir:TEXCOORD2;
                float3 origin:TEXCOORD3;
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            float _MarchDistance;
            sampler3D _Noise;
            float _NoiseDensity;
            float4 _NoiseScale;
            float4 _NoiseFlow;
            sampler2D _CameraDepthTexture;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.origin=mul(unity_ObjectToWorld,float4(0,0,0,1));
                o.worldPos=mul(unity_ObjectToWorld,v.vertex);
                o.viewDir=WorldSpaceViewDir(v.vertex);
                o.screenPos=ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half2 uv = i.screenPos.xy/i.screenPos.w;

                float3 marchStart=i.worldPos;
                half marchDst=RayBoxDistance(mul(unity_ObjectToWorld,float3(-.5,-.5,-.5)),mul(unity_ObjectToWorld,float3(.5,.5,.5)),i.worldPos-i.origin,-normalize(i.viewDir) ).y;
                half depthDst=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screenPos)).r-i.screenPos.w;
                half marchDistance= min(marchDst, depthDst);
                
                float march=0;
                if(marchDistance>0)
                {
                    #if _MARCH_T16
                    int marchTimes=16;
                    #elif _MARCH_T32
                    int marchTimes=32;
                    #elif _MARCH_T64
                    int marchTimes=64;
                    #elif _MARCH_T128
                    int marchTimes=128;
                    #elif _MARCH_T256
                    int marchTimes=256;
                    #elif _MARCH_T512
                    int marchTimes=512;
                    #endif

                    float3 direction=normalize(-i.viewDir);
                    direction= (direction*_MarchDistance)/marchTimes;
                    float marchOffset=1.0/marchTimes;
                    float distanceOffset=length(direction);

                    for(int index=0;index<marchTimes;index++)
                    {
                        if(marchDistance>0&&march<1)
                        {
                            float3 marchPos=marchStart+direction*index;
                            float noise=saturate( tex3D(_Noise,marchPos/_NoiseScale+_NoiseFlow*_Time.y).r*_NoiseDensity);
                            march=saturate(march+ marchOffset* noise);
                            marchDistance-=distanceOffset;
                        }
                    }
                }

                return _Color*march;
            }
            ENDCG
        }
    }
}
