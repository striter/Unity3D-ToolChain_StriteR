using System;
using Runtime.Geometry;
using UnityEngine;
using System.Linq.Extensions;
using Gizmos = UnityEngine.Gizmos;

namespace Examples.Algorithm.Geometry
{
    public class GeometryVisualizeCoordinates : MonoBehaviour
    {
        [Header("Cylindrical")] public float cAzimuth;
        public float cHeight;
        public float cRadius;


        [Header("Spherical")] public float sAzimuth;
        public float sPolar;
        public float sRadius;

        public void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            var positionCS = UCoordinates.Cylindrical.ToCartesian(cAzimuth, cHeight, cRadius);
            Gizmos.DrawWireSphere(positionCS, 0.1f);
            new GDisk(0,Vector3.up,cRadius).DrawGizmos();
            UGizmos.DrawString(UCoordinates.Cylindrical.ToCylindrical(positionCS).ToString(), positionCS);
            new GCylinder(0, kfloat3.up, cHeight, cRadius).DrawGizmos();
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.right * 5f);
            var positionSS = UCoordinates.Spherical.ToCartesian(sAzimuth, sPolar, sRadius);
            Gizmos.DrawWireSphere(positionSS, 0.1f);
            new GSphere(0,sRadius).DrawGizmos();
            UGizmos.DrawString(UCoordinates.Spherical.ToSpherical(positionSS).ToString(), positionSS);
        }
    }

}