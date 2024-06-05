using UnityEditor;
using UnityEngine;
using Runtime.Geometry;

#if UNITY_EDITOR
public static class Handles_Extend
{
    public static void DrawCone(GCone _heightCone)
    {
        using(new Handles.DrawingScope(Handles.color, Handles.matrix*Matrix4x4.TRS(_heightCone.origin, Quaternion.LookRotation(_heightCone.normal),Vector3.one )))
        {
            float radius = _heightCone.Radius;
            Vector3 bottom =  Vector3.forward * _heightCone.height;
            Vector3 bottomForwardDir =  Vector3.up *radius;
            Vector3 bottomForward = bottom +bottomForwardDir;
            Vector3 bottomBack = bottom - bottomForwardDir;
            Vector3 bottomRightDir =Vector3.right * radius;
            Vector3 bottomRight = bottom + bottomRightDir;
            Vector3 bottomLeft = bottom - bottomRightDir;

            Handles.DrawWireArc(bottom,-Vector3.forward, Vector3.right, 360,radius);
            Handles.DrawLine(Vector3.zero, bottomForward);
            Handles.DrawLine(Vector3.zero, bottomBack);
            Handles.DrawLine(Vector3.zero, bottomRight);
            Handles.DrawLine(Vector3.zero, bottomLeft);
        }
    }
    
    public static void DrawLine(GLine _line)
    {
        Quaternion rotation = Quaternion.LookRotation(_line.direction);
        DrawArrow(_line.start, rotation, .5f,.05f);
        using(new Handles.DrawingScope(Handles.color,Handles.matrix*Matrix4x4.TRS(_line.start,rotation,Vector3.one)))
        {
            Handles.DrawLine(Vector3.zero,Vector3.forward*_line.length);
        }
    }
    public static void DrawLines_Concat(Vector3[] _lines)
    {
        int length = _lines.Length;
        for (int i = 0; i < length; i++)
            Handles.DrawLine(_lines[i],_lines[(i+1)%length]);
    }

    public static void DrawWireCapsule(GCapsule _capsule) => DrawWireCapsule(_capsule.origin,Quaternion.LookRotation(_capsule.normal),Vector3.one,_capsule.radius,_capsule.height);
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
    public static void DrawWireCube(Vector3 _pos, Quaternion _rot, Vector3 _cubeSize)
    {
        using (new Handles.DrawingScope(Handles.color, Handles.matrix * Matrix4x4.TRS(_pos, _rot, Handles.matrix.lossyScale)))
        {
            float halfWidth, halfHeight, halfLength;
            halfWidth = _cubeSize.x / 2;
            halfHeight = _cubeSize.y / 2;
            halfLength = _cubeSize.z / 2;

            Handles.DrawLine(new Vector3(halfWidth, halfHeight, halfLength), new Vector3(-halfWidth, halfHeight, halfLength));
            Handles.DrawLine(new Vector3(halfWidth, halfHeight, -halfLength), new Vector3(-halfWidth, halfHeight, -halfLength));
            Handles.DrawLine(new Vector3(halfWidth, -halfHeight, halfLength), new Vector3(-halfWidth, -halfHeight, halfLength));
            Handles.DrawLine(new Vector3(halfWidth, -halfHeight, -halfLength), new Vector3(-halfWidth, -halfHeight, -halfLength));

            Handles.DrawLine(new Vector3(halfWidth, halfHeight, halfLength), new Vector3(halfWidth, -halfHeight, halfLength));
            Handles.DrawLine(new Vector3(-halfWidth, halfHeight, halfLength), new Vector3(-halfWidth, -halfHeight, halfLength));
            Handles.DrawLine(new Vector3(halfWidth, halfHeight, -halfLength), new Vector3(halfWidth, -halfHeight, -halfLength));
            Handles.DrawLine(new Vector3(-halfWidth, halfHeight, -halfLength), new Vector3(-halfWidth, -halfHeight, -halfLength));

            Handles.DrawLine(new Vector3(halfWidth, halfHeight, halfLength), new Vector3(halfWidth, halfHeight, -halfLength));
            Handles.DrawLine(new Vector3(-halfWidth, halfHeight, halfLength), new Vector3(-halfWidth, halfHeight, -halfLength));
            Handles.DrawLine(new Vector3(halfWidth, -halfHeight, halfLength), new Vector3(halfWidth, -halfHeight, -halfLength));
            Handles.DrawLine(new Vector3(-halfWidth, -halfHeight, halfLength), new Vector3(-halfWidth, -halfHeight, -halfLength));
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
#endif