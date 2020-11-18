Shader "Game/Effects/Depth/Raymarch_VolumetricFog_Box"
{
    Properties
    {
        _Color("Color",Color)=(1,1,1,1)
        _Distance("March Distance",Range(0,500))=5
        _Density("Density",Range(0,5))=1
        _DensityClip("_Density Clip",Range(0,1))=.2
        [Enum(_16,16,_32,32,_64,64,_128,128,_256,256,_512,512)]_RayMarch("Ray March Times",int)=128
        _Noise("Noise 3D",3D)="white"{}
        _NoiseScale("Noise Scale",Vector)=(50,50,50,1)
        _NoiseFlow("Noise Flow",Vector)=(0,0,0,1)
    }
    SubShader
    {
        Tags{"Queue"="Transparent"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "../../CommonInclude.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos:TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float3 worldViewDir:TEXCOORD2;
                float3 worldLightDir:TEXCOORD3;
                float3 minBound:TEXCOORD4;
                float3 maxBound:TEXCOORD5;
            };

            int _RayMarch;
            float4 _Color;
            float _Distance;
            float _Density;
            float _DensityClip;
            
            sampler2D _CameraDepthTexture;

            sampler3D _Noise;
            float4 _NoiseScale;
            float4 _NoiseFlow;
            float SampleDensity(float3 worldPos)  {
                return smoothstep(_DensityClip,1,saturate(tex3Dlod(_Noise,float4( worldPos/_NoiseScale+_NoiseFlow*_Time.y,0)).r));
            }


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos=ComputeScreenPos(o.vertex);
                o.worldPos=mul(unity_ObjectToWorld,v.vertex);
                o.worldViewDir=WorldSpaceViewDir(v.vertex);
                o.worldLightDir=WorldSpaceLightDir(v.vertex);
                o.minBound=mul(unity_ObjectToWorld,float4(-.5,-.5,-.5,1));
                o.maxBound=mul(unity_ObjectToWorld,float4(.5,.5,.5,1));
                return o;
            }
            

            fixed4 frag (v2f _input) : SV_Target
            {
                float3 worldMarchDir=-normalize( _input.worldViewDir);
                float worldMarchDst=AABBRayDistance(_input.minBound,_input.maxBound,_input.worldPos,worldMarchDir).y;
                float worldDepthDst=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, _input.screenPos)).r-_input.screenPos.w;
                float marchDistance= min(worldMarchDst, worldDepthDst);
                
                float sumDensity=0;
                if(marchDistance>0)
                {
                    worldMarchDir= normalize(worldMarchDir);
                    float marchOffset=1.0/_RayMarch;
                    float distanceOffset=_Distance/_RayMarch;
                    float dstMarched=0;
                    for(int i=0;i<_RayMarch;i++)
                    {
                        float3 marchPos=_input.worldPos+worldMarchDir*dstMarched;
                        float density=SampleDensity(marchPos)*_Density;
                        sumDensity+=marchOffset*density;
                        dstMarched+=distanceOffset;

                        if(sumDensity>=1||dstMarched>marchDistance)
                            break;
                    }
                }
                sumDensity=saturate(sumDensity);

                return _Color*sumDensity;
            }
            ENDCG
        }
    }
}
