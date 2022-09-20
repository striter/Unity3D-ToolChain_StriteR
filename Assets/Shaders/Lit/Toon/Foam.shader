Shader "Game/Lit/Toon/Foam"
{
    Properties
    {
    	[Header(Foam)]
    	_FoamColor("Color",Color)=(1,1,1,1)
		_FoamRange("Foam",Range(0.01,3))=0
    	_FoamRangeWidth("",float)=0
    	_FoamDistortTex("Distort Tex",2D)="black"{}
    	_FoamDistortStrength("Distort Strength",Range(0,2))=1
    }
    SubShader
    {
        Tags {"Queue" = "Transparent"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
        	ZWrite Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
            	float3 normalOS:NORMAL;
                float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
            	float3 normalWS:NORMAL;
                float4 color : COLOR;
            	float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_FoamDistortTex);SAMPLER(sampler_FoamDistortTex);
            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_Color)
                INSTANCING_PROP(float4,_MainTex_ST)

            	INSTANCING_PROP(float,_FoamRange)
				INSTANCING_PROP(float,_FoamRangeEnd)
				INSTANCING_PROP(float4,_FoamColor)
				INSTANCING_PROP(float,_FoamDistortStrength)
				INSTANCING_PROP(float4,_FoamDistortTex_ST)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
            	o.normalWS = TransformObjectToWorldNormal(v.normalOS);
            	float3 positionWS=TransformObjectToWorld(v.positionOS);
            	o.uv = TRANSFORM_TEX_FLOW_INSTANCE(positionWS.xz,_FoamDistortTex);
                o.color = v.color;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float eyeDepthOffset = 1-i.color.r;
            	float foamParameters = saturate(invlerp(INSTANCE(_FoamRange),INSTANCE(_FoamRange)+INSTANCE(_FoamRangeEnd),eyeDepthOffset-((SAMPLE_TEXTURE2D(_FoamDistortTex,sampler_FoamDistortTex,i.uv).r*2-1))*_FoamDistortStrength));
				foamParameters = (1-foamParameters)*step(0,eyeDepthOffset);
            	float3 indirectDiffuse = IndirectDiffuse_SH(normalize(i.normalWS));
                return float4(_FoamColor.rgb*indirectDiffuse,foamParameters*_FoamColor.a);
            }
            ENDHLSL
        }
        
        USEPASS "Game/Additive/DepthOnly/MAIN"
        USEPASS "Game/Additive/ShadowCaster/MAIN"
    }
}
