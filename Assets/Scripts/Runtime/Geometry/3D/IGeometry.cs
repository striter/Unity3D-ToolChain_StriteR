using System.Collections.Generic;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    public interface IGeometry  : IGeometry<float3> { }

    public interface ISurface : IGeometry
    {
        public float3 Normal { get; }
    }
    
    public interface IVolume : IGeometry
    {
        float3 GetSupportPoint(float3 _direction);
        public GBox GetBoundingBox();
        public GSphere GetBoundingSphere();
    }
    
    public interface IConvex : IGeometry , IEnumerable<float3>
    {
        public IEnumerable<float3> GetAxes();
    }

    public static class IVolume_Extension
    {
#if UNITY_EDITOR
        public static void DrawHandles(this IVolume _volume)
        {
            switch (_volume)
            {
                default: UnityEngine.Debug.LogWarning($"Unknown Handles For Volume {_volume.GetType()}"); break;
                case GBox box: UnityEditor.Handles.DrawWireCube(box.center,box.size); break;
                case GSphere sphere: UnityEditor.UHandles.DrawWireSphere(sphere.center,sphere.radius); break;
            }
        }
#endif
    }

    public static class IConvex_Extension
    {
        private static readonly List<float3> kPoints = new List<float3>();
        public static IEnumerable<GLine> GetEdges(this IConvex _convex)
        {
            kPoints.Clear();
            kPoints.AddRange(_convex);
            for (var i = 0; i < kPoints.Count - 1; i++)
                yield return new GLine(kPoints[i], kPoints[i + 1]);
        }
    }
}