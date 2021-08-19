using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UHexagon;
namespace BoundingCollisionTest
{
    public class GridTest_Hexagon2DAxis : MonoBehaviour
    {
        public float m_Radius = 1;
        public int m_CellSizeX = 3, m_CellSizeY = 4;
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.green;
            for (int i = 0; i < m_CellSizeX; i++)
            {
                for (int j = 0; j < m_CellSizeY; j++)
                {
                    Vector3 origin = Vector3.zero + UHexagonHelper.Axis2D(m_Radius, i, j) + (i * j + i) * Vector3.up * 0.01f;
                    Vector3[] hexagonList = UHexagonConst.C_UnitHexagonPoints.Select(value => new Vector3(value.x, 0, value.y) * m_Radius + origin).ToArray();
                    Gizmos_Extend.DrawLines(hexagonList);
                }
            }
        }
        #endif
    }
}