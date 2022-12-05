using System;
using System.Collections.Generic;

namespace AlgorithmExtension
{
    public static class USorting
    {
        public enum ESortType
        {
            Bubble,
            Selection,
            Insertion,

            Quick,
        }

        private static void SwapItem<T>(this IList<T> list, int index1, int index2)
        {
            (list[index1], list[index2]) = (list[index2], list[index1]);
        }
        public static void TSort<T>(this List<T> sortTarget, ESortType sortType, Comparison<T> _comparison) where T : IComparable
        {
            int count = sortTarget.Count;
            if (count <= 1)
                return;
            
            switch (sortType)
            {
                case ESortType.Bubble:
                    BubbleSort(sortTarget, _comparison);
                    break;
                case ESortType.Selection:
                    SelectionSort(sortTarget,_comparison);
                    break;
                case ESortType.Insertion:
                    InsertionSort(sortTarget,_comparison);
                    break;
                case ESortType.Quick:
                    QuickSort(sortTarget, 0, count - 1, _comparison);
                    break;
            }
        }

        static void BubbleSort<T>(IList<T> sortTarget, Comparison<T> _compare)
        {
            int count = sortTarget.Count;
            for (int i = 0; i < count - 1; i++)
            {
                bool sorted = true;
                for (int j = 0; j < count - 1 - i; j++)
                {
                    if (_compare(sortTarget[j], sortTarget[j + 1])<0)
                        continue;
                    sorted = false;
                    sortTarget.SwapItem(j, j + 1);
                }
                if (sorted)
                    break;
            }
        }
        static void SelectionSort<T>(IList<T> sortTarget, Comparison<T> _compare)
        {
            int count = sortTarget.Count;
            for (int i = 0; i < count - 1; i++)
            {
                int selectionIndex = i;
                for (int j = i + 1; j < count; j++)
                {
                    if (_compare(sortTarget[selectionIndex], sortTarget[j])<0)
                        continue;
                    selectionIndex = j;
                }
                if (selectionIndex == i)
                    break;
                sortTarget.SwapItem(i, selectionIndex);
            }
        }
        static void InsertionSort<T>(IList<T> sortTarget, Comparison<T> _compare)
        {
            int count = sortTarget.Count;
            for (int i = 1; i < count; i++)
            {
                for (int j = i; j > 0; j--)
                {
                    if (_compare(sortTarget[j], sortTarget[j - 1])<0)
                        break;
                    sortTarget.SwapItem(j, j - 1);
                }
            }
        }
        static void QuickSort<T>(IList<T> sortTarget, int startIndex, int endIndex, Comparison<T> _compare) where T : IComparable
        {
            if (startIndex >= endIndex)
                return;

            T temp = sortTarget[0];
            int leftIndex = 0;
            int rightIndex = sortTarget.Count-1;
            while (leftIndex != rightIndex)
            {
                while (leftIndex != rightIndex)
                {
                    if (_compare(temp, sortTarget[rightIndex])<0)
                        break;
                    rightIndex--;
                }
                sortTarget[leftIndex] = sortTarget[rightIndex];

                while (leftIndex != rightIndex)
                {
                    if (_compare(sortTarget[leftIndex], temp)<0)
                        break;
                    leftIndex++;
                }
                sortTarget[rightIndex] = sortTarget[leftIndex];
            }
            sortTarget[leftIndex] = temp;

            QuickSort(sortTarget, startIndex, leftIndex - 1, _compare);
            QuickSort(sortTarget, leftIndex + 1, endIndex, _compare);
        }
    }

}
