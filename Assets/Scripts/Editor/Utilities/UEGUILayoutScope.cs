
using Unity.Mathematics;
using UnityEngine;

namespace UnityEditor.Extensions
{
    public static class GUILayout_HorizontalScope
    {
        static float2 m_StartPos;
        static float2 m_Offset;
        private static float2 m_CurrentRectSize;
        public static void Begin(float2 _initialPos,float _horizontalSize)
        {
            m_StartPos = _initialPos;
            m_CurrentRectSize.x = _horizontalSize;
            m_CurrentRectSize.y = 0;
            m_Offset = 0;
        }
        public static void Begin(Rect _rect)
        {
            m_Offset = 0;
            m_StartPos = _rect.position;
            m_CurrentRectSize.x = _rect.size.x;
            m_CurrentRectSize.y = 0;
        }

        public static Rect NewLine(float _spacing,float _sizeY,float _padding = 0f)
        {
            m_Offset.y += m_CurrentRectSize.y;
            m_Offset.x = _padding;
            m_Offset.y += _spacing;
            m_CurrentRectSize.y = _sizeY;
            return new Rect(m_StartPos + m_Offset, m_CurrentRectSize);
        }

        public static Rect NextRect(float _spacingX, float _sizeX)
        {
            m_Offset.x += _spacingX;
            var rect = new Rect(m_StartPos + m_Offset, new Vector2(_sizeX, m_CurrentRectSize.y));
            m_Offset.x += _sizeX;
            return rect;
        }

        public static Rect NextRectNormalized(float _spacing, float _sizeXNormalized)=> NextRect(_spacing, _sizeXNormalized * m_CurrentRectSize.x);
        public static Rect FinishLineRect(float _spacing = 0)=> NextRect(_spacing, m_CurrentRectSize.x - m_Offset.x - _spacing);
    }
    
    public static class HorizontalScope
    {
        static Vector2 m_StartPos;
        static Vector2 m_Offset;
        public static float m_CurrentY { get; private set; }
        public static Vector2 m_CurrentPos => m_StartPos + m_Offset;
        private static float m_SizeX;
        private static float m_CurrentSizeX;
        public static void Begin(float _startX, float _startY,float _currentY,float _horizontalSize = -1)
        {
            m_StartPos = new Vector2(_startX, _startY);
            m_Offset = Vector2.zero;
            m_SizeX = _horizontalSize;
            m_CurrentY = _currentY;
            m_CurrentSizeX = 0f;
        }
        public static Rect NextRect(float _spacingX, float _sizeX)
        {
            Vector2 originOffset = m_Offset;
            m_Offset.x += _sizeX + _spacingX;
            m_CurrentSizeX += _sizeX + _spacingX;
            return new Rect(m_StartPos + originOffset, new Vector2(_sizeX, m_CurrentY));
        }
        public static Rect NextLine(float _spacingY, float _sizeY)
        {
            m_CurrentY += 0;
            m_Offset.y += m_CurrentY + _spacingY;
            m_CurrentY = _sizeY;
            m_Offset.x = 0;
            return NextRect(0,0);
        }

        public static Rect Finish(float _spacing) => NextRect(_spacing, m_SizeX - m_CurrentSizeX - _spacing);
    }
}