Shader "Game/Effects/Depth/Raymarch_VolumetricCloud_Box"
{
    Properties
    {
        _Strength("Strength",Range(0,5))=1
        _DensityClip("Cloud Clip",Range(0,1))=.1
        _Density("Cloud Density",Range(0,10))=1
        _Distance("March Distance",float)=50
        [Enum(_16,16,_32,32,_64,64,_128,128)]_RayMarchTimes("Ray March Times",int)=32
        [NoScaleOffset]_ColorRamp("Cloud Color Ramp",2D)="white"
        
        [Header(Noise Settings)]
        _Noise("Noise 3D",3D)="white"{}
        _NoiseScale("Noise Scale",Vector)=(10,10,1,1)
        _NoiseFlow("Noise Flow",Vector)=(0,0,0,1)

        [Header(Light March Settings)]
        _LightAbsorption("Light Absorption",Range(0,10))=1
        [Toggle(_LIGHTMARCH)]_EnableLightMarch("Enable Light March",int)=1
        [Enum(_4,4,_8,8,_16,16)]_LightMarchTimes("Light March Times",int)=8
        
        [Header(Scatter Settings)]
        _ScatterRange("Scatter Range",Range(0.5,1))=0.8
        _ScatterStrength("Scatter Strength",Range(0,1))=0.8
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
            #include "../../BoundingCollision.cginc"
            #pragma shader_feature _LIGHTMARCH

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

            int _RayMarchTimes;
            float _Distance;
            float _Strength;
            float _Density;
            float _DensityClip;
            
            float _ScatterRange;
            float _ScatterStrength;

            int _LightMarchTimes;
            float _LightAbsorption;
            sampler2D _ColorRamp;

            sampler2D _CameraDepthTexture;

            sampler3D _Noise;
            float4 _NoiseScale;
            float4 _NoiseFlow;
            float SampleDensity(float3 worldPos)  {
                return saturate(smoothstep(_DensityClip,1 , tex3Dlod(_Noise,float4( worldPos/_NoiseScale+_NoiseFlow*_Time.y,0)).r)*_Density);
            }

            #if _LIGHTMARCH
            float3 lightMarch(float minBound,float3 maxBound, float3 position,float3 marchDir,float marchDst)
            {
                float dstInsideBox=AABBRayDistance(minBound,maxBound,position,marchDir).y;
                float cloudDensity=0;
                float totalDst=0;
                for(int i=0;i<_LightMarchTimes;i++)
                {
                    float3 marchPos=position+marchDir*totalDst;
                    cloudDensity+=SampleDensity(marchPos);
                    totalDst+=marchDst;
                    if(totalDst>dstInsideBox)
                        break;
                }
                return  cloudDensity/_LightMarchTimes;
            }
            #endif
            
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

                float cloudDensity=1;
                float lightIntensity=1;
                if(marchDistance>0)
                {
                    float3 worldLightDir=normalize(_input.worldLightDir);
                    float scatter=(1-smoothstep(_ScatterRange,1,dot(worldMarchDir,worldLightDir))*_ScatterStrength);
                    float cloudMarchDst= _Distance/_RayMarchTimes;
                    float lightMarchDst=_Distance/_LightMarchTimes;
                    worldMarchDir= normalize(worldMarchDir);
                    float dstMarched=0;
                    float marchParam=1.0/_RayMarchTimes;
                    float totalDensity=0;
                    for(int i=0;i<_RayMarchTimes;i++)
                    {
                        float3 marchPos=_input.worldPos+worldMarchDir*dstMarched;
                        float density=SampleDensity(marchPos);
                        density*=marchParam;
                        if(density>0)
                        {
                            cloudDensity*= exp(-density*_Strength);
                            #if _LIGHTMARCH
                            lightIntensity *= exp(-density*scatter*cloudDensity*lerp(0,_LightAbsorption, lightMarch(_input.minBound,_input.maxBound,marchPos,worldLightDir,lightMarchDst)));
                            #else
                            lightIntensity -= density*scatter*cloudDensity*_LightAbsorption;
                            #endif
                        }

                        dstMarched+=cloudMarchDst;
                        if(cloudDensity<0.01||dstMarched>marchDistance)
                            break;
                    }
                }
                float3 rampCol=tex2D(_ColorRamp, lightIntensity).rgb;
                float3 lightCol= lerp(rampCol,_LightColor0.rgb, lightIntensity);
                return float4(lightCol,1-cloudDensity);
            }
            ENDCG
        }
    }
}
