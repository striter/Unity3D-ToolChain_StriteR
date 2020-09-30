Shader "Game/BloomEmmiter/Particles/Additive"
{
	Properties
	{
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color",Color) = (1,1,1,1)
	}
	SubShader
	{ 
		Tags{ "RenderType" = "BloomParticlesAdditive""IgnoreProjector" = "True" "Queue" = "Transparent" "PreviewType" ="Plane"}
		Blend SrcAlpha One
		Cull Off Lighting Off ZWrite Off Fog { Color(0,0,0,0) }
		UsePass "Game/Particle/Additive/MAIN"
	}
}
