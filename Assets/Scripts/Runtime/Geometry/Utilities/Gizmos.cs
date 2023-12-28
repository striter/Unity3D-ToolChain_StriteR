using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Geometry
{
    using static UGizmos;
    public static class Gizmos
    {
        public static void DrawGizmos(this GBox _box)
        {
            UnityEngine.Gizmos.DrawWireCube(_box.center,_box.size);
        }
        public static void DrawGizmos(this GSphere _sphere)
        {
            UnityEngine.Gizmos.DrawWireSphere(_sphere.center,_sphere.radius);
        }
        public static void DrawGizmos(this GCone _cone)
        {
            Handles.color = UnityEngine.Gizmos.color;
            Handles.matrix = UnityEngine.Gizmos.matrix;
            Handles_Extend.DrawCone(_cone);
        }
        
        public static void DrawGizmos(this GCapsule _capsule)
        {
            Handles.color = UnityEngine.Gizmos.color;
            Handles.matrix = UnityEngine.Gizmos.matrix;
            Handles_Extend.DrawWireCapsule(_capsule);
        }

        public static void DrawGizmos(this GDisk _disk)
        {
            Handles.color = UnityEngine.Gizmos.color;
            Handles.matrix = UnityEngine.Gizmos.matrix;
            Handles.DrawWireDisc(_disk.Center,_disk.normal,_disk.radius);
        }

        public static void DrawGizmos(this GLine _line)
        {
            Handles.color = UnityEngine.Gizmos.color;
            Handles.matrix = UnityEngine.Gizmos.matrix;
            Handles_Extend.DrawLine(_line);
        }

        public static void DrawGizmos(this GCylinder _cylinder)
        {
            Handles.color = UnityEngine.Gizmos.color;
            Handles.matrix = UnityEngine.Gizmos.matrix;
            Handles_Extend.DrawCylinder(_cylinder.origin,_cylinder.normal, _cylinder.radius, _cylinder.height);
        }
        
        public static void DrawGizmos(this Qube<Vector3> _qube)
        {
            Handles.color = UnityEngine.Gizmos.color;
            Handles.matrix = UnityEngine.Gizmos.matrix;
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
            Matrix4x4 preMatrix = UnityEngine.Gizmos.matrix;
            UnityEngine.Gizmos.matrix = preMatrix * Matrix4x4.TRS(_ellipsoid.center,Quaternion.identity, _ellipsoid.radius);
            UnityEngine.Gizmos.DrawWireSphere(Vector3.zero,1f);
            UnityEngine.Gizmos.matrix = preMatrix;
        }

        public static void DrawGizmos(this GPolygon _polygon) => DrawLinesConcat(_polygon.positions);
        public static void DrawGizmos(this GQuad _quad) => DrawLinesConcat(_quad.quad);
        
        public static void DrawGizmos(this GPlane _plane,float _radius = 5f)
        {
            Handles.matrix = UnityEngine.Gizmos.matrix;
            Handles.color = UnityEngine.Gizmos.color;
            Handles.DrawWireDisc(_plane.position, _plane.normal, _radius);
            DrawArrow(_plane.position,_plane.normal,.5f,.1f);
        }
        public static void DrawGizmos(this GTriangle _triangle)=>DrawLinesConcat(_triangle.triangle);
        
        public static void DrawGizmos(this GPlane _plane)=>_plane.DrawGizmos(5f);

        
        public static void DrawGizmos(this G2Box _cube) => UnityEngine.Gizmos.DrawWireCube(_cube.center.to3xz(),_cube.size.to3xz());
        public static void DrawGizmos(this G2Polygon _polygon) => DrawLinesConcat(_polygon.positions.Select(p=>p.to3xz()));
        public static void DrawGizmos(this G2Triangle _triangle)=>DrawLinesConcat(_triangle.Select(p=>p.to3xz()));
        public static void DrawGizmos(this G2Quad _quad) => DrawLinesConcat(_quad.Select(p=>p.to3xz()));
        public static void DrawGizmos(this G2Circle _circle)
        {
            Handles.matrix = UnityEngine.Gizmos.matrix;
            Handles.color = UnityEngine.Gizmos.color;
            Handles.DrawWireDisc(_circle.center.to3xz(), Vector3.up, _circle.radius);
        }

        public static void DrawGizmos(this G2Plane _plane,float _radius = 5f)
        {
            var direction = umath.cross(_plane.normal);
            UnityEngine.Gizmos.DrawLine((_plane.position + direction * _radius).to3xz(),( _plane.position - direction*_radius).to3xz() );
            DrawArrow(_plane.position.to3xz(),_plane.normal.to3xz(),1f,.1f);
        }
    }
}