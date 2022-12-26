using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry
{
    [Serializable]
    public struct GChaikinCurveInput
    {
        public float3[] vertices;
        [Clamp(1,10)]public int iteration;
        [Range(0.001f,.5f)]public float ratio;
        public bool closed;
        public static readonly GChaikinCurveInput kDefault = new GChaikinCurveInput()
        {
            vertices = new float3[]{new float3(-1,0,-1),new float3(0,0,1),new float3(1,0,-1)},
            iteration = 3,
            ratio = .25f,
            closed = true,
        };

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
    }
    
    [Serializable]
    public struct GChaikinCurve : ISerializationCallbackReceiver
    {
        public GChaikinCurveInput input;
        [HideInInspector] public float3[] vertices;
        public GChaikinCurve(GChaikinCurveInput _input)
        {
            input = _input;
            vertices = default;
            Ctor();
        }

        void Ctor()
        {
            vertices = input.Output().ToArray();
        }

        public void OnBeforeSerialize(){}
        public void OnAfterDeserialize() => Ctor();

        public static readonly GChaikinCurve kDefault = new GChaikinCurve(GChaikinCurveInput.kDefault);
    }
    
    public static class UChaikinCurve
    {
        public static void DrawGizmos(this GChaikinCurve _curve)
        {
            Gizmos.color = Color.white;
            if(_curve.input.closed)
                Gizmos_Extend.DrawLinesConcat(_curve.input.vertices,p=>(Vector3)p);
            else
                Gizmos_Extend.DrawLines(_curve.input.vertices,p=>(Vector3)p);

            Gizmos.color = Color.green;
            if(_curve.input.closed)
                Gizmos_Extend.DrawLinesConcat(_curve.vertices,p=>(Vector3)p);
            else
                Gizmos_Extend.DrawLines(_curve.vertices,p=>(Vector3)p);
        }
        
    }
}