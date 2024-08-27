using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using Unity.Mathematics;

namespace Runtime.Geometry.Extension.BoundingSphere
{
    //https://ep.liu.se/ecp/034/009/ecp083409.pdf
    public static class EPOS //ExtremalPointsOptimalSphere
    {
        public enum EMode
        {
            EPOS6 = 3,
            EPOS14 = 7,
            EPOS26 = 13,
            EPOS98 = 49,
        }
        
        private static readonly float3[] kNormals49 = { 
            new(1, 0, 0), new(0, 1, 0), new(0, 0, 1) ,
            new(1, 1, 1), new(1, 1, -1), new(1, -1, 1), new(1, -1, -1),
            new (0,1,2),new (1,-1,0),new(1,0,1),new(1,0,1),new(0,1,1),new(0,1,-1),
                                            
            new(0, 1, 2),new(0, 2, 1),new(1, 0, 2),new(2, 0, 1),new(1, 2, 0),new(2, 1, 0),
            new(0,1,-2),new(0,2,-1),new(1,0,-2),new(2,0,1),new(1,-2,0),new(2,-1,0),
                                            
            new(1, 1, 2), new(2, 1, 1), new(1, 2, 1), new(1, -1, 2), new(1, 1, -2), new(1, -1, -2),
            new(2,-1,1),new(2,1,-1),new(2,-1,-1),new(1,-2,1),new(1,2,-1),new(1,-2,1) ,
            
            new(2, 2, 1), new(1, 2, 2), new(2, 1, 2), new(2, -2, 1), new(2, 2, -1), new(2, -2, -1),
            new(1, -2, 2), new(1, 2, -2),new(1,-2,-2), new(2, -1, 2), new(2, 1, -2), new(2, -1, -2) 
        };

        private static List<float3> positions = new List<float3>();
        public static IEnumerable<float3> GetExtremalPoints(IList<float3> _positions, int _normals)
        {
            for(var i=0;i<_normals;i++)
            {
                var normal = kNormals49[i];
                _positions.MinmaxElement(p=>math.dot(normal,p),out var min,out var max);
                yield return min;
                yield return max;
            }
        }
        
        public static GSphere Evaluate(IEnumerable<float3> _positions,EMode _normals = EMode.EPOS14,Func<IEnumerable<float3>,GSphere> _exactSolver = null)
        {
            _exactSolver ??= Welzl<GSphere,float3>.Evaluate;
            positions.Clear();
            positions.AddRange(_positions);

            var n = positions.Count;
            var k = (int)_normals;
            return _exactSolver(n > 2 * k ? GetExtremalPoints(positions, k) : positions);
        }
    }
    
}