using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GridTest;
using UnityEngine;
using Hexagon;
using Unity.Mathematics;

namespace Test
{
    public class GridTest_HexAxial : MonoBehaviour
    {
        public enum EGridAxialTest
        {
            Hit,
            Range,
            Intersect,
            Distance,
            Nearby,
            Mirror,
            Reflect,
            Wrap,
        }

        public enum EAreaVisualize
        {
            Area,
            X,
            Y,
            Z,
        }
        public bool m_Flat = false;
        public float m_Size = 1;
        public int m_CellRadius = 8;
        [Range(2,5)]public int m_AreaRadius = 3;
        public EAreaVisualize m_Visualize= EAreaVisualize.Area;
        public EGridAxialTest m_Test =  EGridAxialTest.Hit;
        [MFoldout(nameof(m_Test),EGridAxialTest.Range,EGridAxialTest.Intersect,EGridAxialTest.Distance)][Range(1,5)]public int m_Radius1;
        [MFoldout(nameof(m_Test),EGridAxialTest.Intersect,EGridAxialTest.Distance)]public HexAxial m_TestAxialPoint=new HexAxial(2,1);
        [MFoldout(nameof(m_Test),EGridAxialTest.Intersect)]public int m_Radius2;
        [MFoldout(nameof(m_Test),EGridAxialTest.Wrap)] public Int2 m_Wrap;
        [MFoldout(nameof(m_Test),EGridAxialTest.Reflect)]public ECubeAxis m_Axis = ECubeAxis.X;
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            UHexagon.flat = m_Flat;
            Gizmos.matrix=transform.localToWorldMatrix;
            Gizmos.color = Color.white;
            
            foreach (HexAxial axialPoint in LoopAxialPoints())
            {
                var axisPos = axialPoint.ToPixel().ToWorld(m_Size);
                Gizmos.color = GetCellColor(axialPoint.ToCube());
                Vector3[] hexagonList = UHexagon.GetPoints().Select(p=>p.ToWorld(m_Size*.95f)  + axisPos).ToArray();
                Gizmos_Extend.DrawLines(hexagonList);
            }

            
            var hitPixel = transform.InverseTransformPoint(GridHelper.SceneRayHit(transform.position))
                .ToPixel(m_Size);
            Gizmos.DrawRay(hitPixel.ToWorld(m_Size), Vector3.up);
            var axialHit = hitPixel.ToAxial();
            
            switch (m_Test)
            {
                case EGridAxialTest.Hit:
                    {
                        Gizmos.color = Color.green;

                        var colPixel = hitPixel.SetCol(0);
                        var colAxis = colPixel.ToAxial();
                        var rowPixel = hitPixel.SetRow(0);
                        var rowAxis = rowPixel.ToAxial();
                        Gizmos.color = Color.blue;
                        Gizmos.DrawRay(colPixel.ToWorld(m_Size), Vector3.up);
                        Gizmos.DrawLine(Vector3.zero, colPixel.ToWorld(m_Size));
                        Gizmos.DrawLine(colPixel.ToWorld(m_Size), hitPixel.ToWorld(m_Size));
                        Gizmos.color = Color.red;
                        Gizmos.DrawRay(rowPixel.ToWorld(m_Size), Vector3.up);
                        Gizmos.DrawLine(Vector3.zero, rowPixel.ToWorld(m_Size));
                        Gizmos.DrawLine(rowPixel.ToWorld(m_Size), hitPixel.ToWorld(m_Size));
                        foreach (HexAxial axialPoint in LoopAxialPoints())
                        {
                            if (!GetGizmosColors(axialHit, axialPoint, colAxis, rowAxis,out Color col ))
                                continue;
                            Gizmos.color = col;
                            
                            axialPoint.DrawHexagon(m_Size);
                        }
                    }
                    break;
                case EGridAxialTest.Range:
                {
                    Gizmos.color = Color.yellow;
                    foreach (HexAxial axialPoint in LoopAxialPoints())
                    {
                        var offset = axialHit - axialPoint;
                        if(!offset.Inside(m_Radius1))
                            continue;
                        
                        axialPoint.DrawHexagon(m_Size,.5f);
                    }
                }
                break;
                case EGridAxialTest.Intersect:
                {
                    foreach (HexAxial axialPoint in LoopAxialPoints())
                    {
                        var offset1 = m_TestAxialPoint - axialPoint;
                        var offset2 = axialHit - axialPoint;
                        bool inRange1 = offset1.Inside(m_Radius2);
                        bool inRange2 = offset2.Inside(m_Radius1);
                        if (inRange1 && inRange2)
                            Gizmos.color = Color.cyan;
                        else if (inRange1)
                            Gizmos.color = Color.green;
                        else if (inRange2)
                            Gizmos.color = Color.blue;
                        else
                            continue;
                        
                        axialPoint.DrawHexagon(m_Size,.5f);
                    }
                }
                break;
                case EGridAxialTest.Distance:
                {
                    foreach (HexAxial axialPoint in LoopAxialPoints())
                    {
                        int offset = m_TestAxialPoint.Distance(axialPoint);
                        if(offset>=m_Radius1)
                            continue;
                        Gizmos.color = Color.Lerp(Color.green, Color.yellow, ((float) offset) / m_Radius1);
                        axialPoint.DrawHexagon(m_Size,.5f);
                    }
                }
                break;
                case EGridAxialTest.Nearby:
                {
                    foreach (var nearbyAxial in HexAxial.m_NearbyCoords.LoopIndex())
                    {
                        HexAxial dstAxial = axialHit + nearbyAxial.item;
                        Gizmos.color = Color.Lerp(Color.blue, Color.red, nearbyAxial.index / 6f);
                        dstAxial.DrawHexagon(m_Size,1f);
                    }
                }
                break;
                case EGridAxialTest.Mirror:
                {
                    for (int i = 0; i < 6; i++)
                    {
                        Gizmos.color = Color.Lerp(Color.green, Color.red, ((float) i) / 6);
                        var axialOffset = UHexagon.RotateMirror(m_CellRadius-1, i).ToAxial();
                        foreach (HexAxial axialPoint in LoopAxialPoints())
                            (axialPoint+axialOffset).DrawHexagon(m_Size);
                    }
                }
                break;
                case EGridAxialTest.Reflect:
                {
                    var axialHitCube = axialHit.ToCube();
                    var reflectCube = axialHitCube.Reflect(m_Axis);
                    Gizmos.color = Color.yellow;
                    axialHit.DrawHexagon(m_Size,1f);
                    Gizmos.color = Color.green;
                    reflectCube.ToAxial().DrawHexagon(m_Size,1f);
                    Gizmos.color = Color.blue;
                    (-axialHitCube).ToAxial().DrawHexagon(m_Size,1f);
                    Gizmos.color = Color.red;
                    (-reflectCube).ToAxial().DrawHexagon(m_Size,1f);
                }
                break;
            }
        }

        IEnumerable<HexAxial> LoopAxialPoints()
        {
            for (int i = -m_CellRadius; i < m_CellRadius + 1; i++)
            for (int j = -m_CellRadius; j < m_CellRadius + 1; j++)
            {
                var axialPoint = new HexAxial(i, j);
                if (!axialPoint.Inside(m_CellRadius))
                    continue;
                yield return new HexAxial(i, j);
            }
        }
        bool GetGizmosColors(HexAxial _axialHit, HexAxial _axialPoint, HexAxial _colAxis, HexAxial _rowAxis,
            out Color col)
        {
            col = Color.black;
            if (_axialHit == _axialPoint)
            {
                col = Color.yellow;
                return true;
            }

            if (_axialPoint == _colAxis)
            {
                col= Color.blue;
                return true;
            }
            
            if (_axialPoint == _rowAxis)
            {
                col= Color.red;
                return true;
            }
            return false;
        }

        Color GetCellColor(HexCube _cell)
        {              
            var area = UHexagon.CellToArea(_cell,m_AreaRadius,out int xh,out int yh,out int zh);
            switch (m_Visualize)
            {
                    case EAreaVisualize.Area:
                    {
                        int number =(int.MaxValue/2+ area.x-area.y) % 3;
                        switch (number)
                        {
                            default: return Color.white;
                            case 1: return Color.grey;
                            case 2: return Color.cyan;
                        }
                    }
                    case EAreaVisualize.X:
                    {
                        if ((int.MaxValue / 2 + xh) % 2 == 1)
                            return Color.white;
                        return Color.cyan;
                    }
                case EAreaVisualize.Y:
                    if ((int.MaxValue / 2 + yh) % 2 == 1)
                        return Color.white;
                    return Color.cyan;
                case EAreaVisualize.Z:
                    if ((int.MaxValue / 2 + zh) % 2 == 1)
                        return Color.white;
                    return Color.cyan;
            }

            return Color.white;
        }
        #endif
    }

    
}