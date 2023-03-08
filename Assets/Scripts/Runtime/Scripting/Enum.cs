using System;
using System.Collections.Generic;
using System.Linq;

public static class UEnum
{
    private static class EnumStorage<T> where T:Enum
    {
        public static readonly T m_Invalid;
        public static readonly T[] m_Values;
        public static readonly Dictionary<T, int> m_EnumToIndex = new Dictionary<T, int>();
        public static readonly Dictionary<T, int> m_EnumToValue = new Dictionary<T, int>();
        public static readonly Dictionary<int, T> m_ValueToEnum = new Dictionary<int, T>();
        
        static EnumStorage()
        {
            List<T> values = new List<T>();
            int index = 0;
            foreach (var value in Enum.GetValues(typeof(T)))
            {
                var enumValue = Convert.ToInt32(value);
                var enumObj = (T) value;
                if (enumValue == -1)
                {
                    m_Invalid = enumObj;
                    continue;
                }
                values.Add(enumObj);
                m_ValueToEnum.Add(enumValue,enumObj);
                m_EnumToValue.Add(enumObj,enumValue);
                m_EnumToIndex.Add(enumObj,index++);
            }

            m_Values = values.ToArray();
        }
    }

    public static int Count<T>() where T : Enum => EnumStorage<T>.m_Values.Length;
    public static T GetInvalid<T>() where T : Enum => EnumStorage<T>.m_Invalid;
    public static int GetIndex<T>(T _enum) where T:Enum=> EnumStorage<T>.m_EnumToIndex[_enum];
    public static T IndexToEnum<T>(int _index) where T : Enum => GetEnums<T>()[_index];
    public static T GetEnum<T>(int _index) where T:Enum=> EnumStorage<T>.m_Values[_index];
    public static T[] GetEnums<T>() where T:Enum=> EnumStorage<T>.m_Values;
    public static int GetValue<T>(T _enum) where T : Enum => EnumStorage<T>.m_EnumToValue[_enum];
    public static T Next<T>(this T _enumValue) where T : Enum
    {
        var allEnums = GetEnums<T>();
        if (allEnums.Length<2)
            throw new Exception("Invalid Enum Type Next:"+typeof(T));

        int index = allEnums.FindIndex(p => p.Equals(_enumValue));
        return allEnums[(index + 1) % allEnums.Length];
    }

    private static bool IsFlagEnable(int _flags,int _compare) => (_flags&_compare) == _compare;
    public static bool IsFlagEnable<T>(this T _flags, T _compare) where T : Enum => IsFlagEnable(Convert.ToInt32(_flags),  GetValue(_compare));
    public static bool IsFlagEnable<T>(this T _flags, int _compare) where T : Enum => IsFlagEnable(Convert.ToInt32(_flags), _compare);
    
    public static bool IsFlagClear<T>(this T _flags) where T : Enum => Convert.ToInt32(_flags) == 0;
    public static IEnumerable<bool> GetNumerable<T>(this T _flags) where T:Enum
    {
        int flagValues = Convert.ToInt32(_flags);
        int maxPower=Convert.ToInt32( Enum.GetValues(typeof(T)).Cast<T>().Max());
        for(int i=0;i<32 ;i++ )
        {
            int curPower = umath.pow(2,i);
            if (curPower > maxPower)
                yield break;
            yield return (flagValues&curPower)==curPower;
        }
    }

    public static T CreateFlags<T>(params T[] _enums) where T:Enum
    {
        int value = 0;
        for (int i = 0; i < _enums.Length; i++)
            value += GetValue(_enums[i]);
        return (T)Enum.ToObject(typeof(T),value);
    }
}