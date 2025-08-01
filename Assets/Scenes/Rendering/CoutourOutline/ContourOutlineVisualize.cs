using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Runtime.Pool;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Rendering.ContourOutline
{
    public class ContourOutlineVisualize : MonoBehaviour {
        public Texture2D m_ContourTexture;
        public bool m_MaxElement = false;
        public bool m_CentricDFS = false;
        [Range(0, 1)] public float m_Bias = 0.01f;
        [Range(0, 1)] public float m_Simplification = 0.1f;
        private ContourTracingData m_ContourTracing;
        [Range(0f, 1f)] public float m_AlphaShapeThreshold = 0.5f;
        private void OnValidate()
        {
            if (m_ContourTexture == null)
                return;
            var pixels = m_ContourTexture.width * m_ContourTexture.height;
            if (pixels > 256 * 256)
            {
                Debug.Log("Too many pixels");
                return;
            }
            m_ContourTracing = ContourTracingData.FromColor(m_ContourTexture.width, m_ContourTexture.ReadPixels(),p=> m_MaxElement?p.to4().maxElement() > m_Bias : p.a > m_Bias);
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            for (var y = 0; y < m_ContourTracing.resolution.y; y++)
            {
                for (var x = 0; x < m_ContourTracing.resolution.x; x++)
                {
                    var pixel = new int2(x, y);
                    var color = m_ContourTracing.tesssellation[pixel.x + pixel.y * m_ContourTracing.resolution.x]
                        ? Color.blue.SetA(.5f)
                        : Color.red.SetA(.5f);
                    var size = .75f;
            
                    Gizmos.color = color;
                    Gizmos.DrawCube(new Vector3(x, 0, y), size * Vector3.one);
                }
            }

            var initialPixel = m_CentricDFS ? m_ContourTracing.resolution / 2 : int2.zero;
            if (!m_ContourTracing.ContourAble(initialPixel, out var startPixel))
                return;
            Gizmos.color = Color.green.SetA(.5f);
            Gizmos.DrawCube(new Vector3(startPixel.x, 0, startPixel.y), 1.25f * Vector3.one);

            var pixels = m_ContourTracing.TheoPavlidis(initialPixel);
            foreach (var pixel in pixels)
                Gizmos.DrawCube(pixel.to3xz(), Vector3.one);
            UGizmos.DrawLines(pixels, p => p.to3xz());
            
            
            Gizmos.color = KColor.kLime.SetA(.5f);
            var convexPolygon = G2Polygon.QuickHull(pixels);
            foreach (var point in convexPolygon)
                Gizmos.DrawWireSphere(point.to3xz(), .5f);
            UGizmos.DrawLinesConcat(convexPolygon, p => p.to3xz());
            
            foreach (var pixel in m_ContourTracing.DFS(startPixel))
            {
                Gizmos.color = Color.cyan.SetA(.3f);
                Gizmos.DrawCube(new Vector3(pixel.x, 0, pixel.y), .5f * Vector3.one);
            }
            
            // Gizmos.color = KColor.kOlive;
            // var simplifiedPolygon = new G2Polygon(UCartographicGeneralization.VisvalingamWhyatt(pixels, (int)(pixels.Count * m_Simplification)));
            // simplifiedPolygon.DrawGizmos();
            // G2Polygon.AlphaShape(simplifiedPolygon.positions,m_AlphaShapeThreshold).DrawGizmos();
        }
    }
}