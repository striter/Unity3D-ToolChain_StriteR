using System;
using UnityEngine;
using Procedural.Tile;
using UnityEngine.UI;

namespace Examples.Algorithm.WaveFunctionCollapse
{
    [Serializable]
    public struct WaveFunctionData:IWFCCompare<ETileDirection,WaveFunctionData>
    {
        public bool m_Top;
        public bool m_Left;
        public bool m_Right;
        public bool m_Bottom;
        public bool WFCValidate(ETileDirection _dir, WaveFunctionData _dst)
        {
            switch (_dir)
            {
                default:throw new Exception("Invalid Direction Found:" + _dir);
                case ETileDirection.Forward:return m_Top == _dst.m_Bottom;
                case ETileDirection.Back:return m_Bottom == _dst.m_Top;
                case ETileDirection.Left:return m_Left == _dst.m_Right;
                case ETileDirection.Right:return m_Right == _dst.m_Left;
            }
        }
    }
    public class WaveFunctionContainer :  AWFCContainer<ETileDirection,WaveFunctionData>
    {
        public RectTransform m_RectTransform { get; private set; }
        private UIEventTriggerListenerExtension m_Listener;
        private int m_Index;
        private Action<int> OnSelect;
        private Action<int> OnRecycle;
        private int identityentity;
        public override void OnPoolCreate()
        {
            base.OnPoolCreate();
            m_RectTransform=transform as RectTransform;
            m_Listener = GetComponent<UIEventTriggerListenerExtension>();
            m_Listener.onClickWorld = OnWorldClick;
        }

        public WaveFunctionContainer Setup(int _index,Action<int> _OnSelect)
        {
            m_Index = _index;
            OnSelect = _OnSelect;
            return this;
        }

        void OnWorldClick(Vector2 _click)
        {
            OnSelect?.Invoke(m_Index);
        }
    }
}