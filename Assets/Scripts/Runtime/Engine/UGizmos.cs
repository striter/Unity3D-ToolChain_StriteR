#if UNITY_EDITOR
using System.Collections.Generic;
using Geometry.Voxel;
using UnityEditor;
using UnityEngine;

public static class Gizmos_Extend
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

    public static void DrawTrapezium(Vector3 _pos, Quaternion _rot, Vector4 trapeziumInfo)
    {
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        Handles_Extend.DrawTrapezium(_pos, _rot, trapeziumInfo);
    }
    public static void DrawCone(GHeightCone _cone)
    {
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        Handles_Extend.DrawCone(_cone);
    }
    public static void DrawLine(GLine _line)
    {
        Handles.color = Gizmos.color;
        Handles.matrix = Gizmos.matrix;
        Handles_Extend.DrawLine(_line);
    }

    public static void DrawLines(IList<Vector3> _points)
    {
        int count = _points.Count;
        for(int i=0;i<count-1;i++)
            Gizmos.DrawLine(_points[i],_points[(i+1)%count]);
    }
    public static void DrawLinesConcat(IList<Vector3> _points)
    {
        int count = _points.Count;
        for(int i=0;i<count;i++)
            Gizmos.DrawLine(_points[i],_points[(i+1)%count]);
    }

    public static void DrawString(Vector3 positionLS,string text)
    {
        if (SceneView.currentDrawingSceneView == null)
            return;
        Handles.BeginGUI();
        var screenPos=SceneView.currentDrawingSceneView.camera.WorldToScreenPoint(Gizmos.matrix.MultiplyPoint(positionLS));

        screenPos.y = SceneView.currentDrawingSceneView.camera.pixelHeight - screenPos.y-20;
        var size=GUI.skin.label.CalcSize(new GUIContent(text));
        GUI.color = Gizmos.color;
        GUI.Label(new Rect(screenPos,Vector2.zero).Expand(size),text);
        Handles.EndGUI();
    }
}

#endif