#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

//Common 
#include "Common/Rotation.hlsl"
#include "Common/Noises/Index.hlsl"
#include "Common/Constants.hlsl"
//Mapping
#include "Common/Complex.hlsl"
#include "Common/ValueMapping.hlsl"
#include "Common/UVMapping.hlsl"

//Transformations
#include "Common/Input.hlsl"
#include "Common/ColorTransform.hlsl"
#include "Common/DepthTransform.hlsl"
#include "Common/SpaceTransform.hlsl"
#include "Common/Normal.hlsl"

//Macros
#include "Common/Macros/Instance.hlsl"
#include "Common/Macros/Fog.hlsl"