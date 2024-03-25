#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

public static class UGizmos
{
    public static void DrawWireCube(Vector3 _pos, Quaternion _rot, Vector3 _cubeSize)
    {
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        Handles_Extend.DrawWireCube(_pos, _rot, _cubeSize);
    }

    public static void DrawArrow(Vector3 _pos, Vector3 _direction, float _length, float _radius) => DrawArrow(_pos, Quaternion.LookRotation(_direction), _length, _radius);
    public static void DrawArrow(Vector3 _pos, Quaternion _rot, float _length, float _radius)
    {
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        Handles_Extend.DrawArrow(_pos, _rot, _length, _radius);
    }
    public static void DrawCylinder(Vector3 _pos, Vector3 _up, float _radius, float _height)
    {
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        Handles_Extend.DrawCylinder(_pos, _up, _radius, _height);
    }

    public static void DrawTrapezium(Vector3 _pos, Quaternion _rot, Vector4 _trapeziumInfo)
    {
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        Handles_Extend.DrawTrapezium(_pos, _rot, _trapeziumInfo);
    }
    public static void DrawLine(Vector3 _src, Vector3 _dest, float _normalizedLength=1f)
    {
        Gizmos.DrawLine(_src,(_src+(_dest-_src)*_normalizedLength));
    }

    public static void DrawLines(IList<Vector3> _points)
    {
        var count = _points.Count;
        for(int i=0;i<count-1;i++)
            Gizmos.DrawLine(_points[i],_points[i+1]);
    }
    
    public static void DrawLines(IEnumerable<Vector3> _points)
    {
        Vector3 tempPoint=default;
        foreach (var (index,point) in _points.LoopIndex())
        {
            if (index == 0)
            {
                tempPoint = point;
                continue;
            }

            Gizmos.DrawLine(tempPoint,point);
            tempPoint = point;
        }
    }
    
    public static void DrawLines<T>(IEnumerable<T> _points,Func<T,Vector3> _convert)
    {
        Vector3 tempPoint=default;
        foreach (var (index,value) in _points.LoopIndex())
        {
            var point = _convert(value);
            if (index == 0)
            {
                tempPoint = point;
                continue;
            }

            Gizmos.DrawLine(tempPoint,point);
            tempPoint = point;
        }
    }

    public static void DrawLinesConcat(params float3[] _lines) => DrawLinesConcat(_lines.AsEnumerable());
    public static void DrawLinesConcat(IEnumerable<float3> _points)
    {
        float3 tempPoint = default;
        float3 startPoint = default;
        foreach (var (index,point) in _points.LoopIndex())
        {
            if (index == 0)
            {
                tempPoint = point;
                startPoint = point;
                continue;
            }

            Gizmos.DrawLine(tempPoint,point);
            tempPoint = point;
        }
        Gizmos.DrawLine(tempPoint,startPoint);
    }
    public static void DrawLinesConcat<T>(IList<T> _points,Func<T,float3> _convert)
    {
        int count = _points.Count;
        for(int i=0;i<count;i++)
            Gizmos.DrawLine(_convert(_points[i]),_convert(_points[(i+1)%count]));
    }
    
    public static void DrawLinesConcat<T>(IEnumerable<T> _points,Func<T,float3> _convert)
    {
        float3 tempPoint=default;
        float3 firstPoint = default;
        foreach (var (index,value) in _points.LoopIndex())
        {
            var point = _convert(value);
            if (index == 0)
            {
                tempPoint = point;
                firstPoint = point;
                continue;
            }

            Gizmos.DrawLine(tempPoint,point);
            tempPoint = point;
        }
        Gizmos.DrawLine(tempPoint,firstPoint);
    }
    
    public static GUIStyle kLabelStyle => new GUIStyle(GUI.skin.label) { alignment = TextAnchor.LowerCenter,fontSize=12, fontStyle = FontStyle.Normal};
    public static void DrawString(Vector3 _position,string _text,float _offset=.1f)
    {
        Handles.matrix = Gizmos.matrix;
        Handles.Label(_position+_offset*Vector3.up,_text,kLabelStyle);
    }
    
    public static void DrawGizmos(this GBox _box)=>Gizmos.DrawWireCube(_box.center,_box.size);
    public static void DrawGizmos(this GSphere _sphere) => Gizmos.DrawWireSphere(_sphere.center, _sphere.radius);
    public static void DrawGizmos(this GFrustumPoints _frustumPoints)
    {
        DrawLinesConcat(_frustumPoints.nearBottomLeft,_frustumPoints.nearBottomRight,_frustumPoints.nearTopRight,_frustumPoints.nearTopLeft);
        DrawLine(_frustumPoints.farBottomLeft,_frustumPoints.nearBottomLeft);
        DrawLine(_frustumPoints.farBottomRight,_frustumPoints.nearBottomRight);
        DrawLine(_frustumPoints.farTopLeft,_frustumPoints.nearTopLeft);
        DrawLine(_frustumPoints.farTopRight,_frustumPoints.nearTopRight);
        DrawLinesConcat(_frustumPoints.farBottomLeft,_frustumPoints.farBottomRight,_frustumPoints.farTopRight,_frustumPoints.farTopLeft);
    }
}

#endif