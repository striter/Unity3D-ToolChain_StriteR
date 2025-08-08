
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;
using Gizmos = UnityEngine.Gizmos;

public static class UGizmos
{

    public static void DrawLine(Vector3 _src, Vector3 _dest, float _normalizedLength=1f)
    {
        Gizmos.DrawLine(_src,(_src+(_dest-_src)*_normalizedLength));
    }

    public static void DrawLines(params Vector3[] _positions) => DrawLines((IList<Vector3>)_positions);
    public static void DrawLines(IList<Vector3> _points)
    {
        var count = _points.Count;
        for(int i=0;i<count-1;i++)
            Gizmos.DrawLine(_points[i],_points[i+1]);
    }
    
    public static void DrawLines(IEnumerable<float3> _points)
    {
        Vector3 tempPoint=default;
        foreach (var (index,point) in _points.WithIndex())
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

    public static void DrawLines(IEnumerable<Vector3> _points)
    {
        Vector3 tempPoint=default;
        foreach (var (index,point) in _points.WithIndex())
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
        foreach (var (index,value) in _points.WithIndex())
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
        foreach (var (index,point) in _points.WithIndex())
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
        foreach (var (index,value) in _points.WithIndex())
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
    
    public static void DrawString(string _text, Vector3 _position = default, float _offset = 0.1f)
    {
#if UNITY_EDITOR
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        UHandles.DrawString(_text,_position,_offset);
#endif
    }
    public static void DrawString(Vector3 _position,string _text, float _offset = 0.1f) => DrawString(_text,_position,_offset);
    public static void DrawArrow(Vector3 _pos, Vector3 _direction, float _length, float _radius) => DrawArrow(_pos, Quaternion.LookRotation(_direction), _length, _radius);
    public static void DrawArrow(Vector3 _pos, Quaternion _rot, float _length, float _radius)
    {
#if UNITY_EDITOR
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        UHandles.DrawArrow(_pos, _rot, _length, _radius);
#endif
    }
    public static void DrawCylinder(Vector3 _pos, Vector3 _up, float _radius, float _height)
    {
#if UNITY_EDITOR
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        UHandles.DrawCylinder(_pos, _up, _radius, _height);
#endif
    }

    public static void DrawTrapezium(Vector3 _pos, Quaternion _rot, Vector4 _trapeziumInfo)
    {
#if UNITY_EDITOR
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        UHandles.DrawTrapezium(_pos, _rot, _trapeziumInfo);
#endif
    }

    public static void DrawWireDisk(Vector3 _pos, Vector3 _normal, float _radius)
    {
#if UNITY_EDITOR
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        Handles.DrawWireDisc(_pos, _normal, _radius);
#endif
    }

    public static void DrawCone(Vector3 _origin,Vector3 _normal,float _radius,float _height)
    {
#if UNITY_EDITOR
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        UHandles.DrawCone(_origin, _normal, _radius, _height);
#endif
    }
    
    public static void DrawWireCapsule(Vector3 _origin,Vector3 _normal,float _radius,float _height)
    {
#if UNITY_EDITOR
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        UHandles.DrawWireCapsule(_origin,_normal, _radius, _height);
#endif
    }

    public static void DrawSolidTriangle(Vector3 _pos1, Vector3 _pos2, Vector3 _pos3)
    {
#if UNITY_EDITOR
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        Handles.DrawAAConvexPolygon(_pos1, _pos2, _pos3);
#endif
    }
    
    public static void DrawEllipsoid(Vector3 center, float3 radius)
    {
        var preMatrix = Gizmos.matrix;
        Gizmos.matrix = preMatrix * Matrix4x4.TRS(center,Quaternion.identity, radius);
        Gizmos.DrawWireSphere(Vector3.zero,1f);
        Gizmos.matrix = preMatrix;
    }
}
