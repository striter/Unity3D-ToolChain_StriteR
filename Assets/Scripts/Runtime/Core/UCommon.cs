using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public static class UCommon
{
    public static readonly RangeFloat m_Range01 = new RangeFloat(0f, 1f);
    public static readonly RangeFloat m_RangeNeg1Pos1 = new RangeFloat(-1f, 2f);
    public static bool InRange(this RangeFloat _value, float _check) => _value.start <= _check && _check <= _value.end;
    public static float InRangeScale(this RangeFloat _value, float _check) => Mathf.InverseLerp(_value.start, _value.end, _check);
    public static readonly Dictionary<Type, Array> m_Enums = new Dictionary<Type, Array>();
    public static IEnumerable<object> GetEnumValues(Type _enumType)
    {
        if(!m_Enums.ContainsKey(_enumType))
            m_Enums.Add(_enumType,Enum.GetValues(_enumType));
        
        foreach(Enum enumObj in m_Enums[_enumType])
        {
            if ( Convert.ToInt32(enumObj) == -1||_enumType.ToString() == "Invalid" )
                continue;
            yield return enumObj;
        }
    }
    public static IEnumerable<T> GetEnumValues<T>()
    {
        foreach (object enumObj in GetEnumValues(typeof(T)))
            yield return (T)enumObj;
    }
    public static T Next<T>(this T enumValue) where T : Enum
    {
        T[] allEnums = (T[])Enum.GetValues(typeof(T));
        if (allEnums.Length<2)
            throw new Exception("Invalid Enum Type Next:"+typeof(T));

        int index = allEnums.FindIndex(p => p.Equals(enumValue));
        return allEnums[(index + 1) % allEnums.Length];
    }

    public static bool IsFlagEnable<T>(this T _flag,T _compare) where T:Enum
    {
        int srcFlag = Convert.ToInt32(_flag);
        int compareFlag = Convert.ToInt32(_compare);
        return (srcFlag&compareFlag)== compareFlag;
    }
    public static bool IsFlagClear<T>(this T _flag) where T : Enum => Convert.ToInt32(_flag) == 0;
    public static IEnumerable<bool> GetNumerable<T>(this T _flags) where T:Enum
    {
        int flagValues =Convert.ToInt32(_flags);
        int maxPower=Convert.ToInt32( Enum.GetValues(typeof(T)).Cast<T>().Max());
        for(int i=0;i<32 ;i++ )
        {
            int curPower = UMath.Pow(2,i);
            if (curPower > maxPower)
                yield break;
            yield return (flagValues&curPower)==curPower;
        }
        yield break;
    }
}

