using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TPool;
using Procedural.Tile;

namespace Examples.Algorithm.WaveFunctionCollapse
{
    public class WaveFunctionCollapse : MonoBehaviour
    {
        public int m_ResolvePerFrame = 1;
        private ObjectPoolBehaviour<int,WaveFunctionContainer>[] m_PossibilitiesPool;
        private ObjectPoolClass<int,WFCTileContainer> m_ObjectPool;
        private readonly Dictionary<Int2, WFCTileContainer> m_Axis=new();
        private readonly List<Int2> m_NoneFinalized=new();
        private void Awake()
        {
            m_ObjectPool = new ObjectPoolClass<int,WFCTileContainer>(transform.Find("Grids/Container"));
            m_PossibilitiesPool = transform.Find("Possibilities").GetComponentsInChildren<WaveFunctionContainer>().Select(p => new ObjectPoolBehaviour<int,WaveFunctionContainer>(p.transform)).ToArray();
            Begin();
        }

        private IEnumerator m_Select;
        private void Update()
        {
            if(Input.GetKey(KeyCode.R))
                Begin();

            if (m_Select != null)
            {
                for (int i = 0; i<m_ResolvePerFrame; i++)
                {
                    if (!m_Select.MoveNext())
                    {
                        m_Select = null;
                        break;
                    }
                }

                return;
            }
            
            if (m_NoneFinalized.Count <= 0)
                return;
            
            m_Axis[m_NoneFinalized .RandomElement()].SelectRandomPossibility();
        }

        void Begin()
        {
            m_Axis.Clear();
            m_NoneFinalized.Clear();
            
            m_ObjectPool.Clear();
            foreach (var possibilitiesPool in m_PossibilitiesPool)
                possibilitiesPool.Clear();
            
            for(int y=-4;y<=4;y++)
                for (int x = -7; x <=7; x++)
                {
                    var possibilities = m_PossibilitiesPool.Select(p => p.Spawn()).ToList();
                    
                    var tile = new Int2(x, y);
                    var container =m_ObjectPool.Spawn().Warmup(tile,possibilities,OnSelect) ;
                    container.m_RectTransform.anchoredPosition =  new Vector2(tile.x*102,tile.y*102);
                    transform.gameObject.name = tile.ToString();
                    m_Axis.Add(tile,  container);
                    m_NoneFinalized.Add(tile);
                }

            foreach (var pair in m_Axis)
            {
                var tile = pair.Key;
                var container = pair.Value;
                foreach (var nearbyPairs in FillNearbyContainers(tile))
                    container.ValidatePossibilities(nearbyPairs.Key,  nearbyPairs.Value==null?m_DefaultData:nearbyPairs.Value.GetAllAvailablePossibilities().Select(p=>p.m_Data));
            }
        }

        private readonly Stack<WFCTileContainer> m_ValidateStack = new Stack<WFCTileContainer>();
        private readonly Dictionary<ETileDirection, WFCTileContainer> m_FillDic = new Dictionary<ETileDirection, WFCTileContainer>();
        private readonly WaveFunctionData[] m_DefaultData = {default};

        void OnSelect(Int2 _tileID)
        {
            if (m_Select != null)
                return;
            m_Select = DoSelect(_tileID);
        }
        IEnumerator DoSelect(Int2 _tileID)
        {
            m_ValidateStack.Push(m_Axis[ _tileID]);
            while (m_ValidateStack.Count>0)
            {
                var container = m_ValidateStack.Pop();
                if (container.m_Finalized)
                    m_NoneFinalized.Remove(container._TileID);
                
                foreach (var nearbyPairs in FillNearbyContainers(container._TileID))
                {
                    if(nearbyPairs.Value==null||!nearbyPairs.Value.ValidatePossibilities(nearbyPairs.Key.Inverse(),container.GetAllAvailablePossibilities().Select(p=>p.m_Data)))
                        continue;
                    m_ValidateStack.Push(nearbyPairs.Value);
                    yield return null;
                }
            }
        }
        
        

        Dictionary<ETileDirection, WFCTileContainer> FillNearbyContainers(Int2 _src)
        {
            m_FillDic.Clear();
            foreach (var tuple in  _src.GetNearbyTilesDirection())
                m_FillDic.Add(tuple.Item1,m_Axis.ContainsKey(tuple.Item2)?m_Axis[tuple.Item2]:null);
            return m_FillDic;
        }
        
        class WFCTileContainer:AWFCTile<ETileDirection,WaveFunctionData>,ITransformHandle,IPoolCallback<int>
        {
            public Transform transform { get; }
            public Action<int> DoRecycle { get; set; }
            public int identity { get; set; }

            public Int2 _TileID;
            public readonly RectTransform m_RectTransform;
            
            private Action<Int2> OnTileSelect;
            public WFCTileContainer(Transform _transform) :base()
            {
                transform = _transform;
                m_RectTransform = transform as RectTransform;
            }

            public WFCTileContainer Warmup(Int2 _tileID, List<WaveFunctionContainer> _allPossibilities,Action<Int2> _OnTileSelect)
            {
                m_Finalized = false;
                _TileID = _tileID;
                OnTileSelect = _OnTileSelect;
                foreach (var tuple in _allPossibilities.LoopIndex())
                {
                    var index = tuple.index;
                    var possibility = tuple.value;
                    int i = index % 4;
                    int j = index / 4;
                    possibility.transform.SetParent(transform);
                    possibility.m_RectTransform.anchoredPosition = new Vector2(100f/4f*i,100/4f*j);
                    possibility.m_RectTransform.localScale = Vector3.one * .5f;
                    m_Possibilities.Add(index,possibility.Setup(index,SelectPossibility));
                }
                return this;
            }

            public void SelectPossibility( int _index)
            {
                var nonePossibles = m_Possibilities.Select(p=>p.Key).Collect(p => p != _index).ToArray();
                foreach (var remove in nonePossibles)
                    RemovePossibility(remove);
                OnTileSelect(_TileID);
            }

            public void SelectRandomPossibility()=> SelectPossibility(m_Possibilities.RandomKey());

            public void RemovePossibility(int _index)
            {
                m_Possibilities[_index].Recycle();
                m_Possibilities.Remove(_index);
                TryFinalize();
            }

            void TryFinalize()
            {
                if(m_Possibilities.Count!=1)
                    return;

                var lastValidate = m_Possibilities.ElementAt(0).Value as WaveFunctionContainer;
                lastValidate.m_RectTransform.anchoredPosition = Vector2.zero;
                lastValidate.m_RectTransform.localScale=Vector3.one;
                m_Finalized = true;
            }

            public bool ValidatePossibilities(ETileDirection _direction, IEnumerable<WaveFunctionData> _tileDatas)
            {
                var removeList = new List<int>();
                foreach (var pair in m_Possibilities)
                {
                    var index = pair.Key;
                    var srcPoss = pair.Value.m_Data;
                    bool validPossibility = _tileDatas.Any(p=>srcPoss.WFCValidate(_direction,p));
                    if(validPossibility)
                        continue;
                    
                    removeList.Add(index);
                }

                foreach (var i in removeList)
                    RemovePossibility(i);
                return removeList.Count > 0;
            }

            public void OnPoolCreate()
            {
            }

            public void OnPoolSpawn()
            {
            }

            public void OnPoolRecycle()
            {
                m_Possibilities.Clear();
            }

            public void OnPoolDispose()
            {
            }
        }
    }

    public abstract class AWFCTile<T,Y> where T:Enum where Y:struct,IWFCCompare<T, Y>
    {
        public Dictionary<int, AWFCContainer<T, Y>> m_Possibilities = new Dictionary<int, AWFCContainer<T, Y>>();
        public IEnumerable<AWFCContainer<T,Y>> GetAllAvailablePossibilities() => m_Possibilities.Values;
        public bool m_Finalized { get; protected set; }
    }

    public abstract class AWFCContainer<T,Y> :PoolBehaviour<int> where T:Enum where Y:struct,IWFCCompare<T, Y>
    {
        [SerializeField] public Y m_Data;
    }

    public interface IWFCCompare<T, Y> where T:Enum where Y:struct
    {
        bool WFCValidate(T _dir,Y _dst);
    }

}