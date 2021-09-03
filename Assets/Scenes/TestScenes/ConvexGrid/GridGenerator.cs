using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry.Pixel;
using ObjectPool;
using UnityEngine;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;

namespace ConvexGrid
{
    [ExecuteInEditMode]
    public partial class GridGenerator : MonoBehaviour,IConvexGridControl
    {
        public bool m_Flat = false;
        public int m_AreaRadius = 8;

        [Header("Smoothen")] public int m_SmoothenTimes = 300;
        [Range(0.001f, 0.5f)] public float m_SmoothenFactor = .1f;

        [Header("Iterate")] public int m_IteratePerFrame = 8;

        private readonly Dictionary<HexCoord, ConvexArea> m_Areas = new Dictionary<HexCoord, ConvexArea>();

        private readonly Dictionary<EConvexIterate, Stack<IEnumerator>> m_ConvexIterator = 
            new Dictionary<EConvexIterate, Stack<IEnumerator>> () {
            {  EConvexIterate.Tesselation,new Stack<IEnumerator>() },
            {  EConvexIterate.Relaxed,new Stack<IEnumerator>() }, };
        
        private readonly Timer m_IterateTimer = new Timer(1f/60f);

        public void Init(Transform _transform)
        {
            
        }
        void Setup()
        {
            ConvexGridHelper.InitRelax(m_SmoothenTimes,m_SmoothenFactor);
            UHexagon.flat = m_Flat;
            UHexagonArea.Init(m_AreaRadius,6,true);
        } 
        
        public void Select(bool _valid,HexCoord _coord, ConvexVertex _vertex)
        {
        }

        public void Clear()
        {
            m_Areas.Clear();
            foreach (var key in m_ConvexIterator.Keys)
                m_ConvexIterator[key].Clear();
        }


        public void Tick(float _deltaTime)
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
            if (area.m_State >= EConvexIterate.Tesselation)
                yield break;
            
            var iterator = area.Tesselation();
            while (iterator.MoveNext())
                yield return null;
        }

        IEnumerator RelaxArea(HexCoord _areaCoord,Dictionary<HexCoord,ConvexVertex> _vertices,Action<ConvexArea> _onAreaFinish)
        {
            if (!m_Areas.ContainsKey(_areaCoord))
                yield break;
            var area = m_Areas[_areaCoord];
            if (area.m_State >= EConvexIterate.Relaxed)
                yield break;
            
            var iterator = area.Relax(m_Areas,_vertices);
            while (iterator.MoveNext())
                yield return null;
            
            _onAreaFinish(area);
        }

        public void ValidateArea(HexCoord _areaCoord,Dictionary<HexCoord,ConvexVertex> _existVertices,Action<ConvexArea> _onAreaRelaxed)
        {
            foreach (HexagonArea tuple in _areaCoord.GetCoordsInRadius(1).Select(UHexagonArea.GetArea))
                m_ConvexIterator[EConvexIterate.Tesselation].Push(TessellateArea(tuple.m_Coord));
            m_ConvexIterator[EConvexIterate.Relaxed].Push(RelaxArea(_areaCoord,_existVertices,_onAreaRelaxed));
        }
    }
}