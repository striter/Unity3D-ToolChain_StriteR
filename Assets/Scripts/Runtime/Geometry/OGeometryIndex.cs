using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Geometry.Three;
namespace Geometry.Index
{
    [Serializable]
    public struct GMeshQuad
    {
        public int index0 => indices[0];
        public int index1 => indices[1];
        public int index2 => indices[2];
        public int index3 => indices[3];
        public int[] indices;
        public GMeshQuad(int _index0, int _index1, int _index2,int _index3) { indices = new int[4] { _index0, _index1, _index2 ,_index3}; }

        public Vector3[] GetVertices(Vector3[] container) => new[] {  container[index0], container[index1], container[index2],container[index3]};
        public Vector3[] GetVertices(List<Vector3> container) => new[] {  container[index0], container[index1], container[index2],container[index3]};
        public Vector3[] GetVertices<T>(List<T> container, Func<T, Vector3> _getVertex) => new[] { _getVertex( container[index0]), _getVertex(container[index1]),_getVertex( container[index2]),_getVertex( container[index3])};
        
        public GQuad GetQuad(List<Vector3> _vertices) => new GQuad(_vertices[index0],_vertices[index1],_vertices[index2],_vertices[index3]);
        public GQuad GetQuad(Vector3[] _vertices) => new GQuad(_vertices[index0],_vertices[index1],_vertices[index2],_vertices[index3]);
    }
    [Serializable]
    public struct GMeshTriangle
    {
        public int index0 => indices[0];
        public int index1 => indices[1];
        public int index2 => indices[2];
        public int[] indices;
        public GMeshTriangle(int _index0, int _index1, int _index2) { indices = new int[3] { _index0, _index1, _index2 }; }

        public Vector3[] GetVertices(Vector3[] container) => new[] {  container[index0], container[index1], container[index2]};
        public Vector3[] GetVertices(List<Vector3> container) => new[] {  container[index0], container[index1], container[index2]};
        public Vector3[] GetVertices<T>(List<T> container, Func<T, Vector3> _getVertex) => new[] { _getVertex( container[index0]), _getVertex(container[index1]),_getVertex( container[index2])};
        public GTriangle GetTriangle(List<Vector3> _vertices) => new GTriangle(_vertices[index0],_vertices[index1],_vertices[index2]);
        public GTriangle GetTriangle(Vector3[] _vertices) => new GTriangle(_vertices[index0],_vertices[index1],_vertices[index2]);
    }
}