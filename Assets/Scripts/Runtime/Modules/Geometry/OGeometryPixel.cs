using System;
using UnityEngine;

namespace Geometry.Pixel
{
    [Serializable]
    public struct G2Triangle:ITriangle<Vector2>,IIterate<Vector2>
    {
        public Vector2 vertex0  { get; set; }
        public Vector2 vertex1 { get; set; }
        public Vector2 vertex2  { get; set; }
        public Vector2[] GetDrawLinesVertices() => new[] { vertex0, vertex1, vertex2, vertex0 };
        public Vector2 this[int index] => GetElement(index);
        public G2Triangle(Vector2 _vertex0, Vector2 _vertex1, Vector2 _vertex2)
        {
            vertex0 = _vertex0;
            vertex1 = _vertex1;
            vertex2 = _vertex2;
        }
        public int Length => 3;
        public Vector2 GetElement(int _index)
        {
            switch (_index)
            {
                default: throw new Exception("Invalid Index:" + _index);
                case 0: return vertex0;
                case 1: return vertex1;
                case 2: return vertex2;
            }
        }
    }

    [Serializable]
    public struct G2Quad:IQuad<Vector2>,IIterate<Vector2>
    {
        public Vector2 vB { get; set; }
        public Vector2 vL { get; set; }
        public Vector2 vF { get; set; }
        public Vector2 vR { get; set; }

        public G2Quad(Vector2 _vertex0, Vector2 _vertex1, Vector2 _vertex2, Vector2 _vertex3)
        {
            vB = _vertex0;
            vL = _vertex1;
            vF = _vertex2;
            vR = _vertex3;
        }
        public Vector2 this[int _index]=>this.GetVertex<G2Quad,Vector2>(_index); 
        public Vector2 this[EQuadCorner _corner] =>this.GetVertex<G2Quad,Vector2>(_corner);
        public Vector2 GetElement(int _index) => this[_index];
        public int Length => 4;

    }
}