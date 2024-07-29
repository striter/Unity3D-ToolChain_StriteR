using System;
using System.Linq.Extensions;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Curves.LineSegments
{
    [Serializable]
    public struct GChaikinCurve : ILine
    {
        public float3[] vertices;
        [Clamp(1,10)]public int iteration;
        [Range(0.001f,.5f)]public float ratio;
        public bool closed;
        public static readonly GChaikinCurve kDefault = new GChaikinCurve()
        {
            vertices = new float3[]{new float3(-1,0,-1),new float3(0,0,1),new float3(1,0,-1)},
            iteration = 3,
            ratio = .25f,
            closed = true,
        };

        public GChaikinCurve(float3[] _vertices,int _iteration,float _ratio,bool _closed)
        {
            vertices = _vertices;
            iteration = _iteration;
            ratio = _ratio;
            closed = _closed;
        }

        public List<float3> Output()
        {
            List<float3> vertices0 = new List<float3>();
            List<float3> vertices1 = new List<float3>();
            vertices.FillList(vertices1);
            
            int iterationIndex = 0;
            while (iterationIndex++ < iteration)
            {
                vertices1.FillList(vertices0);
                
                vertices1.Clear();
                for (int i = 0; i < vertices0.Count; i++)
                {
                    var count = vertices0.Count;
                    bool edge = i == 0 || i == count - 1;
                    var cur = vertices0[i];
                    if (!closed && edge)
                    {
                        vertices1.Add(cur);
                        continue;
                    }

                    var startIndex = (i - 1 + count) % count;
                    var endIndex = (i + 1) % count;
                    
                    var start = vertices0[startIndex];
                    var end = vertices0[endIndex];
                    
                    vertices1.Add(math.lerp(cur,start,ratio));
                    vertices1.Add(math.lerp(cur,end,ratio));
                }
            }
            return vertices1;
        }

        public float3 Origin => vertices[0];
        public void DrawGizmos() => DrawGizmos(true);
        public void DrawGizmos(bool _indicator)
        {
            var outputs = Output();

            Gizmos.color = Color.green;
            if (closed)
                UGizmos.DrawLinesConcat(outputs, p => p);
            else
                UGizmos.DrawLines(outputs, p => p);

            if (!_indicator)
                return;
            Gizmos.color = Color.white;
            if (closed)
                UGizmos.DrawLinesConcat(vertices, p => p);
            else
                UGizmos.DrawLines(vertices, p => p);
        }
    }
}