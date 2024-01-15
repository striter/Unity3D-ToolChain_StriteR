using System;
using Runtime.Geometry;
using UnityEngine;
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
            var positionCS = UCoordinateTransform.CylindricalToCartesian(cAzimuth, cHeight, cRadius);
            Gizmos.DrawWireSphere(positionCS, 0.1f);
            new GDisk(0,Vector3.up,cRadius).DrawGizmos();
            UGizmos.DrawString(positionCS,UCoordinateTransform.CartesianToCylindrical(positionCS).ToString());
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.right * 5f);
            var positionSS = UCoordinateTransform.SphericalToCartesian(sAzimuth, sPolar, sRadius);
            Gizmos.DrawWireSphere(positionSS, 0.1f);
            new GSphere(0,sRadius).DrawGizmos();
            UGizmos.DrawString(positionSS,UCoordinateTransform.CartesianToSpherical(positionSS).ToString());
        }
    }

}