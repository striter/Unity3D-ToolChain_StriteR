// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Raymarch/BoxVolumetricFog"
{
    Properties
    {
        _Color("Color",Color)=(1,1,1,1)
        _Distance("March Distance",Range(0,50))=5
        _Density("Density",Range(0,5))=1
        [Enum(_16,16,_32,32,_64,64,_128,128,_256,256,_512,512)]_March("March Times",int)=128
        _Noise("Noise 3D",3D)="white"{}
        _NoiseScale("Noise Scale",Range(0.1,100))=10
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

            #include "UnityCG.cginc"
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
                float4 vertex : SV_POSITION;
            };

            int _March;
            float4 _Color;
            float _Distance;
            sampler3D _Noise;
            float _Density;
            float _NoiseScale;
            float4 _NoiseFlow;
            sampler2D _CameraDepthTexture;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.objPos=v.vertex;
                o.worldPos=mul(unity_ObjectToWorld,v.vertex);
                o.viewDir=ObjSpaceViewDir(v.vertex);
                o.screenPos=ComputeScreenPos(o.vertex);
                return o;
            }
            

            fixed4 frag (v2f i) : SV_Target
            {
                half3 objViewDir=-normalize(i.viewDir);
                half objMarchDst=RayBoxDistance(-.5,.5,i.objPos,objViewDir).y;

                half worldMarchDst=length(mul(unity_ObjectToWorld,objViewDir*objMarchDst));
                half3 worldMarchDir=mul(unity_ObjectToWorld,objViewDir*objMarchDst);
                half depthDst=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screenPos)).r-i.screenPos.w;
                half marchDistance= min(length( worldMarchDir), depthDst);
                
                float march=0;
                if(marchDistance>0)
                {
                    worldMarchDir= (normalize(worldMarchDir)*_Distance)/_March;
                    float marchOffset=1.0/_March;
                    float distanceOffset=length(worldMarchDir);

                    for(int index=0;index<_March;index++)
                    {
                        if(marchDistance<0)
                            break;
                        float3 marchPos=i.worldPos+worldMarchDir*index;
                        float density=saturate( tex3Dlod(_Noise,float4( marchPos/_NoiseScale+_NoiseFlow*_Time.y,0)).r*_Density);
                        march+=density*marchOffset;
                        marchDistance-=distanceOffset;

                        if(march>=1)
                            break;
                    }
                }
                march=saturate(march);

                return _Color*march;
            }
            ENDCG
        }
    }
}
