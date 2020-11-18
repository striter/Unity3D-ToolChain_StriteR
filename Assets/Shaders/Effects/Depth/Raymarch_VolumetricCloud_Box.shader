Shader "Game/Effects/Depth/Raymarch_VolumetricCloud_Box"
{
    Properties
    {
        _Strength("Strength",Range(0,5))=1
        _Density("Density",Range(0,5))=1
        _DensityClip("Density Clip",Range(0,1))=.1
        _Distance("March Distance",float)=5
        [Enum(_16,16,_32,32,_64,64,_128,128)]_RayMarch("Ray March Times",int)=128
        [Enum(_8,8,_16,16)]_LightMarch("Light March Times",int)=8
        _LightShadowColor("Light Shadow Color",Color)=(0.7,0.7,0.7,0)
        _LightAbsorption("Light Absorption",Range(0,1))=1
        _Noise("Noise 3D",3D)="white"{}
        _NoiseScale("Noise Scale",Vector)=(10,10,1,1)
        _NoiseFlow("Noise Flow",Vector)=(0,0,0,1)
    }
    SubShader
    {
        Tags{"Queue"="Transparent" "LightMode"="ForwardBase"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
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
            float _Distance;
            float _Strength;
            float _Density;
            float _DensityClip;

            int _LightMarch;
            float _LightAbsorption;
            float4 _LightShadowColor;

            sampler2D _CameraDepthTexture;

            sampler3D _Noise;
            float4 _NoiseScale;
            float4 _NoiseFlow;
            float SampleDensity(float3 worldPos)  {
                return smoothstep(_DensityClip,1 ,saturate( tex3Dlod(_Noise,float4( worldPos/_NoiseScale+_NoiseFlow*_Time.y,0)).r*_Density));
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
            

            float3 lightMarch(float minBound,float3 maxBound, float3 position,float3 lightDir,float marchDst)
            {
                float3 marchDir=-lightDir;
                float dstInsideBox=AABBRayDistance(minBound,maxBound,position,-lightDir).y;
                float cloudDensity=0;
                float totalDst=0;
                for(int i=0;i<_LightMarch;i++)
                {
                    float3 marchPos=position+marchDir*totalDst;
                    cloudDensity+=SampleDensity(marchPos);
                    totalDst+=marchDst;
                    if(totalDst>dstInsideBox)
                        break;
                }
                return cloudDensity/_LightMarch*_LightAbsorption;
            }

            fixed4 frag (v2f _input) : SV_Target
            {
                float3 worldMarchDir=-normalize( _input.worldViewDir);
                float worldMarchDst=AABBRayDistance(_input.minBound,_input.maxBound,_input.worldPos,worldMarchDir).y;
                float worldDepthDst=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, _input.screenPos)).r-_input.screenPos.w;
                float marchDistance= min(worldMarchDst, worldDepthDst);

                float sumDensity=1;
                float lightDensity=1;
                if(marchDistance>0)
                {
                    float marchDst= _Distance/_RayMarch;
                    worldMarchDir= normalize(worldMarchDir);
                    float3 worldLightDir=normalize(_input.worldLightDir);
                    float dstMarched=0;
                    float marchParam=1.0/_RayMarch;
                    for(int i=0;i<_RayMarch;i++)
                    {
                        float3 marchPos=_input.worldPos+worldMarchDir*dstMarched;
                        float density=smoothstep(0,1,SampleDensity(marchPos));
                        if(density>0)
                        {
                            float cloudDensity=exp(-density*marchParam*_Strength);
                            sumDensity*= cloudDensity;
                            lightDensity -=density*marchParam*lightMarch(_input.minBound,_input.maxBound,marchPos,worldLightDir,marchDst);
                        }

                        dstMarched+=marchDst;
                        if(sumDensity<0.01||dstMarched>marchDistance)
                            break;
                    }
                }

                sumDensity=1-sumDensity;
                return float4(lerp(_LightShadowColor,_LightColor0.rgb, lightDensity),sumDensity);
            }
            ENDCG
        }
    }
}
