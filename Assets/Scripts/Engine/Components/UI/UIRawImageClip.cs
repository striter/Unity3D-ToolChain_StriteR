using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI
{
    public class UIRawImageClip : RawImage ,ICanvasRaycastFilter
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            var clipRect = uvRect;
            Texture tex = mainTexture;
            vh.Clear();
            if (tex != null)
            {
                var r = GetPixelAdjustedRect();
                var v = new Vector4(r.x + uvRect.xMin * r.width, r.y + uvRect.yMin * r.height, r.x + r.width * uvRect.xMax, r.y + r.height * uvRect.yMax);
                
                
                var scaleX = tex.width * tex.texelSize.x;
                var scaleY = tex.height * tex.texelSize.y;
                {
                    var color32 = color;
                    vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(clipRect.xMin * scaleX, clipRect.yMin * scaleY));
                    vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(clipRect.xMin * scaleX, clipRect.yMax * scaleY));
                    vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(clipRect.xMax * scaleX, clipRect.yMax * scaleY));
                    vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(clipRect.xMax * scaleX, clipRect.yMin * scaleY));

                    vh.AddTriangle(0, 1, 2);
                    vh.AddTriangle(2, 3, 0);
                }
            }
        }
        
        public override void SetNativeSize()
        {
            Texture tex = mainTexture;
            if (tex != null)
            {
                int w = Mathf.RoundToInt(tex.width);
                int h = Mathf.RoundToInt(tex.height);
                rectTransform.anchorMax = rectTransform.anchorMin;
                rectTransform.sizeDelta = new Vector2(w, h);
            }
        }

        private Vector3[] worldCorners = new Vector3[4];
        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            rectTransform.GetWorldCorners(worldCorners);
            var screenMin = RectTransformUtility.WorldToScreenPoint(eventCamera, worldCorners[0]);
            var screenMax = RectTransformUtility.WorldToScreenPoint(eventCamera, worldCorners[2]);

            var size = screenMax - screenMin;
            var screenRect = new Rect(screenMin + size * uvRect.min, size * (uvRect.max - uvRect.min));
            
            return screenRect.Contains(screenPoint);
        }
    }
}