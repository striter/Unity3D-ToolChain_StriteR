Shader "Game/Lit/Transparency/Liquid"
{
    Properties
    {
        _NormalOffset("Normal Offset",Range(-1,1))=-.1
        _SurfaceColor("Color Surface",Color)=(1,1,0,1)
        _FoamColor("Color Foam",Color)=(1,1,1,1)
        _LiquidColor("Color Liquid",Color)=(0,1,1,1)
        
        _FoamWidth("Foam Width",Range(0,1))=.1
        
		[Header(Render Options)]
        [HideInInspector]_ZWrite("Z Write",int)=1
        [HideInInspector]_ZTest("Z Test",int)=2
        [HideInInspector]_Cull("Cull",int)=2
    }
    SubShader
    {
        Cull Off
        ZWrite On
        Blend Off
		Tags{"LightMode" = "UniversalForward"}
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"

            struct appdata
            {
                float3 positionOS : POSITION;
                float3 normalOS:NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS:TEXCOORD0;
                float3 normalWS:TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            INSTANCING_BUFFER_START
                INSTANCING_PROP(float,_NormalOffset)
                INSTANCING_PROP(float4,_LiquidColor)
                INSTANCING_PROP(float4,_FoamColor)
                INSTANCING_PROP(float4,_SurfaceColor)
                INSTANCING_PROP(float,_FoamWidth)
            INSTANCING_BUFFER_END
            
            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                float3 positionWS=TransformObjectToWorld(v.positionOS);
                float3 normalWS=TransformObjectToWorldNormal(v.normalOS);
                positionWS+=INSTANCE(_NormalOffset)*normalWS;
                o.positionCS = TransformWorldToHClip(positionWS);
                o.positionWS=positionWS;
                o.normalWS=normalWS;
                return o;
            }

            float4 frag (v2f i,float facing:VFACE) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float clipping=-i.positionWS.y;
                float3 liquidCol= INSTANCE(_LiquidColor).rgb;
                clip(clipping);

                float3 viewDirWS= normalize(GetCameraPositionWS()-i.positionWS);
                
                float3 normalWS=normalize(i.normalWS);
                float ndv=pow5(1-dot(viewDirWS,normalWS));
                liquidCol+=ndv;
                float3 topCol= lerp(INSTANCE(_FoamColor).rgb,liquidCol,step(INSTANCE(_FoamWidth),clipping));
                float3 finalCol=lerp(INSTANCE(_SurfaceColor).rgb,topCol,step(0,facing));
                return float4(finalCol,1);
            }
            ENDHLSL
        }
        
        USEPASS "Game/Additive/DepthOnly/MAIN"
        USEPASS "Game/Additive/ShadowCaster/MAIN"
    }
}
