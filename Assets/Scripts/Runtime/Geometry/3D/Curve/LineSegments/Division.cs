using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

namespace Geometry.Curves.LineSegments
{
    //https://www.redblobgames.com/maps/noisy-edges/
    [Serializable]
    public struct GDivisionCurve
    {
        public float3 begin;
        public float3 end;
        public float3 control1;
        public float3 control2;

        [Clamp(1,8)] public int level;
        [Range(0,1f)] public float amplitude;

        public static readonly GDivisionCurve kDefault = new GDivisionCurve()
        {
            begin = new float3(-1,0,0),
            end  = new float3(1,0,0),
            control1 = new float3(0,0,1),
            control2 = new float3(0,0,-1),
            
            level = 3,
            amplitude = .25f,
        };

        public struct DivisionOutput
        {
            public Quad<float3> quad;
            public int quadIndex;
        }
        
        public List<float3> Output(out List<DivisionOutput> _divisions,Random _random = null)
        {
            _divisions = new List<DivisionOutput>();
            _divisions.Add (new DivisionOutput()
            {
                quadIndex = 0,
                quad = new Quad<float3>(control2,begin,control1,end),
            });
            List<DivisionOutput> newDivisions = new List<DivisionOutput>();

            List<float3> vertices = new List<float3>();
            vertices.Add(_divisions[0].quad.L);
            vertices.Add(_divisions[0].quad.R);
            uint index = 1;
            for (int i = 1; i <= level; i++)
            {
                newDivisions.Clear();
                int quadIndex = 0;
                int divisionIndex = 0;
                for (int j = 0; j < _divisions.Count; j++)
                {
                    var random = (float)(_random?.NextDouble() ?? ULowDiscrepancySequences.Halton( index++, 1));
                    var divisionInfo = _divisions[j];
                    var quad = divisionInfo.quad;
                    var controlT = quad.F;
                    var controlB = quad.B;
            
                    var division = math.lerp(controlB,controlT,  random * amplitude);
                    divisionIndex += 1;
                    vertices.Insert(divisionIndex,division);
                    divisionIndex += 1;
                    
                    var lf = math.lerp(quad.L, quad.F, .5f);
                    var lb = math.lerp(quad.L, quad.B, .5f);
                    newDivisions.Add(new DivisionOutput(){ quadIndex = quadIndex++,quad = new Quad<float3>(lb,quad.L,lf,division)});

                    var rf = math.lerp(quad.R, quad.F, .5f);
                    var rb = math.lerp(quad.R, quad.B, .5f);
                    newDivisions.Add(new DivisionOutput(){ quadIndex = quadIndex++,quad = new Quad<float3>(rb,division,rf,quad.R)});
                }
                _divisions.Clear();
                _divisions.AddRange(newDivisions);
            }
            
            return vertices;
        }

    }
    
    public static class UDivision
    {
        public static void DrawGizmos(this GDivisionCurve _curve,bool _indicator = true)
        {
            var outputs = _curve.Output(out var divisions);
            UnityEngine.Gizmos.color = Color.green;
            UGizmos.DrawLines(outputs,_p=>_p);

            if (!_indicator)
                return;
            
            UnityEngine.Gizmos.color = Color.blue;
            UnityEngine.Gizmos.DrawWireSphere(_curve.begin,.05f);
            UnityEngine.Gizmos.DrawWireSphere(_curve.end,.05f);

            UnityEngine.Gizmos.color = Color.red;
            UnityEngine.Gizmos.DrawWireSphere(_curve.control1,.05f);
            UnityEngine.Gizmos.DrawWireSphere(_curve.control2,.05f);

            UnityEngine.Gizmos.color = Color.white;
            UnityEngine.Gizmos.DrawLine(_curve.begin,_curve.control1);
            UnityEngine.Gizmos.DrawLine(_curve.begin,_curve.control2);
            UnityEngine.Gizmos.DrawLine(_curve.end,_curve.control1);
            UnityEngine.Gizmos.DrawLine(_curve.end,_curve.control2);
            
            UnityEngine.Gizmos.color = Color.white.SetA(.5f);
            for (int i = 0; i < divisions.Count; i++)
            {
                UGizmos.DrawLinesConcat(divisions[i].quad.Iterate(),_p=>_p);
                UGizmos.DrawString(divisions[i].quad.GetBaryCenter_Dynamic(),divisions[i].quadIndex.ToString());
            }
        }
    }
}