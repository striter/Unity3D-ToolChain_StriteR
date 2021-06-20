using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public static class URandom 
{
    public static float Random01( System.Random seed = null) => seed != null ? (float)seed.NextDouble()  : UnityEngine.Random.Range(0, 1f);
    public static int RandomInt(int length, System.Random seed = null)=>seed?.Next(length) ?? UnityEngine.Random.Range(0,length);
    public static float RandomUnit(System.Random seed = null) => Random01(seed)*2f-1f;
    
    public static Vector2 RandomUnitSphere2D(System.Random seed = null) =>Random01(seed)* RandomVector2(seed) ;
    public static Vector2 RandomVector2(System.Random seed = null)
    {
        float radin = RandomUnit(seed) * Mathf.PI;
        Vector2 randomCirlce = new Vector2(Mathf.Sin(radin), Mathf.Cos(radin));
        return new Vector2(randomCirlce.x, randomCirlce.y);
    }
    public static Vector3 RandomUnitSphere(System.Random seed = null) => Random01(seed) * RandomVector3(seed);
    public static Vector3 RandomVector3(System.Random seed = null)
    {
        //Normalization
        // float x = RandomUnit(seed);
        // float y = RandomUnit(seed);
        // float z = RandomUnit(seed);
        // return new Vector3(x, y, z).normalized;
        
        //Spehrerical
        float theta = Random01(seed) * Mathf.PI;
        float phi = Mathf.Acos( RandomUnit(seed));
        float sinPhi = Mathf.Sin(phi);
        float cosPhi = Mathf.Cos(phi);
        float sinTheta = Mathf.Sin(theta);
        float cosTheta = Mathf.Cos(theta);
        return new Vector3(sinPhi*cosTheta,sinPhi*sinTheta,cosPhi);
    }
    
    
    public static int Random(this RangeInt ir, System.Random seed = null) => ir.start + RandomInt(ir.length + 1, seed);
    public static float Random(this RangeFloat ir, System.Random seed = null) => seed?.Next((int)(ir.start * 1000), (int)(ir.end * 1000)) / 1000f ?? UnityEngine.Random.Range(ir.start, ir.end);
    public static int RandomIndex<T>(this List<T> randomList, System.Random seed = null) => RandomInt(randomList.Count, seed);
    public static int RandomIndex<T>(this T[] randomArray, System.Random randomSeed = null) => RandomInt(randomArray.Length, randomSeed);
    public static T RandomItem<T>(this List<T> randomList, System.Random randomSeed = null) => randomList[randomSeed?.Next(randomList.Count) ?? UnityEngine.Random.Range(0, randomList.Count)];
    public static T RandomItem<T>(this T[] array, System.Random randomSeed = null) => randomSeed != null ? array[randomSeed.Next(array.Length)] : array[UnityEngine.Random.Range(0, array.Length)];
    public static T RandomItem<T>(this T[,] array, System.Random randomSeed = null) => randomSeed != null ? array[randomSeed.Next(array.GetLength(0)), randomSeed.Next(array.GetLength(1))] : array[UnityEngine.Random.Range(0, array.GetLength(0)), UnityEngine.Random.Range(0, array.GetLength(1))];
    public static T RandomKey<T, Y>(this Dictionary<T, Y> dic, System.Random randomSeed = null) => dic.ElementAt(RandomInt(dic.Count, randomSeed)).Key;
    public static Y RandomValue<T, Y>(this Dictionary<T, Y> dic, System.Random randomSeed = null) => dic.ElementAt(RandomInt(dic.Count, randomSeed)).Value;
    public static bool RandomBool(System.Random seed = null) => seed != null ? seed.Next(0, 2) > 0 : UnityEngine.Random.Range(0, 2) > 0;
    public static Color RandomColor(System.Random seed = null, float alpha = -1) => new Color(Random01( seed), Random01( seed), Random01( seed), alpha < 0 ? Random01( seed) : alpha);
    public static int RandomPercentageInt(System.Random random = null) => random?.Next(0, 101) ?? UnityEngine.Random.Range(0, 101);
    public static float RandomPercentageFloat(System.Random random = null) => Random01( random)*100f;
    public static T RandomPercentage<T>(this Dictionary<T, int> percentageRate, System.Random seed) => RandomPercentage(percentageRate, default, seed);
    public static T RandomPercentage<T>(this Dictionary<T, int> percentageRate, T invalid = default, System.Random seed = null)
    {
        float value = RandomPercentageInt(seed);
        T targetLevel = invalid;
        int totalAmount = 0;
        bool marked = false;
        percentageRate.Traversal((temp, amount) => {
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
        percentageRate.Traversal((temp,amount)=>
        {
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
        int randomIndex = _seed?.Next(1, allEnums.Length) ?? UnityEngine.Random.Range(1, allEnums.Length);
        int count = 0;
        foreach (var temp in allEnums)
        {
            count++;
            if (temp.ToString() == "Invalid" || count != randomIndex)
                continue;
            return (T)temp;
        }
        return default;
    }
}
