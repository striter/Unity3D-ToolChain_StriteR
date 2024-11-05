using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Runtime.Random;
using Unity.Mathematics;
using UnityEngine;
public static class URandom 
{
    public static float Random01(IRandomGenerator _random = null)
    {
        _random ??= UnityRandom.kDefault;
        return _random.NextFloat();
    }

    public static int RandomInt(int _max, IRandomGenerator _random = null)
    {
        _random ??= UnityRandom.kDefault;
        return _random.NextInt(_max);
    }

    public static float RandomFloat(float _max, IRandomGenerator _random = null)
    {
        _random ??= UnityRandom.kDefault;
        return _random.NextFloat(_max);
    }
    
    public static float RandomUnit(IRandomGenerator _seed = null) => Random01(_seed)*2f-1f;
    
    public static Vector3 RandomSphere(IRandomGenerator seed = null) => Random01(seed) * RandomDirection(seed);

    public static Vector3 RandomDirection(IRandomGenerator seed = null)
    {
        seed ??= UnityRandom.kDefault;
        return UCoordinates.Spherical.ToCartesian(seed.NextFloat(kmath.kPI2),seed.NextFloat(kmath.kPI2),1);
    }

    public static float2 Random2DQuad(IRandomGenerator _seed = null) => new float2( Random01(_seed), Random01(_seed));
    public static Vector2 Random2DSphere(IRandomGenerator _seed = null) => Random01(_seed)* Random2DDirection(_seed) ;
    public static Vector2 Random2DDirection(IRandomGenerator _seed = null)
    {
        var radin = RandomUnit(_seed) * Mathf.PI;
        var randomCirlce = new Vector2(Mathf.Sin(radin), Mathf.Cos(radin));
        return new Vector2(randomCirlce.x, randomCirlce.y);
    }
    
    public static int Random(this RangeInt ir, IRandomGenerator seed = null) => ir.start + RandomInt(ir.length, seed);
    public static float Random(this RangeFloat ir, IRandomGenerator seed = null) => ir.start + RandomFloat(ir.length, seed);
    public static int RandomIndex<T>(this List<T> randomList, IRandomGenerator seed = null) => RandomInt(randomList.Count - 1, seed);
    public static int RandomIndex<T>(this T[] _array, IRandomGenerator randomSeed = null) => RandomInt(_array.Length - 1, randomSeed);
    public static T RandomElement<T>(this List<T> _list, IRandomGenerator _random = null) => _list[_list.RandomIndex(_random)];
    public static T RandomElement<T>(this T[] array, IRandomGenerator _random = null) => array[array.RandomIndex(_random)];

    public static T RandomElement<T>(this T[,] array, IRandomGenerator _random = null) => array[RandomInt(array.GetLength(0) - 1, _random), RandomInt(array.GetLength(1) - 1, _random)];
    public static T RandomKey<T, Y>(this Dictionary<T, Y> dic, IRandomGenerator randomSeed = null) => dic.ElementAt(RandomInt(dic.Count - 1, randomSeed)).Key;
    public static Y RandomValue<T, Y>(this Dictionary<T, Y> dic, IRandomGenerator randomSeed = null) => dic.ElementAt(RandomInt(dic.Count - 1, randomSeed)).Value;
    public static bool RandomBool(IRandomGenerator seed = null) => RandomInt(1,seed) == 1;
    public static int RandomSign(IRandomGenerator seed = null) => RandomBool(seed) ? 1 : -1;
    public static Color RandomColor(IRandomGenerator seed = null, float alpha = -1) => new Color(Random01( seed), Random01( seed), Random01( seed), alpha < 0 ? Random01( seed) : alpha);
    public static int RandomPercentageInt(IRandomGenerator random = null) => RandomInt(100, random);
    public static float RandomPercentageFloat(IRandomGenerator random = null) => RandomFloat(100f, random);
    public static T RandomPercentage<T>(this Dictionary<T, int> percentageRate, IRandomGenerator seed) => RandomPercentage(percentageRate, default, seed);
    public static T RandomPercentage<T>(this Dictionary<T, int> percentageRate, T invalid = default, IRandomGenerator seed = null)
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
    public static T RandomPercentage<T>(this Dictionary<T, float> percentageRate, T invalid = default(T), IRandomGenerator seed = null)
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
    
    public static IEnumerable<T> RandomLoop<T>(this List<T> _items, IRandomGenerator _seed=null)
    {
        int count = _items.Count();
        int randomIndex = RandomInt(count - 1, _seed);
        for (int i = 0; i < count; i++)
            yield return _items[(randomIndex + i) % count];
    }

    public static IEnumerable<T> RandomLoop<T>(this T[] _items, IRandomGenerator _seed=null)
    {
        int count = _items.Count();
        int randomIndex = RandomInt(count - 1, _seed);
        for (int i = 0; i < count; i++)
            yield return _items[(randomIndex + i) % count];
        
    }
    public static T RandomEnumValues<T>(IRandomGenerator _seed = null) where T : Enum
    {
        Array allEnums = Enum.GetValues(typeof(T));
        int randomIndex = _seed?.NextInt(1, allEnums.Length) ?? UnityEngine.Random.Range(1, allEnums.Length);
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
    
    public static float3 RandomPerpendicular(float3 normal,IRandomGenerator _seed = null)
    {
        float3 randomVector;
        do {
            randomVector = RandomSphere(_seed);
        } while (umath.isParallel(normal, randomVector));
        return umath.calculatePerpendicular(randomVector, normal).normalize();
    }
}
