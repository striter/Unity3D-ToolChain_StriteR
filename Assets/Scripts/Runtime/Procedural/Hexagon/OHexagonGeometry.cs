using System;
using UnityEngine;

namespace Procedural.Hexagon.Geometry
{
    [Serializable]
    public struct HexTriangle
    {
        public PHexCube vertex0;
        public PHexCube vertex1;
        public PHexCube vertex2;
        public readonly PHexCube[] vertices;
        public PHexCube this[int index] => vertices[index];
        public HexTriangle(PHexCube _vertex0,PHexCube _vertex1,PHexCube _vertex2)
        {
            vertex0 = _vertex0;
            vertex1 = _vertex1;
            vertex2 = _vertex2;
            vertices = new[] {vertex0, vertex1, vertex2};
        }

        public T[] GetVertices<T>(Func<PHexCube, T> toWorld) =>
            new[] {toWorld(vertex0), toWorld(vertex1), toWorld(vertex2)};
    }

    [Serializable]
    public struct HexQuad
    {
        public PHexCube vertex0=>vertices[0];
        public PHexCube vertex1=>vertices[1];
        public PHexCube vertex2=>vertices[2];
        public PHexCube vertex3=>vertices[3];
        public readonly PHexCube[] vertices;
        public PHexCube this[int _index]
        {
            get
            {
                
                switch (_index)
                {
                    default:throw new Exception("Invalid Index:" + _index);
                    case 0: return vertex0;
                    case 1: return vertex1;
                    case 2: return vertex2;
                    case 3: return vertex3;
                }
            }
    }
        public HexQuad(PHexCube _vertex0,PHexCube _vertex1,PHexCube _vertex2,PHexCube _vertex3)
        {
            vertices = new[] {_vertex0, _vertex1, _vertex2,_vertex3};
        }
        public T[] GetVertices<T>(Func<PHexCube, T> toWorld) =>
            new[] {toWorld(vertex0), toWorld(vertex1), toWorld(vertex2),toWorld(vertex3)};
    }
}