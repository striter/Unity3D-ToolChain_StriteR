using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Hexagon;
namespace GridTest
{
    public class GridTest_HexCube : MonoBehaviour
    {
        public bool m_Flat = false;
        public float m_Size = 1f;
        public int m_XSize = 1;
        public int m_YSize = 1;
        public int m_ZSize = 1;
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            UHexagon.flat=m_Flat;
            for(int i=0;i<m_XSize;i++)
                for(int j=0;j<m_YSize;j++)
                for (int k = 0; k < m_ZSize; k++)
                {
                    HexCube cube = new HexCube(i, j, k);

                    var pos = cube.ToAxial().ToPixel().ToWorld(m_Size);
                    Gizmos_Extend.DrawLines(UHexagon.GetPoints().Select(p=>pos+p.ToWorld(m_Size)).ToArray());
                }
        }
        #endif
    }

}