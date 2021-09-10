using System;
using System.Collections;
using System.Collections.Generic;
using Geometry.Voxel;
using UnityEngine;

namespace Geometry
{
    #region Enums
    [Flags]
    public enum EQuadCorners
    {
        F=1,
        R=2,
        B=4,
        L=8,
    }
    
    [Flags]
    public enum EQuadFaces
    {
        LF=1,
        FR=2,
        BL=4,
        RB=8,
    }
    #endregion
    
    #region
    public interface IQuad<T> where T : struct
    {
        T vB { get; set; }
        T vL { get; set; }
        T vF { get; set; }
        T vR { get; set; }
        T this[int _index] { get; }
        T this[EQuadCorners _corner] { get; }
    }

    public interface ITriangle<T> where T : struct
    {
        T vertex0 { get; set; }
        T vertex1 { get; set; }
        T vertex2 { get; set; }
        T this[int index] { get; }
    }
    #endregion
}