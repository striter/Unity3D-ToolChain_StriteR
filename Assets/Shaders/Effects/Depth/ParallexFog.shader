Shader "Game/Effects/Depth/ParallexFog"
{
    Properties
    {
        _Color("Color",Color)=(1,1,1,.5)
		[Header(Depth)]
		[ToggleTex(_DEPTHMAP)][NoScaleOffset]_DepthTex("Texure",2D)="white"{}
		[Foldout(_DEPTHMAP)]_DepthScale("Scale",Range(0.001,.5))=1
		[Foldout(_DEPTHMAP)]_DepthOffset("Offset",Range(0,1))=.42
		[Toggle(_DEPTHBUFFER)]_DepthBuffer("Affect Buffer",float)=1
    	[Foldout(_DEPTHBUFFER)]_DepthBufferScale("Affect Scale",float)=1
		[Toggle(_PARALLEX)]_Parallex("Parallex",float)=0
		[Enum(_16,16,_32,32,_64,64,_128,128)]_ParallexCount("Parallex Count",int)=16
        
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
            #pragma shader_feature_local _PARALLEX
            #pragma shader_feature_local _DEPTHBUFFER
            #pragma shader_feature_local _DEPTHMAP

            #include "../../CommonInclude.hlsl"
            #include "../../GeometryInclude.hlsl"
            #include "../../Library/ParallaxInclude.hlsl"

            struct appdata
            {
                half3 positionOS : POSITION;
            };

            struct v2f
            {
                half4 positionCS : SV_POSITION;
                half3 positionOS:TEXCOORD0;
                half4 screenPos : TEXCOORD2;
            };
            
            TEXTURE2D(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            CBUFFER_END
            v2f vert(appdata v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionOS = v.positionOS;
                o.screenPos = ComputeScreenPos(o.positionCS);
                return o;
            }
            
            half4 frag(v2f i) : SV_Target
            {
                return half4( _Color);
            }
            ENDHLSL
        }
    }
}
