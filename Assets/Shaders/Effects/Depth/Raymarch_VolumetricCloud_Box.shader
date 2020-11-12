Shader "Game/Effects/Depth/Raymarch_VolumetricCloud_Box"
{
    Properties
    {
        _Density("Density",Range(0,1))=1
        _Distance("March Distance",float)=5
        [Enum(_16,16,_32,32,_64,64,_128,128)]_RayMarch("Ray March Times",int)=128
        [Enum(_8,8,_16,16)]_LightMarch("Light March Times",int)=8
        _LightShadowColor("Light Shadow Color",Color)=(0.7,0.7,0.7,0)
        _LightAbsorption("Light Absorption",float)=1
        _Noise("Noise 3D",3D)="white"{}
        _NoiseScale("Noise Scale",Vector)=(10,10,1,1)
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
            #include "Lighting.cginc"
            #include "../../CommonInclude.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float3 objPos:TEXCOORD0;
                float3 worldPos:TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                float3 viewDir:TEXCOORD3;
                float3 lightDir:TEXCOORD4;
                float4 vertex : SV_POSITION;
            };

            int _RayMarch;
            float _Distance;
            float _Density;

            sampler2D _CameraDepthTexture;

            sampler3D _Noise;
            float4 _NoiseScale;
            float4 _NoiseFlow;
            float SampleDensity(float3 worldPos)  {
                return  saturate( tex3Dlod(_Noise,float4( worldPos/_NoiseScale+_NoiseFlow*_Time.y,0)).r);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.objPos=v.vertex;
                o.worldPos=mul(unity_ObjectToWorld,v.vertex);
                o.viewDir=ObjSpaceViewDir(v.vertex);
                o.lightDir=WorldSpaceLightDir(v.vertex);
                o.screenPos=ComputeScreenPos(o.vertex);
                return o;
            }
            
            int _LightMarch;
            float _LightAbsorption;
            float4 _LightShadowColor;
            float3 lightMarch(float3 position,float dstTravelled,float3 lightDir)
            {
                float3 dirToLight=-lightDir;
                float dstInsideBox=AABBRayDistance(-.5,.5,position,dirToLight).y;
                float marchDst=dstInsideBox/_LightMarch;
                float lightDensity=0;
                float densityParam=1.0/_LightMarch;
                for(int i=0;i<_LightMarch;i++)
                {
                    float3 marchPos=position+marchDst*i;
                    lightDensity+=SampleDensity(marchPos)*densityParam;
                }
                lightDensity=exp(-lightDensity*_LightAbsorption);
                return   _LightColor0.rgb* lerp(_LightShadowColor.rgb,1, lightDensity);
            }

            fixed4 frag (v2f _input) : SV_Target
            {
                float3 objViewDir=-normalize(_input.viewDir);
                float objMarchDst=AABBRayDistance(-.5,.5,_input.objPos,objViewDir).y;
                float worldMarchDst=length(mul(unity_ObjectToWorld,float3(0, objMarchDst,0)));
                float3 worldMarchDir=mul(unity_ObjectToWorld,objViewDir*objMarchDst);
                float worldDepthDst=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, _input.screenPos)).r-_input.screenPos.w;
                float marchDistance= min(length( worldMarchDir), worldDepthDst);

                float sumDensity=1;
                float3 lightCol=0;
                if(marchDistance>0)
                {
                    float marchDst= _Distance/_RayMarch;
                    float marchParam=1.0/_RayMarch;
                    worldMarchDir= normalize(worldMarchDir);
                    float3 worldLightDir=normalize(_input.lightDir);
                    float dstMarched=0;
                    for(int i=0;i<_RayMarch;i++)
                    {
                        float3 marchPos=_input.worldPos+worldMarchDir*dstMarched;
                        float density=SampleDensity(marchPos);
                        if(density>0)
                        {
                            sumDensity*= exp(-density*marchDst*_Density);
                            lightCol+= density*lightMarch(marchPos,dstMarched,worldLightDir)*marchParam;
                        }

                        dstMarched+=marchDst;
                        if(sumDensity<0.001||dstMarched>marchDistance)
                            break;
                    }
                }
                sumDensity=1-sumDensity;
                return float4(lightCol,sumDensity);
            }
            ENDCG
        }
    }
}
