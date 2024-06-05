using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Curves.LineSegments
{
    [Serializable]
    public class GDragonCurve
    {
        [Range(0,180)] public float angle;
        [Range(1,16)] public int iteration;

        public static readonly GDragonCurve kDefault = new GDragonCurve()
        {
            angle = 90,
            iteration = 5,
        };

        private static List<int> GetSequence(int _iteration)
        {
            var turnSequence = new List<int>();
            var copy = new List<int>(turnSequence);
            for (int i = 0; i < _iteration; i++)
            {
                copy.Clear();
                copy.AddRange(turnSequence);
                copy.Reverse();
                turnSequence.Add(1);
                turnSequence.AddRange(copy.Select(turn => -turn));
            }
            
            return turnSequence;
        }
        
        public List<float3> Output()
        {
            var positions = new List<float3>();
            var position = float3.zero;
            positions.Add(position);
            var direction = kfloat3.forward;
            var forwardRotation = quaternion.Euler(0,angle*kmath.kDeg2Rad,0);
            var backwardRotation = math.inverse(forwardRotation);
            foreach (var turn in GetSequence(iteration))
            {
                direction = math.mul(turn == 1 ? forwardRotation:backwardRotation,direction);
                
                position += direction;
                positions.Add(position);
            }
            return positions;
        }

        public void DrawGizmos()
        {
            var output = Output();
            UGizmos.DrawLines(output);
        }
    }
}