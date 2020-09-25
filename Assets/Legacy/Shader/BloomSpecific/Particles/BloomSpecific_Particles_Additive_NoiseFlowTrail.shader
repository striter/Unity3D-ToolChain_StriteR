Shader "Game/Effect/BloomSpecific/Particles/Additive_NoiseFlow"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_Color("_Color",Color) = (1,1,1,1)
		_SubTex1("Detail Tex",2D) = "white"{}
		_FlowSpeed("Flow Speed(XY Main,ZW Noise)",Vector) = (0,0,0,0)
		_Emmission("Emission",Range(0,5)) = 1
	}

	SubShader
	{ 
		Tags {"RenderType" = "BloomParticlesAdditiveNoiseFlow" "IgnoreProjector" = "True" "Queue" = "Transparent" }
		Cull Back Lighting Off ZWrite Off Fog { Color(0,0,0,0) }

		Blend SrcAlpha One
		USEPASS "Game/Particle/Additive_NoiseFlow_Trail/MAIN"
	}
}
