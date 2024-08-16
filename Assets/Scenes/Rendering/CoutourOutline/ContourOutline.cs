using System.Linq;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Rendering.ContourOutline
{
    public class ContourOutline : MonoBehaviour {
        public Texture2D m_ContourTexture;
        [Range(0, 1)] public float m_Simplification = 0.1f;
        private ContourTracing m_ContourTracing;
        private void OnValidate()
        {
            if (m_ContourTexture == null)
                return;

            m_ContourTracing = ContourTracing.FromColorAlpha(m_ContourTexture.width, m_ContourTexture.GetPixels());
        }

        private void OnDrawGizmos()
        {
            if (!m_ContourTracing.ContourAble(out var startPixel))
                return;
            
            Gizmos.matrix = transform.localToWorldMatrix;
            for (var y = 0 ; y < m_ContourTracing.resolution.y; y++)
            {
                for (var x = 0; x < m_ContourTracing.resolution.x; x++)
                {
                    var pixel = new int2(x, y);
                    var color = m_ContourTracing.m_ContourTessellation[pixel.x + pixel.y * m_ContourTracing.resolution.x] ? Color.blue.SetA(.5f) : Color.red.SetA(.5f);
                    var size = .75f;
                    if (pixel.x == startPixel.x && pixel.y == startPixel.y)
                    {
                        color = Color.green.SetA(.5f);
                        size = 1.25f;
                    }
                    
                    Gizmos.color = color;
                    Gizmos.DrawCube(new Vector3(x, 0, y), size * Vector3.one);
                }
            }

            var pixels = m_ContourTracing.MooreNeighborTracing().Select(p=>(float2)p).ToList();
            foreach (var pixel in pixels)
            {
                Gizmos.color = Color.green.SetA(.5f);
                Gizmos.DrawCube(new Vector3(pixel.x,  0,pixel.y), Vector3.one);
            }

            var polygon = UGeometry.GetBoundingPolygon(pixels,0.1f);
            foreach (var point in polygon)
            {
                Gizmos.color = Color.green.SetA(.5f);
                Gizmos.DrawWireSphere(new Vector3(point.x, 0, point.y), .5f);
            }
            polygon.DrawGizmos();

            Gizmos.color = KColor.kOlive;
            // var tolerance = m_Simplification * m_ContourTracing.resolution.x;
            var simplifiedPolygon = new G2Polygon( CartographicGeneralization.VisvalingamWhyatt(polygon.positions.ToList(), (int)(polygon.positions.Length * m_Simplification),true));
            simplifiedPolygon.DrawGizmos();
            
        }
    }
}