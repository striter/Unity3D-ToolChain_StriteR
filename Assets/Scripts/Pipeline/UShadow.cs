using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Pipeline
{
    public class UShadow
    {
        
        public static void GetScaleAndBiasForLinearDistanceFade(float _fadeDistance, float _border, out float _scale, out float _bias)
        {
            if (_border < 0.0001f)
            {
                var multiplier = 1000f;
                _scale = multiplier;
                _bias = -_fadeDistance * multiplier;
                return;
            }

            _border = 1 - _border;
            _border = umath.pow2(_border);
            var distanceFadeNear = _border * _fadeDistance;
            _scale = 1f / (_fadeDistance - distanceFadeNear);
            _bias = -distanceFadeNear / (_fadeDistance - distanceFadeNear);
        }
        
        public static Matrix4x4 CalculateWorldToShadowMatrix(Matrix4x4 proj, Matrix4x4 view)
        {
            // Currently CullResults ComputeDirectionalShadowMatricesAndCullingPrimitives doesn't
            // apply z reversal to projection matrix. We need to do it manually here.
            if (SystemInfo.usesReversedZBuffer)
            {
                proj.m20 = -proj.m20;
                proj.m21 = -proj.m21;
                proj.m22 = -proj.m22;
                proj.m23 = -proj.m23;
            }
            
            var worldToShadow = proj * view;

            var textureScaleAndBias = Matrix4x4.identity;
            textureScaleAndBias.m00 = 0.5f;
            textureScaleAndBias.m11 = 0.5f;
            textureScaleAndBias.m22 = 0.5f;
            textureScaleAndBias.m03 = 0.5f;
            textureScaleAndBias.m23 = 0.5f;
            textureScaleAndBias.m13 = 0.5f;
            // textureScaleAndBias maps texture space coordinates from [-1,1] to [0,1]

            return textureScaleAndBias * worldToShadow;
        }
        
        public static void CalculateDirectionalShadowMatrices(Light light, GBox shadowCasterBounds, out Matrix4x4 viewMatrix, out Matrix4x4 proj) {
            // Calculate light position behind the bounds
            var padding = .5f; // Adjust as needed
            var lightDirection = light.transform.forward;
            var lightPos = shadowCasterBounds.center - (float3)(lightDirection * (shadowCasterBounds.extent.magnitude()+ padding));
            // Create view matrix (light's perspective)
            viewMatrix = Matrix4x4.TRS(lightPos, Quaternion.LookRotation(lightDirection), Vector3.one).inverse;
            var boundsLS = viewMatrix * shadowCasterBounds;

            // Apply padding to avoid clipping
            var min = boundsLS.min - padding;
            var max = boundsLS.max + padding;

            // Create orthographic projection matrix
            proj = Matrix4x4.Ortho(
                min.x, max.x, 
                min.y, max.y, 
                -max.z, -min.z 
            );
            
            if (SystemInfo.usesReversedZBuffer)
            {
                proj.m20 = -proj.m20;
                proj.m21 = -proj.m21;
                proj.m22 = -proj.m22;
                proj.m23 = -proj.m23;
            }
        }
    }
}