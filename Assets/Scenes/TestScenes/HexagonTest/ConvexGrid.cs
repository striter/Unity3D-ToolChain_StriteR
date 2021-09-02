using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Geometry.Three;
using Geometry.Two;
using GridTest;
using ObjectPool;
using ObjectPoolStatic;
using UnityEngine;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;
using Procedural.Hexagon.Geometry;

namespace ConvexGrid
{
    [ExecuteInEditMode]
    public partial class ConvexGrid : MonoBehaviour
    {
        public bool m_Flat = false;
        public float m_CellRadius = 1;
        public int m_AreaRadius = 8;

        [Header("Smoothen")] public int m_SmoothenTimes = 300;
        [Range(0.001f, 0.5f)] public float m_SmoothenFactor = .1f;

        [Header("Iterate")] public int m_IteratePerFrame = 8;

        private readonly Dictionary<HexCoord, ConvexArea> m_Areas = new Dictionary<HexCoord, ConvexArea>();

        private readonly Dictionary<HexCoord, ConvexVertex> m_Vertices =
            new Dictionary<HexCoord, ConvexVertex>();

        private readonly List<ConvexQuad> m_Quads = new List<ConvexQuad>();

        private readonly Dictionary<EConvexIterate, Stack<IEnumerator>> m_ConvexIterator = 
            new Dictionary<EConvexIterate, Stack<IEnumerator>> () {
            {  EConvexIterate.Tesselation,new Stack<IEnumerator>() },
            {  EConvexIterate.Relaxed,new Stack<IEnumerator>() },
            {  EConvexIterate.Meshed,new Stack<IEnumerator>() } };
        
        private readonly Timer m_IterateTimer = new Timer(1f/60f);

        private int m_QuadSelected=-1;
        private readonly ValueChecker<HexCoord> m_GridSelected =new ValueChecker<HexCoord>(HexCoord.zero);
        private Matrix4x4 m_TransformMatrix;
        void Setup()
        {
            UHexagon.flat = m_Flat;
            UHexagonArea.Init(m_AreaRadius,6,true);
            m_TransformMatrix = transform.localToWorldMatrix*Matrix4x4.Scale(m_CellRadius*Vector3.one);
        } 
        HexCoord ValidateSelection(Coord _localPos,out int quadIndex)
        {
            var quad= m_Quads.Find(p =>p.m_GeometryQuad.IsPointInside(_localPos),out quadIndex);
            if (quadIndex != -1)
                return quad.m_HexQuad[quad.m_GeometryQuad.NearestPointIndex(_localPos)];
            return HexCoord.zero;
        }

        
        void Clear()
        {
            m_QuadSelected = -1;
            m_GridSelected.Check(HexCoord.zero);
            m_Areas.Clear();
            m_Vertices.Clear();
            m_Quads.Clear();
            foreach (var key in m_ConvexIterator.Keys)
                m_ConvexIterator[key].Clear();
            ClearRuntime();
        }

        partial void ClearRuntime();
        
        private void Tick(float _deltaTime)
        {
            m_IterateTimer.Tick(_deltaTime);
            if (m_IterateTimer.m_Timing)
                return;
            m_IterateTimer.Replay();
            
            int index = m_IteratePerFrame;
            while (index-- > 0)
            {
                EConvexIterate curState = UCommon.GetEnumValues<EConvexIterate>().Find(p=>m_ConvexIterator.ContainsKey(p)&&m_ConvexIterator[p].Count>0);
                if (curState == 0)
                    break;

                if (!m_ConvexIterator[curState].First().MoveNext())
                    m_ConvexIterator[curState].Pop();
            }
        }

        IEnumerator TessellateArea(HexCoord _areaCoord)
        {
            if(!m_Areas.ContainsKey(_areaCoord))
                m_Areas.Add(_areaCoord,new ConvexArea(UHexagonArea.GetArea(_areaCoord)));

            var area = m_Areas[_areaCoord];
            var iterator = area.Tesselation();
            while (iterator.MoveNext())
                yield return null;
        }

        IEnumerator RelaxArea(HexCoord _areaCoord)
        {
            if (!m_Areas.ContainsKey(_areaCoord))
                yield break;
            var area = m_Areas[_areaCoord];
            var iterator = area.Relax(m_Areas,m_Vertices,m_Quads,m_SmoothenTimes,m_SmoothenFactor);
            while (iterator.MoveNext())
                yield return null;
        }

        void ValidateArea(HexCoord _areaCoord)
        {
            foreach (HexagonArea tuple in _areaCoord.GetCoordsInRadius(1).Select(UHexagonArea.GetArea))
                m_ConvexIterator[EConvexIterate.Tesselation].Push(TessellateArea(tuple.m_Coord));
            m_ConvexIterator[EConvexIterate.Relaxed].Push(RelaxArea(_areaCoord));
            ValidateAreaRuntime(_areaCoord);
        }

        partial void ValidateAreaRuntime(HexCoord _areaCoord);
    }
}