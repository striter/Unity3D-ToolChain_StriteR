using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Extensions;

namespace Swizzling
{
    // public sealed class float2 : FloatSwizzling<float>
    // {
    //     public Vector4 m_Value { get; private set; }
    //     public float2(float x, float y) : base(new float[] { x, y }) { }
    //     protected override void OnValueChanged(float[] value) { m_Value = new Vector2(value[0], value[1]); }
    //     public static explicit operator Vector2(float2 src) { return src.m_Value; }
    // }
    // public sealed class float3 : FloatSwizzling<float>
    // {
    //     public Vector3 m_Value { get; private set; }
    //     public float3(float x, float y, float z) : base(new float[] { x, y, z }) { }
    //     protected override void OnValueChanged(float[] _value) => m_Value = new Vector3(_value[0], _value[1], _value[2]);
    //     public static explicit operator Vector3(float3 src) { return src.m_Value; }
    // }
    // public sealed class float4 : FloatSwizzling<float>
    // {
    //     public Vector4 m_Value { get; private set; }
    //     public float4(float x, float y, float z, float w) : base(new float[] { x, y, z, w }) { }
    //     protected override void OnValueChanged(float[] _value) => m_Value = new Vector4(_value[0], _value[1], _value[2], _value[3]);
    //     public static explicit operator Vector4(float4 src) { return src.m_Value; }
    // }
    
    public class FloatSwizzling<T> : DynamicObject
    {
        static readonly Dictionary<char, int> m_SwizzlingPositions = new Dictionary<char, int>() { { 'x', 0 }, { 'r', 0 }, { 'y', 1 }, { 'g', 1 }, { 'z', 2 }, { 'b', 2 }, { 'w', 3 }, { 'a', 3 } };
        T[] m_Value;
        public FloatSwizzling(T[] param) 
        { 
            m_Value = param; 
            OnValueChanged(m_Value);
        }
        public T this[int index] => m_Value[index];
        public T this[char index] => m_Value[m_SwizzlingPositions[index]];
        public override string ToString() => m_Value.ToString(',');
        protected virtual void OnValueChanged(T[] _value) { }
        void SafeCheck(string name)
        {
            foreach (char c in name)
            {
                if (!m_SwizzlingPositions.ContainsKey(c) | m_Value.Length - 1 < m_SwizzlingPositions[c])
                    throw new Exception("Invalid Swizzling Binding:" + c + "," + this.GetType());
            }
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            SafeCheck(binder.Name);
            if (binder.Name.Length == 1)
            {
                result = this[binder.Name[0]];
            }
            else
            {
                T[] resultArray = new T[binder.Name.Length];
                for (int i = 0; i < resultArray.Length; i++)
                    resultArray[i] = this[binder.Name[i]];
                result = resultArray;
            }
            return true;
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            SafeCheck(binder.Name);
            if (binder.Name.Length == 1)
            {
                if (!(value is T))
                    throw new Exception("Invalid Swizzling Gammer!");

                m_Value[m_SwizzlingPositions[binder.Name[0]]] = (T)value;
            }
            else
            {
                if (binder.Name.Length > m_Value.Length)
                    throw new Exception("Swizzling Can't Set More Element");

                foreach (char src in binder.Name)
                {
                    int count = 0;
                    foreach (char compare in binder.Name)
                        if (src == compare)
                            count++;
                    if (count > 1)
                        throw new Exception("Swizzling Can't Set Same Element!" + src + "," + this.GetType());
                }

                if (!(value is T[]))
                    throw new Exception("Invalid Swizzling Grammer!");

                T[] vectorValue = value as T[];
                for (int i = 0; i < binder.Name.Length; i++)
                    m_Value[m_SwizzlingPositions[binder.Name[i]]] = vectorValue[i];
            }
            OnValueChanged(m_Value);
            return true;
        }
    }
}