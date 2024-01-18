using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Runtime.Geometry
{
    using static UGizmos;

    [Flags]
    public enum EDrawMeshFlag
    {
        Vertices,
        Triangles,
        // Edges,
    }
    public static class Gizmos_Geometry
    {
        public static void DrawGizmos(this GBox _box)
        {
            Gizmos.DrawWireCube(_box.center,_box.size);
        }
        public static void DrawGizmos(this GSphere _sphere)
        {
            Gizmos.DrawWireSphere(_sphere.center,_sphere.radius);
        }
        public static void DrawGizmos(this GCone _cone)
        {
            Handles.color = Gizmos.color;
            Handles.matrix = Gizmos.matrix;
            Handles_Extend.DrawCone(_cone);
        }
        
        public static void DrawGizmos(this GCapsule _capsule)
        {
            Handles.color = Gizmos.color;
            Handles.matrix = Gizmos.matrix;
            Handles_Extend.DrawWireCapsule(_capsule);
        }

        public static void DrawGizmos(this GDisk _disk)
        {
            Handles.color = Gizmos.color;
            Handles.matrix = Gizmos.matrix;
            Handles.DrawWireDisc(_disk.Center,_disk.normal,_disk.radius);
        }

        public static void DrawGizmos(this GLine _line)
        {
            Handles.color = Gizmos.color;
            Handles.matrix = Gizmos.matrix;
            Handles_Extend.DrawLine(_line);
        }

        public static void DrawGizmos(this GCylinder _cylinder)
        {
            Handles.color = Gizmos.color;
            Handles.matrix = Gizmos.matrix;
            Handles_Extend.DrawCylinder(_cylinder.origin,_cylinder.normal, _cylinder.radius, _cylinder.height);
        }
        
        public static void DrawGizmos(this GQube _qube)
        {
            var qube = _qube.qube;
            Handles.color = Gizmos.color;
            Handles.matrix = Gizmos.matrix;
            DrawLine(qube.vDB,qube.vDL);
            DrawLine(qube.vDL,qube.vDF);
            DrawLine(qube.vDF,qube.vDR);
            DrawLine(qube.vDR,qube.vDB);
            
            DrawLine(qube.vTB,qube.vTL);
            DrawLine(qube.vTL,qube.vTF);
            DrawLine(qube.vTF,qube.vTR);
            DrawLine(qube.vTR,qube.vTB);

            DrawLine(qube.vDB,qube.vTB);
            DrawLine(qube.vDL,qube.vTL);
            DrawLine(qube.vDF,qube.vTF);
            DrawLine(qube.vDR,qube.vTR);
        }

        public static void DrawGizmos(this GEllipsoid _ellipsoid)
        {
            //Dude
            Matrix4x4 preMatrix = Gizmos.matrix;
            Gizmos.matrix = preMatrix * Matrix4x4.TRS(_ellipsoid.center,Quaternion.identity, _ellipsoid.radius);
            Gizmos.DrawWireSphere(Vector3.zero,1f);
            Gizmos.matrix = preMatrix;
        }

        public static void DrawGizmos(this GPolygon _polygon) => DrawLinesConcat(_polygon.positions);
        public static void DrawGizmos(this GQuad _quad) => DrawLinesConcat(_quad.quad);
        
        public static void DrawGizmos(this GPlane _plane,float _radius = 5f)
        {
            Handles.matrix = Gizmos.matrix;
            Handles.color = Gizmos.color;
            Handles.DrawWireDisc(_plane.position, _plane.normal, _radius);
            DrawArrow(_plane.position,_plane.normal,.5f,.1f);
        }
        public static void DrawGizmos(this GTriangle _triangle)=>DrawLinesConcat(_triangle.triangle);
        
        public static void DrawGizmos(this GPlane _plane)=>_plane.DrawGizmos(5f);

        public static void DrawGizmos(this GMesh _mesh,EDrawMeshFlag _flag)
        {
            if(_flag.IsFlagEnable(EDrawMeshFlag.Vertices))
            {
                foreach (var vertex in _mesh.vertices)
                    Gizmos.DrawWireSphere(vertex,.01f);
            }
            
            if (_flag.IsFlagEnable(EDrawMeshFlag.Triangles))
            {
                for (int i = 0; i < _mesh.triangles.Length; i += 3)
                    DrawGizmos(new GTriangle(_mesh.vertices[_mesh.triangles[i]],_mesh.vertices[_mesh.triangles[i + 1]],_mesh.vertices[_mesh.triangles[i + 2]]));
            }
        }

        public static void DrawGizmos(this GMesh _mesh) => _mesh.DrawGizmos(EDrawMeshFlag.Triangles);

        public static void DrawGizmos(this GPointSets _points)
        {
            foreach (var vertex in _points.vertices)
                Gizmos.DrawSphere(vertex,.01f);
            // DrawLinesConcat(_points.vertices);
        }
        
        public static void DrawGizmos(this G2Box _cube) => Gizmos.DrawWireCube(_cube.center.to3xz(),_cube.size.to3xz());

        public static void DrawGizmos(this G2Polygon _polygon)
        {
            if (_polygon.positions == null || _polygon.positions.Length == 0)
                return;
            DrawLinesConcat(_polygon.positions.Select(p=>p.to3xz()));
        } public static void DrawGizmos(this G2Triangle _triangle)=>DrawLinesConcat(_triangle.Select(p=>p.to3xz()));
        public static void DrawGizmos(this G2Quad _quad) => DrawLinesConcat(_quad.Select(p=>p.to3xz()));
        public static void DrawGizmos(this G2Circle _circle)
        {
            Handles.matrix = Gizmos.matrix;
            Handles.color = Gizmos.color;
            Handles.DrawWireDisc(_circle.center.to3xz(), Vector3.up, _circle.radius);
        }

        public static void DrawGizmos(this G2Plane _plane,float _radius = 5f)
        {
            var direction = umath.cross(_plane.normal);
            Gizmos.DrawLine((_plane.position + direction * _radius).to3xz(),( _plane.position - direction*_radius).to3xz() );
            DrawArrow(_plane.position.to3xz(),_plane.normal.to3xz(),1f,.1f);
        }

        
    }
}