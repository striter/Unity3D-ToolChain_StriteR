using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
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
        
        public static implicit operator GQube(Qube<Vector3> _qube) => new GQube(){qube = _qube};
        
        public void DrawGizmos()
        {
            var qube = this.qube;
            Gizmos.DrawLine(qube.vDL,qube.vDF);
            Gizmos.DrawLine(qube.vDF,qube.vDR);
            Gizmos.DrawLine(qube.vDR,qube.vDB);
            
            Gizmos.DrawLine(qube.vTB,qube.vTL);
            Gizmos.DrawLine(qube.vTL,qube.vTF);
            Gizmos.DrawLine(qube.vTF,qube.vTR);
            Gizmos.DrawLine(qube.vTR,qube.vTB);

            Gizmos.DrawLine(qube.vDB,qube.vTB);
            Gizmos.DrawLine(qube.vDL,qube.vTL);
            Gizmos.DrawLine(qube.vDF,qube.vTF);
            Gizmos.DrawLine(qube.vDR,qube.vTR);
        }
    }

    
}