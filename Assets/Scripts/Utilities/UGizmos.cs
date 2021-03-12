using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public static class Gizmos_Extend
{
    public static void DrawWireCapsule(Vector3 _pos, Quaternion _rot, Vector3 _scale, float _radius, float _height)
    {
        using (new Handles.DrawingScope(Gizmos.color, Matrix4x4.TRS(_pos, _rot, _scale)))
        {
            if (_height > _radius * 2)
            {
                Vector3 offsetPoint = Vector3.up * (_height - (_radius * 2)) / 2;

                Handles.DrawWireArc(offsetPoint, Vector3.forward, Vector3.right, 180, _radius);
                Handles.DrawWireArc(offsetPoint, Vector3.right, Vector3.forward, -180, _radius);
                Handles.DrawWireArc(-offsetPoint, Vector3.forward, Vector3.right, -180, _radius);
                Handles.DrawWireArc(-offsetPoint, Vector3.right, Vector3.forward, 180, _radius);

                Handles.DrawWireDisc(offsetPoint, Vector3.up, _radius);
                Handles.DrawWireDisc(-offsetPoint, Vector3.up, _radius);

                Handles.DrawLine(offsetPoint + Vector3.left * _radius, -offsetPoint + Vector3.left * _radius);
                Handles.DrawLine(offsetPoint - Vector3.left * _radius, -offsetPoint - Vector3.left * _radius);
                Handles.DrawLine(offsetPoint + Vector3.forward * _radius, -offsetPoint + Vector3.forward * _radius);
                Handles.DrawLine(offsetPoint - Vector3.forward * _radius, -offsetPoint - Vector3.forward * _radius);
            }
            else
            {
                Handles.DrawWireDisc(Vector3.zero, Vector3.up, _radius);
                Handles.DrawWireDisc(Vector3.zero, Vector3.right, _radius);
                Handles.DrawWireDisc(Vector3.zero, Vector3.forward, _radius);
            }
        }
    }

    public static void DrawWireCube(Vector3 _pos, Quaternion _rot, Vector3 _cubeSize)
    {
        using (new Handles.DrawingScope(Gizmos.color, Matrix4x4.TRS(_pos, _rot, Handles.matrix.lossyScale)))
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
    public static void DrawArrow(Vector3 _pos, Quaternion _rot, float _length, float _radius)
    {
        using (new Handles.DrawingScope(Gizmos.color, Gizmos.matrix * Matrix4x4.TRS(_pos, _rot, Handles.matrix.lossyScale)))
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
    public static void DrawCylinder(Vector3 _pos, Quaternion _rot, float _radius, float _height)
    {
        using (new Handles.DrawingScope(Gizmos.color, Matrix4x4.TRS(_pos, _rot, Handles.matrix.lossyScale)))
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
        using (new Handles.DrawingScope(Gizmos.color, Matrix4x4.TRS(_pos, _rot, Handles.matrix.lossyScale)))
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