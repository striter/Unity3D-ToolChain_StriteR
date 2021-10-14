using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LinqExtension;

public static class UEnum
{
    private static class EnumStorage<T> where T:Enum
    {
        public static readonly T[] m_Values;
        public static readonly T m_Invalid;
        public static readonly Dictionary<int, T> m_ValueDic=new Dictionary<int, T>();
        public static readonly Dictionary<T, int> m_IndexDic = new Dictionary<T, int>();
        static EnumStorage()
        {
            List<T> values = new List<T>();
            int index = 0;
            foreach (var value in Enum.GetValues(typeof(T)))
            {
                var enumIndex = Convert.ToInt32(value);
                var enumObj = (T) value;
                if (enumIndex == -1 || value.ToString() == "Invalid")
                {
                    m_Invalid = enumObj;
                    continue;
                }
                values.Add(enumObj);
                m_ValueDic.Add(enumIndex,enumObj);
                m_IndexDic.Add(enumObj,index++);
            }

            m_Values = values.ToArray();
        }
    }

    public static T GetInvalid<T>() where T : Enum => EnumStorage<T>.m_Invalid;
    public static int GetIndex<T>(T _enum) where T:Enum=> EnumStorage<T>.m_IndexDic[_enum];
    public static int Count<T>() where T : Enum => EnumStorage<T>.m_Values.Length;
    public static T GetValue<T>(int _index) where T:Enum=> EnumStorage<T>.m_ValueDic[_index];
    public static T[] GetValues<T>() where T:Enum=> EnumStorage<T>.m_Values;
    public static T Next<T>(this T enumValue) where T : Enum
    {
        var allEnums = GetValues<T>();
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
    }
}