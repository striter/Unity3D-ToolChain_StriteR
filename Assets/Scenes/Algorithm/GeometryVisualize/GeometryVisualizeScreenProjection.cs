using Runtime.Geometry;
using Runtime.Geometry.Extension;
using UnityEngine;

namespace Examples.Algorithm.MathmaticVisualize
{
    public class GeometryVisualizeScreenProjection : MonoBehaviour
    {
        public GSphere m_Sphere = GSphere.kDefault;
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var camera = UnityEditor.SceneView.lastActiveSceneView.camera;
            Gizmos.matrix = camera.transform.localToWorldMatrix;
            m_Sphere.DrawGizmos();
            UGizmos.DrawString(((int)m_Sphere.ScreenProjection(camera.worldToCameraMatrix,camera.pixelHeight,camera.fieldOfView * kmath.kDeg2Rad)).ToString(),m_Sphere.Origin);
        }
#endif
    }
    
}