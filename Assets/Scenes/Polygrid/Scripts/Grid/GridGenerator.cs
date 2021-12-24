using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using UnityEngine;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;
using Procedural.Hexagon.Geometry;

namespace PolyGrid
{
    public partial class GridGenerator : MonoBehaviour
    {
        public bool m_Flat = false;
        public int m_AreaRadius = 8;

        [Header("Smoothen")] public int m_SmoothenTimes = 300;
        [Range(0.001f, 0.5f)] public float m_SmoothenFactor = .1f;

        [Header("Iterate")] public int m_IteratePerFrame = 8;

        private readonly Dictionary<HexCoord, RelaxArea> m_Areas = new Dictionary<HexCoord, RelaxArea>();
        public readonly Dictionary<HexCoord, Coord> m_ExistVertices = new Dictionary<HexCoord, Coord>();

        private readonly Dictionary<EConvexIterate, Stack<IEnumerator>> m_ConvexIterator = 
            new Dictionary<EConvexIterate, Stack<IEnumerator>> () {
            {  EConvexIterate.Tesselation,new Stack<IEnumerator>() },
            {  EConvexIterate.Relaxed,new Stack<IEnumerator>() }, };
        
        private readonly Counter m_IterateCounter = new Counter(1f/60f);

        void Setup()
        {
            UProcedural.InitMatrix(transform.localToWorldMatrix,transform.worldToLocalMatrix, 1f/6f*UMath.SQRT2);
            UHexagon.flat = m_Flat;
            UHexagonArea.Init(m_AreaRadius,6,true);
        } 
        
        void Clear()
        {
            m_ExistVertices.Clear();
            m_Areas.Clear();
            foreach (var key in m_ConvexIterator.Keys)
                m_ConvexIterator[key].Clear();
        }

        public void Tick(float _deltaTime)
        {
            m_IterateCounter.Tick(_deltaTime);
            if (m_IterateCounter.m_Counting)
                return;
            m_IterateCounter.Replay();
            
            int index = m_IteratePerFrame;
            while (index-- > 0)
            {
                EConvexIterate curState = UEnum.GetValues<EConvexIterate>().Find(p=>m_ConvexIterator.ContainsKey(p)&&m_ConvexIterator[p].Count>0);
                if (curState == 0)
                    break;

                if (!m_ConvexIterator[curState].First().MoveNext())
                    m_ConvexIterator[curState].Pop();
            }
        }

        IEnumerator TessellateArea(HexCoord _areaCoord)
        {
            if(!m_Areas.ContainsKey(_areaCoord))
                m_Areas.Add(_areaCoord,new RelaxArea(UHexagonArea.GetArea(_areaCoord)));

            var area = m_Areas[_areaCoord];
            if (area.m_State >= EConvexIterate.Tesselation)
                yield break;
            
            var iterator = area.Tesselation();
            while (iterator.MoveNext())
                yield return null;
        }

        IEnumerator RelaxArea(HexCoord _areaCoord,Dictionary<HexCoord,Coord> _vertices,Action<RelaxArea> _onAreaFinish)
        {
            if (!m_Areas.ContainsKey(_areaCoord))
                yield break;
            var area = m_Areas[_areaCoord];
            if (area.m_State >= EConvexIterate.Relaxed)
                yield break;
            
            var iterator = area.Relax(m_Areas,_vertices,m_SmoothenTimes,m_SmoothenFactor);
            while (iterator.MoveNext())
                yield return null;
            
            _onAreaFinish(area);
        }

        public void ValidateArea(HexCoord _areaCoord)
        {
            foreach (HexagonArea tuple in _areaCoord.GetCoordsInRadius(1).Select(UHexagonArea.GetArea))
                m_ConvexIterator[EConvexIterate.Tesselation].Push(TessellateArea(tuple.coord));
            m_ConvexIterator[EConvexIterate.Relaxed].Push(RelaxArea(_areaCoord,m_ExistVertices,area =>
            {
                Debug.Log($"Area{area.m_Area.coord} Constructed!");
                foreach (var pair in area.m_Vertices)
                    m_ExistVertices.TryAdd(pair.Key,pair.Value);
            }));
        }

    }
}