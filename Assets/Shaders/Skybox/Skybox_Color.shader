Shader "Game/Skybox/Color"
{
    Properties
    {
        [Header(Sky Color)]
        _DayTopColor("_DayTopColor",Color)=(1,1,0.85,1)
        _DayBottomColor("_DayBottomColor",Color)=(.8,.8,.8,1)
        _NightTopColor("_NightTopColor",Color)=(.25,.3,.4,1)
        _NightBottomColor("_NightBottomColor",Color)=(.3,.2,.3,1)

        [Header(Horizon)]
        _HorizonClip("Horizon Clip",Range(0,1))=.15
        _HorizonSize("Horizon Size",Range(0,1))=.3
        _HorizonColor("Horizon Color",Color)=(.15,.15,.15,1)
        
        [Header(Sun Moon)]
        [HDR]_SunColor("Sun Color",Color)=(1,1,1,1)
        _SunSize("Sun Size",Range(.5,1))=.95
        [HDR]_MoonColor("Moon Color",Color)=(.8,.8,.8,1)
        _MoonSize("Moon Size",Range(.5,1))=.95
        _MoonCrescent("Moon Crescent",Range(-.2,.2))=.05

        
        [Header(Cloud)]
        _A("Short Width",Range(0,1))=1
        _B("Long Width",Range(0,1)) = 1
        _SkyHeight("Height",float) = 20
        _CloudTex("Main Cloud",2D)="white"{}
        _CloudTex2("Main Cloud",2D)="white"{}
        [MinMaxRange]_CloudRange("Cloud Range",Range(0,1))=0.5
        [HideInInspector]_CloudRangeEnd("",float)=0.6
        _CloudColor("Cloud Color",Color)=(1,1,1,1)
        _CloudColorEnd("Cloud Color End",Color)=(1,1,1,1)
        
        [Header(Star)]
        [NoScaleOffset]_StarTexture("Star Texture",2D)="black"{}
        _StarDensity("Star Density",Range(0.1,2))=1
        [HDR]_StarColor("Star Color",Color)=(1,1,1,1)
        [NoScaleOffset]_StarMask("Star Mask",2D)="white"{}
        _StarMaskStrength("Star Mask Strength",Range(0,1))=.5
        _StarMaskFlow("Star Mask Flow",Vector)=(.05,0.01,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Geometry-1000" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"
            TEXTURE2D_X(_StarTexture);SAMPLER(sampler_StarTexture);
            TEXTURE2D_X(_StarMask);SAMPLER(sampler_StarMask);
            TEXTURE2D(_CloudTex);SAMPLER(sampler_CloudTex);
            TEXTURE2D(_CloudTex2);SAMPLER(sampler_CloudTex2);
            CBUFFER_START(UnityPerMaterial)
                INSTANCING_PROP(float3,_DayTopColor)
                INSTANCING_PROP(float3,_DayBottomColor)
                INSTANCING_PROP(float3,_NightTopColor)
                INSTANCING_PROP(float3,_NightBottomColor)

                INSTANCING_PROP(float,_HorizonClip)
                INSTANCING_PROP(float,_HorizonSize)
                INSTANCING_PROP(float3,_HorizonColor)

                INSTANCING_PROP(float3,_SunColor)
                INSTANCING_PROP(float,_SunSize)
                INSTANCING_PROP(float3,_MoonColor)
                INSTANCING_PROP(float,_MoonSize)
                INSTANCING_PROP(float,_MoonCrescent)

                INSTANCING_PROP(float3,_StarColor)
                INSTANCING_PROP(float,_StarDensity)
                INSTANCING_PROP(float2,_StarMaskFlow)
                INSTANCING_PROP(float,_StarMaskStrength)
            
                float _A;
                float _B;
                float _SkyHeight;
                float _CloudRange;
                float _CloudRangeEnd;
                float4 _CloudTex_ST;
                float4 _CloudTex2_ST;
                float4 _CloudColor;
                float4 _CloudColorEnd;
            CBUFFER_END

            struct a2v
            {
                float3 positionOS : POSITION;
                float3 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 uv : TEXCOORD0;
            };

            v2f vert (a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float3 sunDir=normalize(_MainLightPosition.xyz);
                float sunHeight=sunDir.y;
                float skyGradient=saturate(i.uv.y);
                float daynightGradient= invlerp(-1,1,sunDir.y);
                float sunGradient=saturate(sunDir.y);
                float moonGradient=saturate(-sunDir.y);

                float3 finalCol=0;

                float3 gradientDay=lerp(_DayBottomColor,_DayTopColor,skyGradient);
                float3 gradientNight=lerp(_NightBottomColor,_NightTopColor,skyGradient);
                finalCol=lerp(gradientNight,gradientDay,daynightGradient);

                float horizonShape=smoothstep(_HorizonSize,1, (1-abs(i.uv.y)));
                float horizonClip=smoothstep(_HorizonClip,1 ,1-abs(sunHeight));
                finalCol=lerp(finalCol,_HorizonColor,horizonClip*horizonShape);

                float sun=step(_SunSize,1-distance(i.uv,sunDir));
                sun*=sunGradient;
                finalCol+=sun*_SunColor;

                float moon=step(_MoonSize,1-distance(i.uv,-sunDir));
                float moonCrescent=step(_MoonSize,1-distance(i.uv+float3(1,0,0)*_MoonCrescent,-sunDir));
                moon=saturate(moon-moonCrescent);
                moon*=moonGradient;
                finalCol+=moon*_MoonColor;
                
                float fade = saturate(invlerp(0.05,0.2,i.uv.y));
                float2 baseUV = i.uv.xz / (sqrt(_A + (_B - _A) * i.uv.y * i.uv.y * _SkyHeight * _SkyHeight));//i.uv.xz * rcp(i.uv.y);

                float cloudSample = SAMPLE_TEXTURE2D(_CloudTex,sampler_CloudTex,TransformTex_Flow(baseUV,_CloudTex_ST));
                float cloudSample2 = SAMPLE_TEXTURE2D(_CloudTex2,sampler_CloudTex2,TransformTex_Flow(baseUV,_CloudTex2_ST));

                float cloudParam = saturate(invlerp(_CloudRange,_CloudRangeEnd,cloudSample*cloudSample2));
                finalCol = lerp(finalCol,lerp(_CloudColor,_CloudColorEnd,cloudParam-max(sun,moon)),step(0.01,cloudParam)*fade);

                float star= SAMPLE_TEXTURE2D(_StarTexture,sampler_StarTexture,baseUV*_StarDensity).r* skyGradient*moonGradient;
                star*=step(SAMPLE_TEXTURE2D(_StarMask,sampler_StarMask,baseUV+_Time.y*_StarMaskFlow).r,_StarMaskStrength);
                finalCol+=star*_StarColor*(1-cloudParam)*fade;
                return float4(finalCol,1);
            }
            ENDHLSL
        }
    }
}
