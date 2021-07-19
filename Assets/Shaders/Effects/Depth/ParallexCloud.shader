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
        
        [Header(Misc)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",float)=1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend",float)=1
    }
    SubShader
    {
        Tags{"Queue"="Transparent" }
        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "../../CommonInclude.hlsl"
            #include "../../Library/ParallaxInclude.hlsl"

            struct appdata
            {
                half3 positionOS : POSITION;
            };

            struct v2f
            {
                half4 positionCS : SV_POSITION;
                float3 viewDirWS:TEXCOORD0;
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
            v2f vert(appdata v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                float3 positionWS=TransformObjectToWorld(v.positionOS);
                o.viewDirWS=TransformWorldToViewDir(positionWS,UNITY_MATRIX_V);

                float2 uv=positionWS.xz/_Scale;
                uv+=float2(_FlowX,_FlowY)*_Time.y;
                o.uv=uv;
                return o;
            }
            
            half4 frag(v2f i) : SV_Target
            {
                float3 viewDirWS=normalize(i.viewDirWS);
                viewDirWS.xz/=viewDirWS.y;
                viewDirWS*=_DepthScale;
                float depth= ParallaxMapping(_DepthTex,sampler_DepthTex,i.uv,viewDirWS.xz,_ParallaxCount,_DepthOffset);
                float3 color=lerp(_BeginColor.rgb,_EndColor.rgb,depth);
                return float4(color,1);
            }
            ENDHLSL
        }
    }
}
