#ifdef GeometryDetection
#define Geometry
#endif

#ifdef GeometrySDF
#ifndef Geometry
#define Geometry
#endif
#endif

#ifdef Geometry
#include "Geometry/GeometryInput.hlsl"
#ifdef GeometryDetection
#include "Geometry/GeometryDetection.hlsl"
#endif
#ifdef GeometrySDF
#include "Geometry/GeometrySDFInput.hlsl"
#include "Geometry/GeometrySDFMethods.hlsl"
#endif
#endif