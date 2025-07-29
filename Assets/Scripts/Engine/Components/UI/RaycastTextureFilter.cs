using System;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;

namespace UnityEngine.UI
{
    public class RaycastTextureFilter : MonoBehaviour , ICanvasRaycastFilter , IMaterialModifier
    {
        [Range(0f, 1f)] public float m_AlphaClip = 0.1f;
        [Min(0f)] public float m_Expand = 0f;
        private G2Polygon m_PolygonNS = G2Polygon.kDefaultUV;
        
        private void OnValidate()
        {
            Construct();
        }

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            Construct();
            return baseMaterial;
        }

        Texture2D CollectTexture()
        {
            var texture = GetComponent<RawImage>()?.texture;
            if (texture == null)
            {
                var sprite = GetComponent<Image>()?.sprite;
                if (sprite != null)
                    texture = sprite.texture;
            }
            return texture as Texture2D;
        }
        
        void Construct()
        {
            var texture = CollectTexture();
            if (texture == null)
                return;
            
            var contourTracingData = ContourTracingData.FromColor(texture.width, texture.ReadPixels(),p => p.a > m_AlphaClip);
            if(!contourTracingData.ContourAble(int2.zero, out _))
                return;
            
            m_PolygonNS = G2Polygon.ConvexHull(contourTracingData.TheoPavlidis(int2.zero));
            var bounds = G2Box.Minmax(0,new float2(texture.width, texture.height));
            var center = bounds.center;
            m_PolygonNS = new G2Polygon(m_PolygonNS.positions.Select(p => bounds.GetUV((p + .5f) + m_Expand * (p - center).normalize()).saturate()));
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow.SetA(.5f);
            Gizmos.matrix = transform.localToWorldMatrix;
            var rectTransform = transform as RectTransform;
            var boundsLS = (G2Box)rectTransform.rect;
            var polygonLS = new G2Polygon(m_PolygonNS.positions.Select(p => boundsLS.GetPoint(p)));
            polygonLS.DrawGizmosXY();
        }
        
        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            var rectTransform = transform as RectTransform;
            var boundsLS = (G2Box)rectTransform.rect;
            if(!rectTransform.TransformScreenToLocal(sp, eventCamera, out var positionLS))
                return false;
            
            var polygonLS = new G2Polygon(m_PolygonNS.positions.Select(p => boundsLS.GetPoint(p)));
            return polygonLS.Contains(positionLS);
        }

    }
}