using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UAlgorithm
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
        public static void SwapItem<T>(this List<T> list, int index1, int index2)
        {
            T temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }
        static bool TSortCheck<T>(T a, T b, bool lower) where T : IComparable
        {
            int offset = b.CompareTo(a);
            return lower ? offset < 0 : offset > 0;
        }
        public static void TSort<T>(this List<T> sortTarget, ESortType sortType, bool startLower = true) where T : IComparable
        {
            int count = sortTarget.Count;
            if (count <= 1)
                return;

            switch (sortType)
            {
                case ESortType.Bubble:
                    for (int i = 0; i < count - 1; i++)
                    {
                        bool sorted = true;
                        for (int j = 0; j < count - 1 - i; j++)
                        {
                            if (!TSortCheck(sortTarget[j], sortTarget[j + 1], startLower))
                                continue;
                            sorted = false;
                            sortTarget.SwapItem(j, j + 1);
                        }
                        if (sorted)
                            break;
                    }
                    break;
                case ESortType.Selection:
                    for (int i = 0; i < count - 1; i++)
                    {
                        int selectionIndex = i;
                        for (int j = i + 1; j < count; j++)
                        {
                            if (!TSortCheck(sortTarget[selectionIndex], sortTarget[j], startLower))
                                continue;
                            selectionIndex = j;
                        }
                        if (selectionIndex == i)
                            break;
                        sortTarget.SwapItem(i, selectionIndex);
                    }
                    break;
                case ESortType.Insertion:
                    for (int i = 1; i < count; i++)
                    {
                        for (int j = i; j > 0; j--)
                        {
                            if (TSortCheck(sortTarget[j], sortTarget[j - 1], startLower))
                                break;
                            sortTarget.SwapItem(j, j - 1);
                        }
                    }
                    break;
                case ESortType.Quick:
                    TQuickSort(sortTarget, 0, count - 1, startLower);
                    break;
            }
        }

        static void TQuickSort<T>(List<T> sortTarget, int startIndex, int endIndex, bool startLower) where T : IComparable
        {
            if (startIndex >= endIndex)
                return;

            T temp = sortTarget[startIndex];
            int leftIndex = startIndex;
            int rightIndex = endIndex;
            while (leftIndex != rightIndex)
            {
                while (leftIndex != rightIndex)
                {
                    if (TSortCheck(temp, sortTarget[rightIndex], startLower))
                        break;
                    rightIndex--;
                }
                sortTarget[leftIndex] = sortTarget[rightIndex];

                while (leftIndex != rightIndex)
                {
                    if (TSortCheck(sortTarget[leftIndex], temp, startLower))
                        break;
                    leftIndex++;
                }
                sortTarget[rightIndex] = sortTarget[leftIndex];
            }
            sortTarget[leftIndex] = temp;

            TQuickSort(sortTarget, startIndex, leftIndex - 1, startLower);
            TQuickSort(sortTarget, leftIndex + 1, endIndex, startLower);
        }
    }
    public static class UQuaternion
    {
        public static Quaternion EulerToQuaternion(Vector3 euler) => EulerToQuaternion(euler.x, euler.y, euler.z);
        public static Quaternion EulerToQuaternion(float _angleX, float _angleY, float _angleZ)     //Euler Axis XYZ
        {
            float radinHX = UMath.Deg2Rad*_angleX;
            float radinHY = UMath.Deg2Rad*_angleY;
            float radinHZ = UMath.Deg2Rad*_angleZ;
            float sinHX = Mathf.Sin(radinHX); float cosHX = Mathf.Cos(radinHX);
            float sinHY = Mathf.Sin(radinHY); float cosHY = Mathf.Cos(radinHY);
            float sinHZ = Mathf.Sin(radinHZ); float cosHZ = Mathf.Cos(radinHZ);
            float qX = cosHX * sinHY * sinHZ + sinHX * cosHY * cosHZ;
            float qY = cosHX * sinHY * cosHZ + sinHX * cosHY * sinHZ;
            float qZ = cosHX * cosHY * sinHZ - sinHX * sinHY * cosHZ;
            float qW = cosHX * cosHY * cosHZ - sinHX * sinHY * sinHZ;
            return new Quaternion(qX, qY, qZ, qW);
        }
        public static Quaternion AngleAxisToQuaternion(float _angle, Vector3 _axis)
        {
            float radinH =  UMath.Deg2Rad*_angle / 2;
            float sinH = Mathf.Sin(radinH);
            float cosH = Mathf.Cos(radinH);
            return new Quaternion(_axis.x * sinH, _axis.y * sinH, _axis.z * sinH, cosH);
        }
        public static Matrix3x3 AngleAxisToRotateMatrix(float _angle,Vector3 _axis)
        {
            float radin =  UMath.Deg2Rad*_angle;
            float s = Mathf.Sin(radin);
            float c = Mathf.Cos(radin);

            float t = 1 - c;
            float x = _axis.x;
            float y = _axis.y;
            float z = _axis.z;

            return new Matrix3x3(t * x * x + c, t * x * y - s * z, t * x * z + s * y,
                t * x * y + s * z, t * y * y + c, t * y * z - s * x,
                t * x * z - s * y, t * y * z + s * x, t * z * z + c);
        }
    }

}
