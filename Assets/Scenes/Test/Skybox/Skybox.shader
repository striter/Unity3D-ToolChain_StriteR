Shader "Unlit/Skybox"
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
        _SunColor("Sun Color",Color)=(1,1,1,1)
        _SunSize("Sun Size",Range(.5,1))=.95
        _MoonColor("Moon Color",Color)=(.8,.8,.8,1)
        _MoonSize("Moon Size",Range(.5,1))=.95
        _MoonCrescent("Moon Crescent",Range(-.2,.2))=.05

        [Header(Star)]
        [NoScaleOffset]_StarTexture("Star Texture",2D)="black"{}
        _StarDensity("Star Density",Range(0.1,2))=1
        _StarColor("Star Color",Color)=(1,1,1,1)
        [NoScaleOffset]_StarMask("Star Mask",2D)="white"{}
        _StarMaskStrength("Star Mask Strength",Range(0,1))=.5
        _StarMaskFlow("Star Mask Flow",Vector)=(.05,0.01,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Geometry-1000" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            float3 _DayTopColor;
            float3 _DayBottomColor;
            float3 _NightTopColor;
            float3 _NightBottomColor;

            float _HorizonClip;
            float _HorizonSize;
            float3 _HorizonColor;

            float3 _SunColor;
            float _SunSize;
            float3 _MoonColor;
            float _MoonSize;
            float _MoonCrescent;

            sampler2D _StarTexture;
            float3 _StarColor;
            float _StarDensity;
            sampler2D _StarMask;
            float2 _StarMaskFlow;
            float _StarMaskStrength;
            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
            };

            struct v2f
            {
                float3 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float invlerp(float a,float b,float value)
            {
                return (value-a)/(b-a);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 sunDir=_WorldSpaceLightPos0;
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
                float3 horizonColor=_HorizonColor*horizonShape*horizonClip;
                finalCol=lerp(finalCol,_HorizonColor,horizonClip*horizonShape);

                float sun=step(_SunSize,1-distance(i.uv,sunDir));
                sun*=sunGradient;
                finalCol+=sun*_SunColor;

                float moon=step(_MoonSize,1-distance(i.uv,-sunDir));
                float moonCrescent=step(_MoonSize,1-distance(i.uv+float3(1,0,0)*_MoonCrescent,-sunDir));
                moon=saturate(moon-moonCrescent);
                moon*=moonGradient;
                finalCol+=moon*_MoonColor;

                float2 skyUV= i.uv.xz/i.uv.y;

                float star= tex2D(_StarTexture,skyUV*_StarDensity).r* skyGradient*moonGradient;
                star*=step(tex2D(_StarMask,skyUV+_Time.y*_StarMaskFlow).r,_StarMaskStrength);
                finalCol+=star*_StarColor;
                
                return float4(finalCol,1);
            }
            ENDCG
        }
    }
}
