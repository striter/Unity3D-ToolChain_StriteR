//Instance
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
    #define INSTANCING_BUFFER_START CBUFFER_START(UnityPerMaterial)
    #define INSTANCING_PROP(type,param) type param;
    #define INSTANCE(param) param
    #define INSTANCING_BUFFER_END CBUFFER_END
#else
    #define INSTANCING_BUFFER_START UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    #define INSTANCING_PROP(type,param) UNITY_DEFINE_INSTANCED_PROP(type,param)
    #define INSTANCE(param) UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,param)
    #define INSTANCING_BUFFER_END UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)
#endif

#define TRANSFORM_TEX_INSTANCE(uv,tex) TransformTex(uv,INSTANCE(tex##_ST))
#define TRANSFORM_TEX_FLOW_INSTANCE(uv,tex) TransformTex_Flow(uv,INSTANCE(tex##_ST))