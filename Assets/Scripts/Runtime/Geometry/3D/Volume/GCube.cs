using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry
{
    
    [Serializable]
    public struct GQube : IQube<Vector3>,IIterate<Vector3>
    {
        public Qube<Vector3> qube;

        public Vector3 this[int _index] => throw new NotImplementedException();

        public Vector3 DB => qube.vDB;
        public Vector3 DL => qube.vDL;
        public Vector3 DF => qube.vDF;
        public Vector3 DR => qube.vDR;
        public Vector3 TB => qube.vTB;
        public Vector3 TL => qube.vTL;
        public Vector3 TF => qube.vTF;
        public Vector3 TR => qube.vTR;
        public int Length => qube.Length;
    }

    
}