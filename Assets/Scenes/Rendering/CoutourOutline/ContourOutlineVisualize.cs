using System.Linq;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Rendering.ContourOutline
{
    public class ContourOutlineVisualize : MonoBehaviour {
        public Texture2D m_ContourTexture;
        [Range(0, 1)] public float m_AlphaBias = 0.01f;
        [Range(0, 1)] public float m_Simplification = 0.1f;
        private ContourTracingData m_ContourTracing;
        private void OnValidate()
        {
            if (m_ContourTexture == null)
                return;

            m_ContourTracing = ContourTracingData.FromColor(m_ContourTexture.width, m_ContourTexture.GetPixels(),p=>p.a > m_AlphaBias);
        }

        private void OnDrawGizmos()
        {

            Gizmos.matrix = transform.localToWorldMatrix;
            for (var y = 0 ; y < m_ContourTracing.resolution.y; y++)
            {
                for (var x = 0; x < m_ContourTracing.resolution.x; x++)
                {
                    var pixel = new int2(x, y);
                    var color = m_ContourTracing.m_ContourTessellation[pixel.x + pixel.y * m_ContourTracing.resolution.x] ? Color.blue.SetA(.5f) : Color.red.SetA(.5f);
                    var size = .75f;
                    
                    Gizmos.color = color;
                    Gizmos.DrawCube(new Vector3(x, 0, y), size * Vector3.one);
                }
            }

            if (!m_ContourTracing.ContourAble(out var startPixel))
                return;
            Gizmos.color = Color.green.SetA(.5f);
            Gizmos.DrawCube(new Vector3(startPixel.x, 0, startPixel.y), 1.25f * Vector3.one);
            
            var pixels = m_ContourTracing.TheoPavlidis();
            foreach (var pixel in pixels)
                Gizmos.DrawCube(pixel.to3xz(), Vector3.one);
            UGizmos.DrawLines(pixels,p=>p.to3xz());

            Gizmos.color = KColor.kLime.SetA(.5f);
            var polygon = UGeometry.GetBoundingPolygon(pixels,0.1f);
            foreach (var point in polygon)
                Gizmos.DrawWireSphere(point.to3xz(), .5f);
            UGizmos.DrawLinesConcat(polygon,p=>p.to3xz());

            Gizmos.color = KColor.kOlive;
            var simplifiedPolygon = new G2Polygon( CartographicGeneralization.VisvalingamWhyatt(polygon, (int)(polygon.Count * m_Simplification),true));
            simplifiedPolygon.DrawGizmos();

            foreach (var pixel in m_ContourTracing.DFS(startPixel))
            {
                Gizmos.color = Color.cyan.SetA(.3f);
                Gizmos.DrawCube(new Vector3(pixel.x, 0, pixel.y),  .5f * Vector3.one);
            }
            
        }
    }
}