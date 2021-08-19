Shader "Game/Effects/PlanarShadow"
{
    Properties
    {
        _Color("Color",Color)=(0,0,0,0)
    	_PlaneDistance("Plane Distance(Vertical)",float)=-.5
    	
		[Header(Misc)]
		_Stencil("Stencil ID", Float) = 0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Enum(Off,0,Front,1,Back,2)]_Cull("Cull",int)=2
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Geometry+1" }
        Pass 
        {	
		    Name "PLanar Shadow"
        	
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
			#define IGeometryDetection
			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Geometry.hlsl"

			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_Color)
				INSTANCING_PROP(float,_PlaneDistance)
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
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f vert(a2v v) {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				float3 positionWS=TransformObjectToWorld(v.positionOS);
				half3 lightDir=normalize(_MainLightPosition.xyz);
				GRay ray=GRay_Ctor(positionWS,lightDir);
				GPlane plane=GPlane_Ctor(half3(0,1,0),INSTANCE(_PlaneDistance));
				half distance=PlaneRayDistance(plane,ray);
				positionWS=ray.GetPoint(distance);
				o.positionCS=TransformWorldToHClip(positionWS);
				return o;
			}
			float4 frag(v2f i) :SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				return INSTANCE(_Color);
			}
			ENDHLSL
		}
    }
}
