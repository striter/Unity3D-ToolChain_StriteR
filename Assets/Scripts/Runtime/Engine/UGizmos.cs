#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public static class UGizmos
{
    public static void DrawWireCapsule(Vector3 _pos, Quaternion _rot, Vector3 _scale, float _radius, float _height)
    {
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        Handles_Extend.DrawWireCapsule(_pos, _rot, _scale, _radius, _height);
    }

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
    public static void DrawCylinder(Vector3 _pos, Quaternion _rot, float _radius, float _height)
    {
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        Handles_Extend.DrawCylinder(_pos, _rot, _radius, _height);
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
        int count = _points.Count;
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
    
    public static void DrawGizmos(this GHeightCone _cone)
    {
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        Handles_Extend.DrawCone(_cone);
    }
    
    public static void DrawGizmos(this GLine _line)
    {
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        Handles_Extend.DrawLine(_line);
    }

    public static void DrawGizmos(this Qube<Vector3> _qube)
    {
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        DrawLine(_qube.vDB,_qube.vDL);
        DrawLine(_qube.vDL,_qube.vDF);
        DrawLine(_qube.vDF,_qube.vDR);
        DrawLine(_qube.vDR,_qube.vDB);
        
        DrawLine(_qube.vTB,_qube.vTL);
        DrawLine(_qube.vTL,_qube.vTF);
        DrawLine(_qube.vTF,_qube.vTR);
        DrawLine(_qube.vTR,_qube.vTB);

        DrawLine(_qube.vDB,_qube.vTB);
        DrawLine(_qube.vDL,_qube.vTL);
        DrawLine(_qube.vDF,_qube.vTF);
        DrawLine(_qube.vDR,_qube.vTR);
    }

    public static void DrawGizmos(this GEllipsoid _ellipsoid)
    {
        //Dude
        Matrix4x4 preMatrix = Gizmos.matrix;
        Gizmos.matrix = preMatrix * Matrix4x4.TRS(_ellipsoid.center,Quaternion.identity, _ellipsoid.radius);
        Gizmos.DrawWireSphere(Vector3.zero,1f);
        Gizmos.matrix = preMatrix;
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

    public static void DrawGizmos(this GPolygon _polygon) => DrawLinesConcat(_polygon.positions);
    
    #region 2D
    public static void DrawGizmos(this G2Box _cube) => Gizmos.DrawWireCube(_cube.center.to3xz(),_cube.size.to3xz());
    public static void DrawGizmos(this G2Polygon _polygon) => DrawLinesConcat(_polygon.positions.Select(p=>p.to3xz()));
    public static void DrawGizmos(this G2Quad _quad) => DrawLinesConcat(_quad.Select(p=>p.to3xz()));
    public static void DrawGizmos(this G2Circle _circle)
    {
        Handles.matrix = Gizmos.matrix;
        Handles.color = Gizmos.color;
        Handles.DrawWireDisc(_circle.center.to3xz(), Vector3.up, _circle.radius);
    }

    public static void DrawGizmos(this G2Plane _plane,float _length = 5f)
    {
        var direction = umath.cross(_plane.normal);
        Gizmos.DrawLine((_plane.position + direction * _length).to3xz(),( _plane.position - direction*_length).to3xz() );
        DrawArrow(_plane.position.to3xz(),_plane.normal.to3xz(),1f,.1f);
    }

    public static void DrawGizmos(this G2Triangle _triangle)=>DrawLinesConcat(_triangle.Select(p=>p.to3xz()));
    
    #endregion
}

#endif