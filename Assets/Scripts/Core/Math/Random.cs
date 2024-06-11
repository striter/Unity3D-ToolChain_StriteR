using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
public static class URandom 
{
    public static float Random01( System.Random _seed = null) => _seed != null ? (float)_seed.NextDouble()  : UnityEngine.Random.Range(0, 1f);
    public static int RandomInt(int _length, System.Random _seed = null)=>_seed?.Next(_length) ?? UnityEngine.Random.Range(0,_length);
    public static float RandomUnit(System.Random _seed = null) => Random01(_seed)*2f-1f;
    
    public static Vector3 RandomSphere(System.Random seed = null) => Random01(seed) * RandomDirection(seed);
    public static Vector3 RandomDirection(System.Random seed = null)
    {
        //Normalization
        // float x = RandomUnit(seed);
        // float y = RandomUnit(seed);
        // float z = RandomUnit(seed);
        // return new Vector3(x, y, z).normalized;
        
        //Spehrerical
        float v = Random01(seed);
        float theta = v * Mathf.PI * 2;
        float phi = Mathf.Acos(  RandomUnit(seed));
        float sinPhi = Mathf.Sin(phi);
        float cosPhi = Mathf.Cos(phi);
        float sinTheta = Mathf.Sin(theta);
        float cosTheta = Mathf.Cos(theta);
        return new Vector3(sinPhi*cosTheta,sinPhi*sinTheta,cosPhi);
    }

    public static float2 Random2DQuad(System.Random _seed = null)=>new float2( Random01(_seed), Random01(_seed));
    public static Vector2 Random2DSphere(System.Random _seed = null) =>Random01(_seed)* Random2DDirection(_seed) ;
    public static Vector2 Random2DDirection(System.Random _seed = null)
    {
        float radin = RandomUnit(_seed) * Mathf.PI;
        Vector2 randomCirlce = new Vector2(Mathf.Sin(radin), Mathf.Cos(radin));
        return new Vector2(randomCirlce.x, randomCirlce.y);
    }

    
    public static int Random(this RangeInt ir, System.Random seed = null) => ir.start + RandomInt(ir.length + 1, seed);
    public static float Random(this RangeFloat ir, System.Random seed = null) => seed?.Next((int)(ir.start * 1000), (int)(ir.end * 1000)) / 1000f ?? UnityEngine.Random.Range(ir.start, ir.end);
    public static int RandomIndex<T>(this List<T> randomList, System.Random seed = null) => RandomInt(randomList.Count, seed);
    public static int RandomIndex<T>(this T[] randomArray, System.Random randomSeed = null) => RandomInt(randomArray.Length, randomSeed);
    public static T RandomElement<T>(this List<T> randomList, System.Random randomSeed = null) => randomList[randomSeed?.Next(randomList.Count) ?? UnityEngine.Random.Range(0, randomList.Count)];
    public static T RandomElement<T>(this T[] array, System.Random randomSeed = null) => randomSeed != null ? array[randomSeed.Next(array.Length)] : array[UnityEngine.Random.Range(0, array.Length)];
    public static T RandomElement<T>(this T[,] array, System.Random randomSeed = null) => randomSeed != null ? array[randomSeed.Next(array.GetLength(0)), randomSeed.Next(array.GetLength(1))] : array[UnityEngine.Random.Range(0, array.GetLength(0)), UnityEngine.Random.Range(0, array.GetLength(1))];
    public static T RandomKey<T, Y>(this Dictionary<T, Y> dic, System.Random randomSeed = null) => dic.ElementAt(RandomInt(dic.Count, randomSeed)).Key;
    public static Y RandomValue<T, Y>(this Dictionary<T, Y> dic, System.Random randomSeed = null) => dic.ElementAt(RandomInt(dic.Count, randomSeed)).Value;
    public static bool RandomBool(System.Random seed = null) => seed != null ? seed.Next(0, 2) > 0 : UnityEngine.Random.Range(0, 2) > 0;
    public static int RandomSign(System.Random seed = null) => (seed != null ? seed.Next(0, 2) > 0 : UnityEngine.Random.Range(0, 2) > 0)?1:-1;
    public static Color RandomColor(System.Random seed = null, float alpha = -1) => new Color(Random01( seed), Random01( seed), Random01( seed), alpha < 0 ? Random01( seed) : alpha);
    public static int RandomPercentageInt(System.Random random = null) => random?.Next(0, 101) ?? UnityEngine.Random.Range(0, 101);
    public static float RandomPercentageFloat(System.Random random = null) => Random01( random)*100f;
    public static T RandomPercentage<T>(this Dictionary<T, int> percentageRate, System.Random seed) => RandomPercentage(percentageRate, default, seed);
    public static T RandomPercentage<T>(this Dictionary<T, int> percentageRate, T invalid = default, System.Random seed = null)
    {
        float value = RandomPercentageInt(seed);
        int totalAmount = 0;
        foreach (var pair in percentageRate)
        {
            totalAmount += pair.Value;
            if (totalAmount >= value)
                return pair.Key;
        }
        return invalid;
    }
    public static T RandomPercentage<T>(this Dictionary<T, float> percentageRate, T invalid = default(T), System.Random seed = null)
    {
        float value = RandomPercentageInt(seed);
        float totalAmount = 0;
        foreach (var pair in percentageRate)
        {
            totalAmount += pair.Value;
            if (totalAmount >= value)
                return pair.Key;
        }
        return invalid;
    }
    
    public static IEnumerable<T> RandomLoop<T>(this List<T> _items, System.Random _seed=null)
    {
        int count = _items.Count();
        int randomIndex = RandomInt(count, _seed);
        for (int i = 0; i < count; i++)
            yield return _items[(randomIndex + i) % count];
    }

    public static IEnumerable<T> RandomLoop<T>(this T[] _items, System.Random _seed=null)
    {
        int count = _items.Count();
        int randomIndex = RandomInt(count, _seed);
        for (int i = 0; i < count; i++)
            yield return _items[(randomIndex + i) % count];
        
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
    
    public static float3 RandomPerpendicular(float3 normal,System.Random _seed = null)
    {
        float3 randomVector;
        do {
            randomVector = RandomSphere(_seed);
        } while (umath.isParallel(normal, randomVector));
        return umath.calculatePerpendicular(randomVector, normal).normalize();
    }

    
#region Shuffle
    public static void Shuffle<T>(IList<T> _array,int _count,int _dimension, System.Random _random=null)
    {
        var shuffleTimes = _count / _dimension;
        
        for (var i = 0; i < shuffleTimes; i++)
        {
            var other = i + (int)(Random01(_random) * (shuffleTimes - i));
            other *= _dimension;
            var src = i * _dimension;
            for (var j = 0; j < _dimension; j++)
            {
                var srcDimension = src + j;
                var otherDimension = other + j;
                (_array[srcDimension], _array[otherDimension]) = (_array[otherDimension], _array[srcDimension]);
            }
        }
    }

    public static void LatinHypercube<T>(IList<T> _array,int _count, int _dimension, System.Random _random = null)
    {
        var shuffleTimes = _count / _dimension;
        for (var i = 0; i < shuffleTimes; i++)
        {
            var other = i + (int)(Random01(_random) * (shuffleTimes - i));
            other *= _dimension;
            var src = i * _dimension;
            var replace = (int) (Random01(_random) * _dimension);

            other += replace;
            src += replace;
            (_array[src], _array[other]) = (_array[other], _array[src]);
        }
    }
#endregion
}
