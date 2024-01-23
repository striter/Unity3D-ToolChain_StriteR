Shader "Game/Effects/Depth/ParallaxCloud"
{
    Properties
    {
		[Header(Shape)]
		[NoScaleOffset]_DepthTex("Texure",2D)="white"{}
        _Scale("Scale",Range(1,1000))=100
		_DepthScale("Scale",Range(0.001,.5))=1
		_DepthOffset("Offset",Range(-.5,.5))=.42
		[Enum(_16,16,_32,32,_64,64,_128,128)]_ParallaxCount("Parallex Count",int)=16
        
        [Header(Color)]
        _BeginColor("Begin",Color)=(1,1,1,0)
        _EndColor("End",Color)=(0,0,0,0)
        
        [Header(Flow)]
        _FlowX("Flow X",Range(0,5))=1
        _FlowY("Flow Y",Range(0,5))=1
        
        [HideInInspector]_ZWrite("Z Write",int)=1
        [HideInInspector]_ZTest("Z Test",int)=2
        [HideInInspector]_Cull("Cull",int)=2
    }
    SubShader
    {
        Tags{"Queue"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Additional/Local/Parallax.hlsl"

            struct a2v
            {
                half3 positionOS : POSITION;
            };

            struct v2f
            {
                half4 positionCS : SV_POSITION;
                float3 positionWS:TEXCOORD0;
                float2 uv:TEXCOORD1;
            };

            TEXTURE2D(_DepthTex);SAMPLER(sampler_DepthTex);
            CBUFFER_START(UnityPerMaterial)
            half _Scale;
            half _FlowX;
            half _FlowY;
            
            half4 _BeginColor;
            half4 _EndColor;

            half _DepthScale;
            half _DepthOffset;
            uint _ParallaxCount;
            half4 _DepthTex_ST;
            CBUFFER_END
            v2f vert(a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionWS=TransformObjectToWorld(v.positionOS);

                float2 uv=o.positionWS.xz;
                uv+=float2(_FlowX,_FlowY)*_Time.y;
                uv/=_Scale;
                o.uv=uv;
                return o;
            }
            
            half4 frag(v2f i) : SV_Target
            {
                float3 viewDirWS=GetCameraRealDirectionWS(i.positionWS);
                viewDirWS.xz/=viewDirWS.y;
                viewDirWS*=_DepthScale;
                float depth= ParallaxMappingPOM(TEXTURE2D_ARGS(_DepthTex,sampler_DepthTex),_DepthOffset,i.uv,viewDirWS.xz,_ParallaxCount);
                return lerp(_BeginColor,_EndColor,depth);
            }
            ENDHLSL
        }
        USEPASS "Game/Additive/DepthOnly/MAIN"
    }
}
