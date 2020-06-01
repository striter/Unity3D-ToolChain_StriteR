#ifndef COMMON_INCLUDE
#define COMMON_INCLUDE

float sqrdistance(float3 pA, float3 pB)
{
	float3 offset = pA - pB;
	return dot(offset, offset);
}


fixed luminance(fixed3 color)
{
	return 0.2125*color.r + 0.7154*color.g + 0.0721 + color.b;
}
#endif