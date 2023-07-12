
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AlgorithmExtension;
using Geometry;
using Geometry.PointSet;
using Unity.Mathematics;
using UnityEngine;

public struct BVHVolume
{
    public int iteration;
    public List<G2Triangle> triangles;
    public G2Box bounds;

    public BVHVolume(int _iteration, List<G2Triangle> _triangles)
    {
        iteration = _iteration;
        triangles = _triangles;
        var numerable = _triangles.Select(p => (IEnumerable<float2>)p);  //wut?
        bounds = UBounds.GetBoundingBox(numerable.Resolve());
    }
}

public class BVH    //Bounding volume hierarchy
{
    
    public List<BVHVolume> m_Volumes = new List<BVHVolume>();

    void Divide(BVHVolume _volume,out BVHVolume _node1,out BVHVolume _node2)
    {
        var last = _volume.triangles.Count;
        var median = _volume.triangles.Count / 2;
        _volume.triangles
            .Divide(median,
        // .Sort(
        // ESortType.Bubble,
        (_a, _b) =>
        {
            var axis = _volume.bounds.size.maxAxis();
            switch (axis)
            {
                default: throw new InvalidEnumArgumentException();
                case EAxis.X:return _a.baryCentre.x >= _b.baryCentre.x ? 1: -1;
                case EAxis.Y: return _a.baryCentre.y >= _b.baryCentre.y ? 1:-1;
            }
        });

        var nextIteration = _volume.iteration + 1;
        _node1 = new BVHVolume(nextIteration,new List<G2Triangle>(_volume.triangles.Iterate(0,median)));
        _node2 = new BVHVolume(nextIteration, new List<G2Triangle>(_volume.triangles.Iterate(median,last)));
    }
    
    public void Construct(IList<G2Triangle> _triangles, int _maxIteration, int _volumeCapacity)
    {
        m_Volumes.Clear();
        m_Volumes.Add(new BVHVolume(0,new List<G2Triangle>( _triangles)));
        
        bool doBreak = true;
        while (doBreak)
        {
            bool splited = false;
            for (int i = 0; i < m_Volumes.Count; i++)
            {
                var node = m_Volumes[i];
                if (node.iteration >= _maxIteration)
                    continue;
                
                if (node.triangles.Count < _volumeCapacity)
                    continue;

                Divide(node,out var node1,out var node2);
                m_Volumes.Add(node1);
                m_Volumes.Add(node2);
                
                m_Volumes.RemoveAt(i);
                splited = true;
                break;
            }
            
            doBreak = splited;
        }
    }

    public void DrawGizmos()
    {
        int index = 0;
        var matrix = Gizmos.matrix;
        foreach (var node in m_Volumes)
        {
            Gizmos.color = UColor.IndexToColor(index++ % 6);
            Gizmos.matrix = matrix;
            node.bounds.DrawGizmos();
            foreach (var triangle in node.triangles)
                triangle.DrawGizmos();
        }

        Gizmos.matrix = matrix;
    }
}
