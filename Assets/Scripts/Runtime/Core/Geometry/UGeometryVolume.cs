using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TPoolStatic;
using UnityEngine;

namespace Geometry.Voxel
{
    public static class UGeometryVolume
    {
        public static bool IsPointInside(this GBox _box, Vector3 _point)=> 
            _point.x >= _box.min.x && _point.x <= _box.max.x && 
            _point.y >= _box.min.y && _point.y <= _box.max.y && 
            _point.z >= _box.min.z && _point.z <= _box.max.z;
        public static Qube<Vector3> ExpandToQube<T>(this T _quad, Vector3 _expand, float _baryCenter = 0) where T : IQuad<Vector3>
        {
            var expand = _expand * (1 - _baryCenter);
            var shrink = _expand * _baryCenter;

            return new Qube<Vector3>(_quad.B - shrink, _quad.L - shrink, _quad.F - shrink, _quad.R - shrink,
                             _quad.B + expand, _quad.L + expand, _quad.F + expand, _quad.R + expand);
        }
        public static IEnumerable<Qube<Vector3>> SplitToQubes(this Quad<Vector3> _quad, Vector3 _halfSize, bool insideOut)
        {
            var quads = _quad.SplitToQuads(insideOut).ToArray();
            foreach (var quad in quads)
                yield return new Quad<Vector3>(quad.vB, quad.vL, quad.vF, quad.vR).ExpandToQube(_halfSize, 1f);
            foreach (var quad in quads)
                yield return new Quad<Vector3>(quad.vB, quad.vL, quad.vF, quad.vR).ExpandToQube(_halfSize, 0f);
        }

        public static Quad<T> GetQuad<T>(this CubeFacing<T> _facing) => new Quad<T>(_facing.fBL, _facing.fLF, _facing.fFR, _facing.fRB);

        public static void FillFacingQuadTriangle(this Qube<Vector3> _qube, ECubeFacing _facing, List<Vector3> _vertices, List<int> _indices,
            List<Vector2> _uvs, List<Vector3> _normals, List<Color> _colors = null, Color _color = default)
        {
            new GQuad(_qube.GetFacingCornersCW(_facing)).FillQuadTriangle(_vertices, _indices, _uvs, _normals, _colors, _color);
        }
        public static void FillFacingSplitQuadTriangle(this Qube<Vector3> _qube, ECubeFacing _facing, List<Vector3> _vertices, List<int> _indices,
            List<Vector2> _uvs, List<Vector3> _normals, List<Color> _colors = null, Color _color = default)
        {
            foreach (var quad in new GQuad(_qube.GetFacingCornersCW(_facing)).SplitToQuads(true))
                new GQuad(quad.B, quad.L, quad.F, quad.R).FillQuadTriangle(_vertices, _indices, _uvs, _normals, _colors, _color);
        }
        public static void FillTopDownQuadTriangle(this Qube<Vector3> _qube, ECubeFacing _facing, List<Vector3> _vertices, List<int> _indices,
            List<Vector2> _uvs, List<Vector3> _normals, List<Color> _colors = null, Color _color = default)
        {
            foreach (var quad in new GQuad(_qube.GetFacingCornersCW(_facing)).SplitTopDownQuads())
                new GQuad(quad.B, quad.L, quad.F, quad.R).FillQuadTriangle(_vertices, _indices, _uvs, _normals, _colors, _color);
        }

        public static Matrix4x4 GetMirrorMatrix(this GPlane _plane)
        {
            Matrix4x4 mirrorMatrix = Matrix4x4.identity;
            mirrorMatrix.m00 = 1 - 2 * _plane.normal.x * _plane.normal.x;
            mirrorMatrix.m01 = -2 * _plane.normal.x * _plane.normal.y;
            mirrorMatrix.m02 = -2 * _plane.normal.x * _plane.normal.z;
            mirrorMatrix.m03 = 2 * _plane.normal.x * _plane.distance;
            mirrorMatrix.m10 = -2 * _plane.normal.x * _plane.normal.y;
            mirrorMatrix.m11 = 1 - 2 * _plane.normal.y * _plane.normal.y;
            mirrorMatrix.m12 = -2 * _plane.normal.y * _plane.normal.z;
            mirrorMatrix.m13 = 2 * _plane.normal.y * _plane.distance;
            mirrorMatrix.m20 = -2 * _plane.normal.x * _plane.normal.z;
            mirrorMatrix.m21 = -2 * _plane.normal.y * _plane.normal.z;
            mirrorMatrix.m22 = 1 - 2 * _plane.normal.z * _plane.normal.z;
            mirrorMatrix.m23 = 2 * _plane.normal.z * _plane.distance;
            mirrorMatrix.m30 = 0;
            mirrorMatrix.m31 = 0;
            mirrorMatrix.m32 = 0;
            mirrorMatrix.m33 = 1;
            return mirrorMatrix;
        }

        public static Bounds ToBounds(this GBox _box)
        {
            return new Bounds(_box.center, _box.size);
        }
    }
}
