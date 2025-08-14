using System;
using System.Linq;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;

namespace UnityEngine.UI
{
    public class RaycastTextureFilter : MonoBehaviour , ICanvasRaycastFilter , IMaterialModifier
    {
        public enum EMode
        {
            AutomaticConvex,
            Manual,
        }

        public EMode m_Mode = EMode.AutomaticConvex;
        [Foldout(nameof(m_Mode),EMode.AutomaticConvex),Min(0f)] public float m_Expand = 0f;
        [Foldout(nameof(m_Mode), EMode.Manual)] public G2Polygon m_PolygonNS = G2Polygon.kDefaultUV;

        private void OnValidate()
        {
            if(m_Mode == EMode.AutomaticConvex)
                ConstructConvex(m_Expand);
        }

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            if(m_Mode == EMode.AutomaticConvex)
                ConstructConvex(m_Expand);
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

        private const float m_AlphaClip = .1f;
        [InspectorButtonFoldout(nameof(m_Mode), EMode.Manual)]
        void ConstructConvex(float _expand = 0f)
        {
            var texture = CollectTexture();
            if (texture == null)
                return;

            const int downSample = 4;
            var contourTracingData = ContourTracingData.FromColor(texture.width / downSample, texture.ReadPixels(downSample),p => p.a > m_AlphaClip);
            if(!contourTracingData.ContourAble(int2.zero, out var startPixel))
                return;

            var positions = contourTracingData.TheoPavlidis(startPixel);
            m_PolygonNS = G2Polygon.ConvexHull(positions);
            
            var bounds = G2Box.Minmax(0,new float2(texture.width / downSample, texture.height  / downSample));
            var center = bounds.center;
            m_PolygonNS = new G2Polygon(m_PolygonNS.positions.Select(p => bounds.GetUV((p + .5f) + _expand * (p - center).normalize()).saturate()));
        }

        [InspectorButtonFoldout(nameof(m_Mode),EMode.Manual)]
        void ConstructConcave(float _threshold = .5f,int _desireCount = 32)
        {
            var texture = CollectTexture();
            if (texture == null)
                return;

            const int downSample = 4;
            var contourTracingData = ContourTracingData.FromColor(texture.width / downSample, texture.ReadPixels(downSample),p => p.a > m_AlphaClip);
            if(!contourTracingData.ContourAble(int2.zero, out var startPixel))
                return;

            var positions = contourTracingData.TheoPavlidis(startPixel);
            m_PolygonNS = G2Polygon.AlphaShape(positions,_threshold);
            
            var bounds = G2Box.Minmax(0,new float2(texture.width / downSample, texture.height  / downSample));
            m_PolygonNS = new G2Polygon(m_PolygonNS.positions.Select(p => bounds.GetUV((p + .5f) ).saturate()));
            if(m_PolygonNS.Count > _desireCount)
                m_PolygonNS = UCartographicGeneralization.VisvalingamWhyatt(m_PolygonNS.positions, _desireCount);
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