using System;
using System.Collections.Generic;
using UnityEngine;

namespace AlgorithmExtension
{
    public enum ESortType
    {
        Bubble,
        Selection,
        Insertion,

        Quick,
    }

    public static class USorting
    {
        private static void SwapItem<T>(this IList<T> list, int index1, int index2)
        {
            (list[index1], list[index2]) = (list[index2], list[index1]);
        }
        public static void Sort<T>(this IList<T> _sortTarget, ESortType _sortType, Comparison<T> _comparison) 
        {
            int count = _sortTarget.Count;
            if (count <= 1)
                return;
            
            switch (_sortType)
            {
                case ESortType.Bubble:
                    BubbleSort(_sortTarget, _comparison);
                    break;
                case ESortType.Selection:
                    SelectionSort(_sortTarget,_comparison);
                    break;
                case ESortType.Insertion:
                    InsertionSort(_sortTarget,_comparison);
                    break;
                case ESortType.Quick:
                    QuickSort(_sortTarget, 0, count - 1, _comparison);
                    break;
            }
        }

        static void BubbleSort<T>(IList<T> _sortTarget, Comparison<T> _compare)
        {
            int count = _sortTarget.Count;
            for (int i = 0; i < count - 1; i++)
            {
                bool sorted = true;
                for (int j = 0; j < count - 1 - i; j++)
                {
                    if (_compare(_sortTarget[j], _sortTarget[j + 1])<0)
                        continue;
                    sorted = false;
                    _sortTarget.SwapItem(j, j + 1);
                }
                if (sorted)
                    break;
            }
        }
        static void SelectionSort<T>(IList<T> _sortTarget, Comparison<T> _compare)
        {
            int count = _sortTarget.Count;
            for (int i = 0; i < count - 1; i++)
            {
                int selectionIndex = i;
                for (int j = i + 1; j < count; j++)
                {
                    if (_compare(_sortTarget[selectionIndex], _sortTarget[j])<0)
                        continue;
                    selectionIndex = j;
                }
                if (selectionIndex == i)
                    break;
                _sortTarget.SwapItem(i, selectionIndex);
            }
        }
        static void InsertionSort<T>(IList<T> _sortTarget, Comparison<T> _compare)
        {
            int count = _sortTarget.Count;
            for (int i = 1; i < count; i++)
            {
                for (int j = i; j > 0; j--)
                {
                    if (_compare(_sortTarget[j], _sortTarget[j - 1])<0)
                        break;
                    _sortTarget.SwapItem(j, j - 1);
                }
            }
        }

        static void QuickSort<T>(IList<T> _sortTarget, int _startIndex, int _endIndex, Comparison<T> _compare,int _estimate = -1)
        {
            if (_startIndex >= _endIndex)
                return;
            T pivot = _sortTarget[_startIndex];
            int leftIndex = _startIndex;
            int rightIndex = _endIndex;
            while (leftIndex != rightIndex)
            {
                while (leftIndex != rightIndex)
                {
                    if (_compare(_sortTarget[rightIndex],pivot)<0)
                        break;
                    rightIndex--;
                }
                _sortTarget[leftIndex] = _sortTarget[rightIndex];

                while (leftIndex != rightIndex)
                {
                    if (_compare( pivot,_sortTarget[leftIndex])<0)
                        break;
                    leftIndex++;
                }
                _sortTarget[rightIndex] = _sortTarget[leftIndex];
            }
            _sortTarget[leftIndex] = pivot;
        
            if(_estimate < 0 || leftIndex  >= _estimate)
                QuickSort(_sortTarget, _startIndex, leftIndex - 1, _compare,_estimate);
            if(_estimate < 0 || leftIndex  <= _estimate)
                QuickSort(_sortTarget, leftIndex + 1, _endIndex, _compare,_estimate);
        }

        public static void Divide<T>(this IList<T> _sortTarget, int _estimate, Comparison<T> _comparison)
        {
            QuickSort(_sortTarget, 0, _sortTarget.Count - 1,_comparison,_estimate);
        }
    }

}
