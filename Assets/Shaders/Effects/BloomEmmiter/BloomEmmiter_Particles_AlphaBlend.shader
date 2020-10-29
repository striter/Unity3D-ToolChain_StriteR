Shader "Game/Effects/BloomEmitter/Particles/AlphaBlend"
{
	Properties
	{
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color",Color) = (1,1,1,1)
	}
	SubShader
	{ 
		Tags{ "RenderType" = "BloomParticlesAlphaBlend""IgnoreProjector" = "True" "Queue" = "Transparent" "PreviewType"="Plane"}
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off Lighting Off ZWrite Off Fog { Color(0,0,0,0) }
			UsePass "Game/Particle/AlphaBlend/MAIN"
	}
}
