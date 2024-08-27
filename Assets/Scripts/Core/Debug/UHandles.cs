using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEngine;

#if UNITY_EDITOR
namespace UnityEditor
{
    public static class UHandles
    {
        public static GUIStyle kLabelStyle => new GUIStyle(GUI.skin.label) { alignment = TextAnchor.LowerCenter,fontSize=12, fontStyle = FontStyle.Normal};
        public static void DrawString(Vector3 _position,string _text, float _offset = 0.1f) => DrawString(_text,_position,_offset);
        public static void DrawString(string _text, Vector3 _position = default, float _offset = 0.1f)
        {
#if UNITY_EDITOR
            Handles.Label(_position+_offset*Vector3.up,_text,kLabelStyle);
#endif
        }
        public static void DrawCone(Vector3 _origin,Vector3 _normal,float _radius,float _height)
        {
            using(new Handles.DrawingScope(Handles.color, Handles.matrix*Matrix4x4.TRS(_origin, Quaternion.LookRotation(_normal),Vector3.one )))
            {
                var bottom =  Vector3.forward * _height;
                var bottomForwardDir =  Vector3.up *_radius;
                var bottomForward = bottom +bottomForwardDir;
                var bottomBack = bottom - bottomForwardDir;
                var bottomRightDir =Vector3.right * _radius;
                var bottomRight = bottom + bottomRightDir;
                var bottomLeft = bottom - bottomRightDir;

                Handles.DrawWireArc(bottom,-Vector3.forward, Vector3.right, 360,_radius);
                Handles.DrawLine(Vector3.zero, bottomForward);
                Handles.DrawLine(Vector3.zero, bottomBack);
                Handles.DrawLine(Vector3.zero, bottomRight);
                Handles.DrawLine(Vector3.zero, bottomLeft);
            }
        }

        public static void DrawLinesConcat(Vector3[] _lines) => DrawLinesConcat(_lines.Select(p => (float3)p));
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

                Handles.DrawLine(tempPoint,point);
                tempPoint = point;
            }
            Handles.DrawLine(tempPoint,startPoint);
        }
        public static void DrawWireCapsule(Vector3 _pos, Quaternion _rot, Vector3 _scale, float _radius, float _height)
        {
            using (new Handles.DrawingScope(Handles.color, Handles.matrix * Matrix4x4.TRS(_pos, _rot, _scale)))
            {
                var forward = Vector3.forward;
                var up = Vector3.up;
                var right = Vector3.right;
                var left = -right;
                Vector3 offsetPoint = forward * _height / 2;

                Handles.DrawWireArc(offsetPoint, -up, right, 180, _radius);
                Handles.DrawWireArc(offsetPoint, right, -up, -180, _radius);
                Handles.DrawWireArc(-offsetPoint, -up, right, -180, _radius);
                Handles.DrawWireArc(-offsetPoint, right,- up, 180, _radius);

                Handles.DrawWireDisc(offsetPoint, forward, _radius);
                Handles.DrawWireDisc(-offsetPoint, forward, _radius);

                Handles.DrawLine(offsetPoint + left * _radius, -offsetPoint + left * _radius);
                Handles.DrawLine(offsetPoint - left * _radius, -offsetPoint - left * _radius);
                Handles.DrawLine(offsetPoint + up * _radius, -offsetPoint + up * _radius);
                Handles.DrawLine(offsetPoint - up * _radius, -offsetPoint - up * _radius);
            }
        }
        public static void DrawWireSphere(Vector3 _pos, float _radius) => DrawWireSphere(_pos,kfloat3.forward,_radius);
        public static void DrawWireSphere(Vector3 _pos, Vector3 _dir, float _radius) => DrawWireSphere(_pos,Quaternion.LookRotation(_dir),_radius);
        public static void DrawWireSphere(Vector3 _pos,Quaternion _rot,  float _radius)
        {
            using (new Handles.DrawingScope(Handles.color, Handles.matrix* Matrix4x4.TRS(_pos, _rot, Vector3.one)))
            {
                Handles.DrawWireDisc(Vector3.zero, Vector3.up, _radius);
                Handles.DrawWireDisc(Vector3.zero, Vector3.right, _radius);
                Handles.DrawWireDisc(Vector3.zero, Vector3.forward, _radius);
            }
        }
        public static void DrawArrow(Vector3 _pos, Vector3 _direction, float _length, float _radius) => DrawArrow(_pos, Quaternion.LookRotation(_direction), _length, _radius);
        public static void DrawArrow(Vector3 _pos, Quaternion _rot, float _length, float _radius)
        {
            using (new Handles.DrawingScope(Handles.color, Handles.matrix * Matrix4x4.TRS(_pos, _rot, Handles.matrix.lossyScale)))
            {
                Vector3 capBottom = Vector3.forward * _length / 2;
                Vector3 capTop = Vector3.forward * _length;
                float rootRadius = _radius / 4;
                float capBottomSize = _radius / 2;
                Handles.DrawWireDisc(Vector3.zero, Vector3.forward, rootRadius);
                Handles.DrawWireDisc(capBottom, Vector3.forward, rootRadius);
                Handles.DrawLine(Vector3.up * rootRadius, capBottom + Vector3.up * rootRadius);
                Handles.DrawLine(-Vector3.up * rootRadius, capBottom - Vector3.up * rootRadius);
                Handles.DrawLine(Vector3.right * rootRadius, capBottom + Vector3.right * rootRadius);
                Handles.DrawLine(-Vector3.right * rootRadius, capBottom - Vector3.right * rootRadius);

                Handles.DrawWireDisc(capBottom, Vector3.forward, capBottomSize);
                Handles.DrawLine(capBottom + Vector3.up * capBottomSize, capTop);
                Handles.DrawLine(capBottom - Vector3.up * capBottomSize, capTop);
                Handles.DrawLine(capBottom + Vector3.right * capBottomSize, capTop);
                Handles.DrawLine(capBottom + -Vector3.right * capBottomSize, capTop);
            }
        }
        public static void DrawCylinder(Vector3 _origin,Vector3 _up, float _radius, float _height)
        {
            using (new Handles.DrawingScope(Handles.color, Handles.matrix * Matrix4x4.TRS(_origin, Quaternion.LookRotation(_up), Vector3.one)))
            {
                Vector3 top = Vector3.forward * _height;

                Handles.DrawWireDisc(Vector3.zero, Vector3.forward, _radius);
                Handles.DrawWireDisc(top, Vector3.forward, _radius);

                Handles.DrawLine(Vector3.right * _radius, top + Vector3.right * _radius);
                Handles.DrawLine(-Vector3.right * _radius, top - Vector3.right * _radius);
                Handles.DrawLine(Vector3.up * _radius, top + Vector3.up * _radius);
                Handles.DrawLine(-Vector3.up * _radius, top - Vector3.up * _radius);
            }
        }
        public static void DrawTrapezium(Vector3 _pos, Quaternion _rot, Vector4 trapeziumInfo)
        {
            using (new Handles.DrawingScope(Handles.color, Handles.matrix * Matrix4x4.TRS(_pos, _rot, Vector3.one)))
            {
                Vector3 backLeftUp = -Vector3.right * trapeziumInfo.x / 2 + Vector3.forward * trapeziumInfo.y / 2 - Vector3.up * trapeziumInfo.z / 2;
                Vector3 backLeftDown = -Vector3.right * trapeziumInfo.x / 2 - Vector3.forward * trapeziumInfo.y / 2 - Vector3.up * trapeziumInfo.z / 2;
                Vector3 backRightUp = Vector3.right * trapeziumInfo.x / 2 + Vector3.forward * trapeziumInfo.y / 2 - Vector3.up * trapeziumInfo.z / 2;
                Vector3 backRightDown = Vector3.right * trapeziumInfo.x / 2 - Vector3.forward * trapeziumInfo.y / 2 - Vector3.up * trapeziumInfo.z / 2;

                Vector3 forwardLeftUp = -Vector3.right * trapeziumInfo.w / 2 + Vector3.forward * trapeziumInfo.y / 2 + Vector3.up * trapeziumInfo.z / 2;
                Vector3 forwardLeftDown = -Vector3.right * trapeziumInfo.w / 2 - Vector3.forward * trapeziumInfo.y / 2 + Vector3.up * trapeziumInfo.z / 2;
                Vector3 forwardRightUp = Vector3.right * trapeziumInfo.w / 2 + Vector3.forward * trapeziumInfo.y / 2 + Vector3.up * trapeziumInfo.z / 2;
                Vector3 forwardRightDown = Vector3.right * trapeziumInfo.w / 2 - Vector3.forward * trapeziumInfo.y / 2 + Vector3.up * trapeziumInfo.z / 2;

                Handles.DrawLine(backLeftUp, backLeftDown);
                Handles.DrawLine(backLeftDown, backRightDown);
                Handles.DrawLine(backRightDown, backRightUp);
                Handles.DrawLine(backRightUp, backLeftUp);

                Handles.DrawLine(forwardLeftUp, forwardLeftDown);
                Handles.DrawLine(forwardLeftDown, forwardRightDown);
                Handles.DrawLine(forwardRightDown, forwardRightUp);
                Handles.DrawLine(forwardRightUp, forwardLeftUp);

                Handles.DrawLine(backLeftUp, forwardLeftUp);
                Handles.DrawLine(backLeftDown, forwardLeftDown);
                Handles.DrawLine(backRightUp, forwardRightUp);
                Handles.DrawLine(backRightDown, forwardRightDown);
            }
        }
    }
}
#endif