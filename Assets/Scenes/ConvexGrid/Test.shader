Shader "Unlit/Test"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		[Fold(_PBRMAP)]_Glossiness("Glossiness",Range(0,1))=1
    }
    SubShader
    {
        Tags { "LightMode"="UniversalForward" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#include "Assets/Shaders/Library/Common.hlsl"

            struct appdata
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS:NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 positionWS:TEXCOORD1;
                float3 normalWS:TEXCOORD2;
            };

            float _Glossiness;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionWS=TransformObjectToWorld(v.positionOS);
                o.normalWS=TransformObjectToWorldNormal(v.normalOS);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float3 normalWS=normalize(i.normalWS);
                float3 viewDirWS=GetWorldSpaceNormalizeViewDir(i.positionWS);
                half3 lightDir=normalize(_MainLightPosition.xyz);
                float3 halfDir = SafeNormalize(float3(lightDir) + float3(viewDirWS));

                half perceptualRoughness = 1.0h - _Glossiness;
                half roughness = max(HALF_MIN_SQRT, perceptualRoughness * perceptualRoughness);
                half roughness2 = max(HALF_MIN, roughness * roughness);
                
                float NoH = saturate(dot(normalWS, halfDir));
                half LoH = saturate(dot(lightDir, halfDir));

                // GGX Distribution multiplied by combined approximation of Visibility and Fresnel
                // BRDFspec = (D * V * F) / 4.0
                // D = roughness^2 / ( NoH^2 * (roughness^2 - 1) + 1 )^2
                // V * F = 1.0 / ( LoH^2 * (roughness + 0.5) )
                // See "Optimizing PBR for Mobile" from Siggraph 2015 moving mobile graphics course
                // https://community.arm.com/events/1155

                // Final BRDFspec = roughness^2 / ( NoH^2 * (roughness^2 - 1) + 1 )^2 * (LoH^2 * (roughness + 0.5) * 4.0)
                // We further optimize a few light invariant terms
                // brdfData.normalizationTerm = (roughness + 0.5) * 4.0 rewritten as roughness * 4.0 + 2.0 to a fit a MAD.
                float d = NoH * NoH * (roughness2-1.) + 1.00001f;

                half LoH2 = LoH * LoH;
                half specularTerm = roughness2 / ((d * d) * max(0.1h, LoH2) * (roughness*4.+2.));

                // On platforms where half actually means something, the denominator has a risk of overflow
                // clamp below was added specifically to "fix" that, but dx compiler (we convert bytecode to metal/gles)
                // sees that specularTerm have only non-negative terms, so it skips max(0,..) in clamp (leaving only min(100,...))
            #if defined (SHADER_API_MOBILE) || defined (SHADER_API_SWITCH)
                specularTerm = specularTerm - HALF_MIN;
                specularTerm = clamp(specularTerm, 0.0, 100.0); // Prevent FP16 overflow on mobiles
            #endif

				float3 finalCol=specularTerm;//NDF_CookTorrance(dot(normalWS,normalize(viewDirWS+normalize(_MainLightPosition.xyz))),surface.roughness2);//BRDFLighting(surface,brdfMainLight);
				return float4(finalCol,1);
            }
            ENDHLSL
        }
    }
}
