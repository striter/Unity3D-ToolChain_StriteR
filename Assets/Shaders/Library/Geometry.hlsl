#ifdef IGeometryDetection
#define Geometry
#endif

#ifdef IGeometrySDF
#ifndef Geometry
#define Geometry
#endif
#endif

#ifdef Geometry
#include "Geometry/GeometryInput.hlsl"
#ifdef IGeometryDetection
#include "Geometry/GeometryDetection.hlsl"
#endif
#ifdef IGeometrySDF
#include "Geometry/GeometrySDFInput.hlsl"
#include "Geometry/GeometrySDFMethods.hlsl"
#endif
#endif