using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UAlthogrim
{
    public static class USort
    {
        public enum enum_SortType
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
        public static void TSort<T>(this List<T> sortTarget, enum_SortType sortType, bool startLower = true) where T : IComparable
        {
            int count = sortTarget.Count;
            if (count <= 1)
                return;

            switch (sortType)
            {
                case enum_SortType.Bubble:
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
                case enum_SortType.Selection:
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
                case enum_SortType.Insertion:
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
                case enum_SortType.Quick:
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
            float radinHX = UMath.AngleToRadin(_angleX / 2);
            float radinHY = UMath.AngleToRadin(_angleY / 2);
            float radinHZ = UMath.AngleToRadin(_angleZ / 2);
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
            float radinH = UMath.AngleToRadin(_angle / 2);
            float sinH = Mathf.Sin(radinH);
            float cosH = Mathf.Cos(radinH);
            return new Quaternion(_axis.x * sinH, _axis.y * sinH, _axis.z * sinH, cosH);
        }
    }
    public static partial class UVector
    {
        public static float SqrMagnitude(Vector3 _src) => _src.x * _src.x + _src.y * _src.y + _src.z * _src.z;
        public static float Dot(Vector3 _src, Vector3 _dst) => _src.x * _dst.x + _src.y * _dst.y + _src.z * _dst.z;
        public static Vector3 Project(Vector3 _src, Vector3 _dst) => (Dot(_src, _dst) / SqrMagnitude(_dst)) * _dst;
        public static Vector3 Cross(Vector3 _src, Vector3 _dst) => new Vector3(_src.y * _dst.z - _src.z * _dst.y, _src.z * _dst.x - _src.x * _dst.z, _src.x * _dst.y - _src.y * _dst.x);
    }

    public static class UNoise
    {
        private static readonly int[] IA_PerlinPermutation = { 151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180 };
        private static readonly int[] IA_PerlinPremutationRepeat = IA_PerlinPermutation.Add(IA_PerlinPermutation);
        static int Inc(int src) => (src + 1) % 255;
        static double Lerp(double a, double b, double x) => a + x * (b - a);
        public static double Fade(double t) => t * t * t * (t * (t * 6 - 15) + 10);
        public static double Gradient(int hash, double x, double y, double z)
        {
            switch (hash & 0xF)
            {
                case 0x0: return x + y;
                case 0x1: return -x + y;
                case 0x2: return x - y;
                case 0x3: return -x - y;
                case 0x4: return x + z;
                case 0x5: return -x + z;
                case 0x6: return x - z;
                case 0x7: return -x - z;
                case 0x8: return y + z;
                case 0x9: return -y + z;
                case 0xA: return y - z;
                case 0xB: return -y - z;
                case 0xC: return y + x;
                case 0xD: return -y + z;
                case 0xE: return y - x;
                case 0xF: return -y - z;
                default: throw new Exception("Invalid Gradient Result Here!");
            }
        }
        public static double Perlin(double x, double y, double z)
        {
            int xi = (int)x & 255;
            int yi = (int)y & 255;
            int zi = (int)z & 255;
            double xf = x - (int)x;
            double yf = y - (int)y;
            double zf = z - (int)z;
            double u = Fade(xf);
            double v = Fade(yf);
            double w = Fade(zf);

            int[] p = IA_PerlinPremutationRepeat;
            int aaa, aba, aab, abb, baa, bba, bab, bbb;
            aaa = p[p[p[xi] + yi] + zi];
            aba = p[p[p[xi] + Inc(yi)] + zi];
            aab = p[p[p[xi] + yi] + Inc(zi)];
            abb = p[p[p[xi] + Inc(yi)] + Inc(zi)];
            baa = p[p[p[Inc(xi)] + yi] + zi];
            bba = p[p[p[Inc(xi)] + Inc(yi)] + zi];
            bab = p[p[p[Inc(xi)] + yi] + Inc(zi)];
            bbb = p[p[p[Inc(xi)] + Inc(yi)] + Inc(zi)];

            double x1, x2, y1, y2;
            x1 = Lerp(Gradient(aaa, xf, yf, zf), Gradient(baa, xf - 1, yf, zf), u);
            x2 = Lerp(Gradient(aba, xf, yf - 1, zf), Gradient(bba, xf - 1, yf - 1, zf), u);
            y1 = Lerp(x1, x2, v);
            x1 = Lerp(Gradient(aab, xf, yf, zf - 1), Gradient(bab, xf - 1, yf, zf - 1), u);
            x2 = Lerp(Gradient(abb, xf, yf - 1, zf - 1), Gradient(bbb, xf - 1, yf - 1, zf - 1), u);
            y2 = Lerp(x1, x2, v);
            return (Lerp(y1, y2, w) + 1) / 2;
        }

        public static double OctavePerlin(double x, double y, double z, int octaves, double persistence)
        {
            double total = 0;
            double frequency = 1;
            double amplitude = 1;
            double maxValue = 0;
            for (int i = 0; i < octaves; i++)
            {
                total += Perlin(x * frequency, y * frequency, z * frequency) * amplitude;
                maxValue += amplitude;
                amplitude *= persistence;
                frequency *= 2;
            }
            return total / maxValue;
        }
    }

}
