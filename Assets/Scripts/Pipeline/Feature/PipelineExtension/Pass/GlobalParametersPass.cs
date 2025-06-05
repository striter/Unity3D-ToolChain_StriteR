using System.Collections;
using System.Collections.Generic;
using Runtime.Geometry;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    
        public class GlobalParametersPass : ScriptableRenderPass
        {
        #region IDs
            private static readonly int kFrustumCornersRayBL = Shader.PropertyToID("_FrustumCornersRayBL");
            private static readonly int kFrustumCornersRayBR = Shader.PropertyToID("_FrustumCornersRayBR");
            private static readonly int kFrustumCornersRayTL = Shader.PropertyToID("_FrustumCornersRayTL");
            private static readonly int kFrustumCornersRayTR = Shader.PropertyToID("_FrustumCornersRayTR");

            private static readonly int kOrthoCameraDirection = Shader.PropertyToID("_OrthoCameraDirection");
            private static readonly int kOrthoCameraPositionBL = Shader.PropertyToID("_OrthoCameraPosBL");
            private static readonly int kOrthoCameraPositionBR = Shader.PropertyToID("_OrthoCameraPosBR");
            private static readonly int kOrthoCameraPositionTL = Shader.PropertyToID("_OrthoCameraPosTL");
            private static readonly int kOrthoCameraPositionTR = Shader.PropertyToID("_OrthoCameraPosTR");

            private static readonly int kMatrixV = Shader.PropertyToID("_Matrix_V");
            private static readonly int kMatrixI_V = Shader.PropertyToID("_Matrix_I_V");
            private static readonly int kMatrix_VP = Shader.PropertyToID("_Matrix_VP");
            private static readonly int kMatrix_I_VP=Shader.PropertyToID("_Matrix_I_VP");
            private static readonly int kMatrix_P = Shader.PropertyToID("_Matrix_P");
            private static readonly int kMatrix_I_P = Shader.PropertyToID("_Matrix_I_P");
        #endregion

            public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
            {
                var camera = _renderingData.cameraData.camera;
                if (_renderingData.cameraData.camera.orthographic)
                {
                    Shader.SetGlobalVector(kOrthoCameraDirection,camera.transform.forward);
                    camera.CalculateOrthographicPositions(out var topLeft,out var topRight,out var bottomLeft,out var bottomRight);
                    Shader.SetGlobalVector(kOrthoCameraPositionBL, bottomLeft);
                    Shader.SetGlobalVector(kOrthoCameraPositionBR, bottomRight);
                    Shader.SetGlobalVector(kOrthoCameraPositionTL, topLeft);
                    Shader.SetGlobalVector(kOrthoCameraPositionTR, topRight);
                }
                else
                {
                    var rays = new GFrustum(camera).rays;
                    Shader.SetGlobalVector(kFrustumCornersRayBL, rays.bottomLeft.direction.to4());
                    Shader.SetGlobalVector(kFrustumCornersRayBR, rays.bottomRight.direction.to4());
                    Shader.SetGlobalVector(kFrustumCornersRayTL, rays.topLeft.direction.to4());
                    Shader.SetGlobalVector(kFrustumCornersRayTR, rays.topRight.direction.to4());
                }
            
                var projection = GL.GetGPUProjectionMatrix(_renderingData.cameraData.GetProjectionMatrix(),_renderingData.cameraData.IsCameraProjectionMatrixFlipped());
                var view = _renderingData.cameraData.GetViewMatrix();
                var vp = projection * view;

                Shader.SetGlobalMatrix(kMatrix_VP,vp);
                Shader.SetGlobalMatrix(kMatrix_I_VP,vp.inverse);
                Shader.SetGlobalMatrix(kMatrixV,view);
                Shader.SetGlobalMatrix(kMatrixI_V,view.inverse);
                Shader.SetGlobalMatrix(kMatrix_P,projection);
                Shader.SetGlobalMatrix(kMatrix_I_P,projection.inverse);
            }


            public void Dispose()
            {
            }
        }
}