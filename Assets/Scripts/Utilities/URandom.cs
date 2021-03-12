using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class URandom 
{
    public static int Random(int length, System.Random seed = null) => seed != null ? seed.Next(length) : UnityEngine.Random.Range(0, length);
    public static float Random(float length, System.Random seed = null) => seed != null ? (float)seed.NextDouble() * length : UnityEngine.Random.Range(0, length);
    public static float RandomUnit(System.Random seed = null) => seed != null ? (float)seed.NextDouble() * 2f - 1f : UnityEngine.Random.Range(-1f, 1f);
    public static Vector3 RandomUnitSphere(System.Random seed = null) => RandomUnitCircle(seed) * RandomUnit(seed);
    public static Vector3 RandomUnitCircle(System.Random seed = null)
    {
        float radin = RandomUnit(seed) * Mathf.PI;
        Vector2 randomCirlce = new Vector2(Mathf.Sin(radin), Mathf.Cos(radin));
        return new Vector3(randomCirlce.x, 0, randomCirlce.y);
    }
    public static int Random(this RangeInt ir, System.Random seed = null) => ir.start + Random(ir.length + 1, seed);
    public static float Random(this RangeFloat ir, System.Random seed = null) => seed != null ? seed.Next((int)(ir.start * 1000), (int)(ir.end * 1000)) / 1000f : UnityEngine.Random.Range(ir.start, ir.end);
    public static int RandomIndex<T>(this List<T> randomList, System.Random seed = null) => Random(randomList.Count, seed);
    public static int RandomIndex<T>(this T[] randomArray, System.Random randomSeed = null) => Random(randomArray.Length, randomSeed);
    public static T RandomItem<T>(this List<T> randomList, System.Random randomSeed = null) => randomList[randomSeed != null ? randomSeed.Next(randomList.Count) : UnityEngine.Random.Range(0, randomList.Count)];
    public static T RandomItem<T>(this T[] array, System.Random randomSeed = null) => randomSeed != null ? array[randomSeed.Next(array.Length)] : array[UnityEngine.Random.Range(0, array.Length)];
    public static T RandomItem<T>(this T[,] array, System.Random randomSeed = null) => randomSeed != null ? array[randomSeed.Next(array.GetLength(0)), randomSeed.Next(array.GetLength(1))] : array[UnityEngine.Random.Range(0, array.GetLength(0)), UnityEngine.Random.Range(0, array.GetLength(1))];
    public static T RandomKey<T, Y>(this Dictionary<T, Y> dic, System.Random randomSeed = null) => dic.ElementAt(Random(dic.Count, randomSeed)).Key;
    public static Y RandomValue<T, Y>(this Dictionary<T, Y> dic, System.Random randomSeed = null) => dic.ElementAt(Random(dic.Count, randomSeed)).Value;
    public static bool RandomBool(System.Random seed = null) => seed != null ? seed.Next(0, 2) > 0 : UnityEngine.Random.Range(0, 2) > 0;
    public static Color RandomColor(System.Random seed = null, float alpha = -1) => new Color(Random(1f, seed), Random(1f, seed), Random(1f, seed), alpha < 0 ? Random(1f, seed) : alpha);
    public static int RandomPercentageInt(System.Random random = null) => random != null ? random.Next(0, 101) : UnityEngine.Random.Range(0, 101);
    public static float RandomPercentageFloat(System.Random random = null) => Random(100, random);
    public static T RandomPercentage<T>(this Dictionary<T, int> percentageRate, System.Random seed) => RandomPercentage(percentageRate, default(T), seed);
    public static T RandomPercentage<T>(this Dictionary<T, int> percentageRate, T invlaid = default(T), System.Random seed = null)
    {
        float value = RandomPercentageInt(seed);
        T targetLevel = invlaid;
        int totalAmount = 0;
        bool marked = false;
        percentageRate.Traversal((T temp, int amount) => {
            if (marked)
                return;
            totalAmount += amount;
            if (totalAmount >= value)
            {
                targetLevel = temp;
                marked = true;
            }
        });
        return targetLevel;
    }
    public static T RandomPercentage<T>(this Dictionary<T, float> percentageRate, T invalid = default(T), System.Random seed = null)
    {
        float value = RandomPercentageInt(seed);
        T targetLevel = invalid;
        float totalAmount = 0;
        bool marked = false;
        percentageRate.Traversal((T temp, float amount) => {
            if (marked)
                return;
            totalAmount += amount;
            if (totalAmount >= value)
            {
                targetLevel = temp;
                marked = true;
            }
        });
        return targetLevel;
    }

    public static T RandomEnumValues<T>(System.Random _seed = null) where T : Enum
    {
        Array allEnums = Enum.GetValues(typeof(T));
        int randomIndex = _seed != null ? _seed.Next(1, allEnums.Length) : UnityEngine.Random.Range(1, allEnums.Length);
        int count = 0;
        foreach (object temp in allEnums)
        {
            count++;
            if (temp.ToString() == "Invalid" || count != randomIndex)
                continue;
            return (T)temp;
        }
        return default;
    }
}
