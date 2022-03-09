Shader "Geme/Lit/Vegetation"
{
    Properties
    {
        [Header(Albedo)]
        _MainTex ("Main", 2D) = "white" {}
        [Toggle(_COLORMASK)] _ColorMask("Masked Vegetation",int)=0
        [Foldout(_COLORMASK)]_MaskR1("Root Color",color)=(1,1,1,1)
        [Foldout(_COLORMASK)]_MaskR2("Edge Color",color)=(0.5,1,0.5,1)
        [Foldout(_COLORMASK)]_MaskRGradientBegin("Root Begin",Range(0,1))=0
        [Foldout(_COLORMASK)]_MaskRGradientWidth("Root Width",Range(0,1))=0
        [Foldout(_COLORMASK)]_MaskF1("Flower 1 Edge",color)=(1,1,1,1)
        [Foldout(_COLORMASK)]_MaskF1C("Flower 1 Core",color)=(1,0,0,1)
        [Foldout(_COLORMASK)]_MaskF2("Flower 2 Edge",color)=(0,1,0,1)
        [Foldout(_COLORMASK)]_MaskF2C("Flower 2 Core",color)=(0,0,1,1)
          
        [Header(Flow)]
        _WindFlowTex("Flow Texture",2D)="white"{}
        _BendStrength("Strength (Angle)",float)=1
    	[Header(_Wiggle)]
    	[Toggle(_WIGGLE)]_Wiggle("Wiggle",int)=0
    	_WiggleDensity("Wiggle Density",float)=1
    	_WiggleStrength("Wiggle Strength",float)=.1
         
    	[Header(Render Options)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",int)=1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend",int)=0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull",int)=2
        [Toggle(_ALPHACLIP)]_AlphaClip("Alpha Clip",float)=0
        [Foldout(_ALPHACLIP)]_AlphaClipRange("Range",Range(0.01,1))=0.01
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
		Blend [_SrcBlend] [_DstBlend]
		Cull [_Cull]
		ZWrite [_ZWrite]
		ZTest [_ZTest]
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"

            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_MainTex_ST)
			    INSTANCING_PROP(float,_AlphaClipRange)
                INSTANCING_PROP(float3,_MaskR1)
                INSTANCING_PROP(float3,_MaskR2)
                INSTANCING_PROP(float,_MaskRGradientBegin)
                INSTANCING_PROP(float,_MaskRGradientWidth)
                INSTANCING_PROP(float3,_MaskF1)
                INSTANCING_PROP(float3,_MaskF1C)
                INSTANCING_PROP(float3,_MaskF2)
                INSTANCING_PROP(float3,_MaskF2C)

                INSTANCING_PROP(float4,_WindFlowTex_ST)
			    INSTANCING_PROP(float,_WiggleStrength)
			    INSTANCING_PROP(float,_WiggleDensity)
			    INSTANCING_PROP(float,_BendStrength)
            
            INSTANCING_BUFFER_END
            
			#include "Assets/Shaders/Library/Additional/Local/AlphaClip.hlsl"
			#pragma shader_feature_local _ALPHACLIP
            #pragma shader_feature_local _COLORMASK
            #pragma shader_feature_local _WIGGLE

            TEXTURE2D(_WindFlowTex);SAMPLER(sampler_WindFlowTex);

            float3 _BendDirection;
            float3 Wind(float3 positionWS,float windEffect,float _bendStrength)
            {
                float4 windParameters=INSTANCE(_WindFlowTex_ST);
                float2 bendUV= positionWS.xz+_Time.y*windParameters.zw;
                bendUV*=windParameters.xy;
                float2 flowSample=SAMPLE_TEXTURE2D_LOD(_WindFlowTex,sampler_WindFlowTex,bendUV,0).rg;
                float windInput= flowSample.x-flowSample.y;
            	
                float3x3 bendRotation=Rotate3x3(_bendStrength*windInput*Deg2Rad,_BendDirection);
                float3 offset=float3(0,windEffect,0);

                float3 bendOffset=mul(bendRotation,offset);
                bendOffset-=offset;

                #if _WIGGLE
				    float wiggleDensity=INSTANCE(_WiggleDensity);
				    float2 wiggleClip=abs((positionWS.xz+_Time.y*windParameters.zw)%wiggleDensity-wiggleDensity*.5f)/wiggleDensity;
				    float wiggle=wiggleClip.x+wiggleClip.y;
				    bendOffset.y +=  wiggle*INSTANCE(_WiggleStrength)*windEffect;
				#endif
            	
                return bendOffset;
            }

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            
            struct a2v
            {
                float3 positionOS : POSITION;
                float3 normalOS:NORMAL;
                float2 uv : TEXCOORD0;
                float4 color:COLOR;
                A2V_LIGHTMAP
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS:NORMAL;
                float2 uv : TEXCOORD0;
                float4 color:COLOR;
                V2F_FOG(1)
				V2F_LIGHTMAP(2)
            };

            v2f vert (a2v v)
            {
                v2f o;
                float3 positionWS=TransformObjectToWorld(v.positionOS);
				positionWS+=Wind(positionWS,v.color.b,INSTANCE(_BendStrength));
                o.positionCS = TransformWorldToHClip(positionWS);
                o.uv = TRANSFORM_TEX_INSTANCE(v.uv, _MainTex);
                o.color=v.color;
                o.normalWS=TransformObjectToWorldNormal(v.normalOS);
                FOG_TRANSFER(o)
                LIGHTMAP_TRANSFER(v,o)
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 sample=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
                half alpha=sample.a;
                AlphaClip(alpha);
                float3 albedo = 0;
                #if _COLORMASK
                    float root=step(0.05,sample.r);
                    float rootGradient=saturate(invlerp(INSTANCE(_MaskRGradientBegin),INSTANCE(_MaskRGradientBegin)+INSTANCE(_MaskRGradientWidth),i.color.r));
                    albedo += lerp(INSTANCE(_MaskR1),INSTANCE(_MaskR2),rootGradient)*sample.r * root;
                    
                    float flower=1-root;
                    float flower1= step(i.color.g,0.05);
                    float3 flowerEdgeCol=lerp(_MaskF1,_MaskF2,flower1);
                    float3 flowerCoreColor=lerp(_MaskF1C,_MaskF2C,flower1);
                    albedo+= flowerCoreColor*flower*sample.g+flowerEdgeCol*flower*sample.b;
                #else
                    albedo = sample.rgb;
                #endif

                Light mainLight=GetMainLight();
				half3 indirectDiffuse= IndirectDiffuse(mainLight,i,normalize(i.normalWS));
                float3 finalCol=albedo*indirectDiffuse;
                FOG_MIX(i,finalCol);
                return float4(finalCol,alpha);
            }
            ENDHLSL
        }
    }
}
