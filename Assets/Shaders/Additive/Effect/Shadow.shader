Shader "Game/Additive/Shadow"
{
    Properties
    {
    	[Header(Plane)]
    	[Vector3]_PlaneNormal("Plane Normal",Vector)=(0,1,0,0)
    	_PlaneDistance("Plane Distance",float)=-.5
    	
    	[Header(Color)]
        _Color("Color",Color)=(0,0,0,1)
    	[Toggle(_FALLOFF)]_("Enable",int)=0
    	[MinMaxRange]_FallOff("FallOff",Range(0,2))=0
    	[HideInInspector]_FallOffEnd("",float)=0.1
    	
    	[Header(Mirror)]
    	[Toggle(_MIRROR)]_Mirror("Enable",int)=0
    	
		[Header(Misc)]
		_Stencil("Stencil ID", int) = 0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Enum(Off,0,Front,1,Back,2)]_Cull("Cull",int)=2
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Geometry+1" }
        Pass 
        {	
		    Name "Per Entity Shadow"
        	
			Stencil
			{
				Ref[_Stencil]
				Comp NotEqual
				Pass Replace
			}
        	
    		Blend SrcAlpha OneMinusSrcAlpha
			Cull [_Cull]
			ZWrite [_ZWrite]
			ZTest [_ZTest]
        	
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma shader_feature_local_vertex _MIRROR
			#pragma shader_feature_local_vertex _FALLOFF
			#define IGeometryDetection
			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Geometry.hlsl"

			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_Color)
				INSTANCING_PROP(float,_PlaneDistance)
				INSTANCING_PROP(float3,_PlaneNormal)
				INSTANCING_PROP(float,_FallOff)
				INSTANCING_PROP(float,_FallOffEnd)
			INSTANCING_BUFFER_END

			struct a2v
			{
				float3 positionOS : POSITION;
				float3 normalOS:NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS:SV_POSITION;
				float alphaMultiplier:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f vert(a2v v) {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				o.alphaMultiplier = 1;
				float3 positionWS=TransformObjectToWorld(v.positionOS);
				half3 lightDir=normalize(_MainLightPosition.xyz);
				float planeDistance = INSTANCE(_PlaneDistance);
				float3 planeNormal = INSTANCE(_PlaneNormal);

				GPlane plane = GPlane_Ctor(planeNormal,planeDistance);
				#if _MIRROR
				    float normalProj = dot(positionWS.xyz - plane.position, _PlaneNormal);
					positionWS = positionWS.xyz - normalProj * _PlaneNormal * 2;

					o.alphaMultiplier = step(0,normalProj);
					#if _FALLOFF
						o.alphaMultiplier*=(1-saturate(invlerp(_FallOff,_FallOffEnd,normalProj)));
					#endif
				#else
					GRay ray=GRay_Ctor(positionWS,lightDir);
					half distance=Distance(plane,ray);
					float3 projectionPositionWS=ray.GetPoint(distance);

					#if _FALLOFF
						o.alphaMultiplier*=(1-saturate(invlerp(_FallOff,_FallOffEnd,sqrDistance(positionWS-projectionPositionWS))));
					#endif
					positionWS=projectionPositionWS;
				
				#endif
				
				o.positionCS=TransformWorldToHClip(positionWS);
				return o;
			}
			float4 frag(v2f i) :SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				clip(i.alphaMultiplier-0.01);
				return INSTANCE(_Color)*float4(1,1,1,i.alphaMultiplier);
			}
			ENDHLSL
		}
    }
}
