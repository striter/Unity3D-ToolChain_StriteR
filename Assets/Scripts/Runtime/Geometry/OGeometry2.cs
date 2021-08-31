using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry.Two
{
    [Serializable]
    public struct G2Triangle
    {
        public Vector2 vertex0=>vertices[0];
        public Vector2 vertex1=>vertices[1];
        public Vector2 vertex2=>vertices[2];
        public Vector2 uOffset { get; private set; }
        public Vector2 vOffset { get; private set; }
        public Vector2[] vertices { get; private set; }
        public Vector2[] GetDrawLinesVertices() => new[] { vertex0, vertex1, vertex2, vertex0 };
        public Vector2 this[int index]
        {
            get
            {
                switch (index)
                {
                    default: Debug.LogError("Invalid Index:" + index); return vertex0;
                    case 0: return vertex0;
                    case 1: return vertex1;
                    case 2: return vertex2;
                }
            }
        }
        public G2Triangle(Vector2 _vertex1, Vector2 _vertex2, Vector2 _vertex3) : this(new[] { _vertex1, _vertex2, _vertex3 }) { }
        public G2Triangle(Vector2[] _vertices)
        {
            Debug.Assert(_vertices.Length == 3, "Triangles' Vertices Count Must Equals 3!");
            vertices = _vertices;
            uOffset = _vertices[1] - _vertices[0];
            vOffset = _vertices[2] - _vertices[0];
        }

        public Vector2 GetUVPoint(float u,float v)=>(1f - u - v) * vertex0 + u * uOffset + v * vOffset;
    }

    [Serializable]
    public struct G2Quad
    {
        public Vector2 vertex1 => vertices[0];
        public Vector2 vertex2=> vertices[1];
        public Vector2 vertex3=> vertices[2];
        public Vector2 vertex4=> vertices[3];
        public Vector2[] vertices { get; private set; }

        public Vector2 center => vertices.Average();

        public Vector2 this[int index]
        {
            get
            {
                switch (index)
                {
                    default: Debug.LogError("Invalid Index:" + index); return vertex1;
                    case 0: return vertex1;
                    case 1: return vertex2;
                    case 2: return vertex3;
                    case 3: return vertex4;
                }
            }
        }
        public G2Quad(Vector2 _vertex1, Vector2 _vertex2, Vector2 _vertex3,Vector2 _vertex4) : this(new[] { _vertex1, _vertex2, _vertex3,_vertex4 }) { }
        public G2Quad(Vector2[] _vertices)
        {
            Debug.Assert(_vertices.Length == 4, "Quads' Vertices Count Must Equals 4!");
            vertices = _vertices;
        }
    }

}