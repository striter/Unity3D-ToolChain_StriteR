using System;
using System.Collections;
using System.Collections.Generic;
using Geometry.Voxel;
using UnityEngine;

namespace ConvexGrid
{
    public class ModuleBakeContainer : MonoBehaviour
    {
        public EQubeCorner m_Corners;

        private static readonly GQube m_Qube = new GQuad(Vector3.left+Vector3.back,Vector3.left+Vector3.forward,Vector3.forward+Vector3.right,Vector3.right+Vector3.back).ExpandToQUbe(Vector3.up*2f,.5f);
        public void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            foreach (var corner in UCommon.GetEnumValues<EQubeCorner>())
            {
                Gizmos.color = m_Corners.IsFlagEnable(corner) ? Color.green : Color.red;
                Gizmos.DrawSphere(m_Qube[corner],.05f);
            }
        }
    }
}