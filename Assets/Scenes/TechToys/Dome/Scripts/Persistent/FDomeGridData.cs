
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Procedural.Tile;
using Unity.Mathematics;
using UnityEngine;

namespace Dome
{
    using static math;

    public class FDomeCell
    {
        public bool available;
        public GQuad positions;
        public int[] connections;
    }

    public struct FDomeOutput
    {
        public List<float3> positions;
        public List<FDomeCell> vertices;
        public List<TR> initialTechPoints;
        public GSphere bounds;
    }
    
    [CreateAssetMenu(fileName = "DomeGrid",menuName = "Game/Dome/Grid")]
    public class FDomeGridData : ScriptableObject
    {
        [Min(2)] public int width;
        [Min(0)] public float radius;
        [Min(0)] public int gap;
        [Header("Test")]
        public float interp;
        public float4[] initialTechPoints;

        public void DrawGizmos()
        {
            var o = Output();
            Gizmos.color = Color.white;
            foreach (var position in o.positions)
                Gizmos.DrawWireSphere(position, radius*0.05f);

            foreach (var quad in o.vertices)
            {
                if(!quad.available)
                    continue;
                
                Gizmos.color = Color.white;
                UGizmos.DrawLinesConcat(quad.positions);
                    foreach (var (index,quadIndex) in quad.connections.LoopIndex())
                    {
                        Gizmos.color = UColor.IndexToColor(index);
                        if (quadIndex < 0) continue;
                        UGizmos.DrawLine(quad.positions.Center,o.vertices[quadIndex].positions.Center,.5f);
                    }
            }

            foreach (var (index,tr) in o.initialTechPoints.LoopIndex())
            {
                Gizmos.color = UColor.IndexToColor(index);
                Gizmos.DrawSphere(tr.position,1f);
                UGizmos.DrawArrow(tr.position,math.mul( tr.rotation,kfloat3.forward),6f,1f);
            }
        }

        public FDomeOutput Output()
        {
            var r = radius;
            List<float3> positions = new List<float3>();
            List<FDomeCell> vertices = new List<FDomeCell>();
            
            var length = width;
            for (int j = -length; j <= length; j++)
            {
                for (int i = -length; i <= length; i++)
                {
                    var position = new Vector3(i, 0, j);
                    var final =  normalizesafe(position) *  max(abs(i),abs(j)) ;

                    final = lerp(position, final, interp);
                    positions.Add(final*r);
                }
            }

            var center = new Int2(length, length);
            
            var quadCount = length * 2;
            var sqrGap = gap * gap;
            for (int j = 0; j < quadCount; j++)
            {
                for (int i = 0; i < quadCount; i++)
                {
                    Func<int,int,int> ValidateConnection = (_i, _j) =>
                    {
                        if (_i < 0 || _i >= quadCount || _j < 0 || _j >= quadCount)
                            return -1;
                        return new Int2(_i, _j).ToIndex(quadCount);
                    };

                    var indexes = new Quad<Int2>(
                        new Int2(i, j ),
                        new Int2(i, j+1),
                        new Int2(i+1, j+1),
                        new Int2(i + 1, j));
                    
                    
                    vertices.Add(new FDomeCell()
                    {
                        available = indexes.Any(p=>(p-center).sqrMagnitude > sqrGap),
                        positions = (GQuad) indexes.Convert(p => positions[p.ToIndex(quadCount+1)]),
                        connections = new[]
                        {
                            ValidateConnection(i - 1, j + 1), ValidateConnection(i, j + 1),
                            ValidateConnection(i + 1, j + 1),
                            ValidateConnection(i - 1, j), ValidateConnection(i + 1, j),
                            ValidateConnection(i - 1, j - 1), ValidateConnection(i, j - 1),
                            ValidateConnection(i + 1, j - 1),
                        },
                    });
                }
            }

            //Sanity check for pathfinding
            foreach (var vertex in vertices)
            {
                for (int i = 0; i < 8; i++)
                {
                    var connection = vertex.connections[i];
                    if(connection<=0)
                        continue;

                    if (!vertices[connection].available)
                        vertex.connections[i] = -1;
                }
            }

            return new FDomeOutput()
            {
                positions = positions,
                vertices = vertices,
                initialTechPoints = initialTechPoints.Select(p=>new TR()
                {
                    position = p.xyz,
                    rotation = Unity.Mathematics.quaternion.Euler(0,p.w*kmath.kDeg2Rad,0),
                }).ToList(),
                bounds = new GSphere(0,width*radius),
            };
        }
    }

}