using System;
using System.Collections.Generic;
using UnityEngine;
#region Unit
[Serializable]
public class Ref<T>
{
    public T m_RefValue = default;
    public Ref() { }
    public Ref(T _refValue) { m_RefValue = _refValue; }
    public override bool Equals(object obj) => this == (Ref<T>)obj;
    public override int GetHashCode() => m_RefValue.GetHashCode();
    public static bool operator !=(Ref<T> _src, Ref<T> _tar) => !(_src == _tar);
    public static bool operator ==(Ref<T> _src,Ref<T> _tar)
    {
        if (_src!=null && _tar!=null)
            return _src.m_RefValue.Equals(_tar.m_RefValue);
        return _src==null && _tar==null;
    }
    public static implicit operator Ref<T>(T _value)=>new Ref<T>(_value) ;
    public static implicit operator T(Ref<T> _refValue) => _refValue.m_RefValue;
    public void SetValue(T _value) => m_RefValue = _value;
}

public class Instance<T> where T:class
{
    private T m_Instance;
    private readonly Func<T> GetInstance;
    public bool m_Instanced => m_Instance != null;

    public Instance(Func<T> _GetInstance)
    {
        GetInstance = _GetInstance;
    }

    public T m_Value
    {
        get
        {
            if (!m_Instanced)
                m_Instance = GetInstance();
            return m_Instance;
        }
    }

    public static implicit operator T(Instance<T> _instance) => _instance.m_Value;
}

public class ByteArray<T>
{
    T[] datas = new T[byte.MaxValue];
    public byte length = byte.MinValue;
    public T this[byte _index] => datas[length];

    public void AddLast(T _value)
    {
        if (length == byte.MaxValue)
            return;
        datas[length++] = _value;    
    }

    public void RemoveLast(T _value)
    {
        if (length == byte.MinValue)
            return;
        datas[length--] = default;
    }

    public void Set(byte _index, T _value) => datas[_index] = _value;
}


[Serializable]
public struct RangeFloat
{
    public float start;
    public float length;
    public float end => start + length;
    public RangeFloat(float _start, float _length)
    {
        start = _start;
        length = _length;
    }
}
[Serializable]
public struct RangeInt
{

    public int start;
    public int length;
    public int end => start + length;
    public RangeInt(int _start, int _length)
    {
        start = _start;
        length = _length;
    }
}
#endregion
#region ValueHelper
public class ValueLerpBase
{
    float m_check;
    float m_duration;
    protected float m_value { get; private set; }
    protected float m_previousValue { get; private set; }
    protected float m_targetValue { get; private set; }
    readonly Action<float> OnValueChanged;
    public ValueLerpBase(float startValue, Action<float> _OnValueChanged)
    {
        m_targetValue = startValue;
        m_previousValue = startValue;
        m_value = m_targetValue;
        OnValueChanged = _OnValueChanged;
        OnValueChanged(m_value);
    }

    protected void SetLerpValue(float value, float duration)
    {
        if (Math.Abs(value - m_targetValue) < float.Epsilon)
            return;
        m_duration = duration;
        m_check = m_duration;
        m_previousValue = m_value;
        m_targetValue = value;
    }

    public void SetFinalValue(float value)
    {
        if (Math.Abs(value - m_value) < float.Epsilon)
            return;
        m_value = value;
        m_previousValue = m_value;
        m_targetValue = m_value;
        OnValueChanged(m_value);
    }

    public void TickDelta(float deltaTime)
    {
        if (m_check <= 0)
            return;
        m_check -= deltaTime;
        m_value = GetValue(m_check / m_duration);
        OnValueChanged(m_value);
    }
    protected virtual float GetValue(float checkLeftParam)
    {
        Debug.LogError("Override This Please");
        return 0;
    }
}
public class ValueLerpSeconds : ValueLerpBase
{
    readonly float m_perSecondValue;
    readonly float m_maxDuration;
    float m_maxDurationValue;
    public ValueLerpSeconds(float startValue, float perSecondValue, float maxDuration, Action<float> _OnValueChanged) : base(startValue, _OnValueChanged)
    {
        m_perSecondValue = perSecondValue;
        m_maxDuration = maxDuration;
        m_maxDurationValue = m_perSecondValue * maxDuration;
    }

    public void SetLerpValue(float value) => SetLerpValue(value, Mathf.Abs(value - m_value) > m_maxDurationValue ? m_maxDuration : Mathf.Abs((value - m_value)) / m_perSecondValue);

    protected override float GetValue(float checkLeftParam) => Mathf.Lerp(m_previousValue, m_targetValue, 1 - checkLeftParam);
}

public class ValueChecker<T> 
{
    public T m_Value { get; private set; }
    Action<T> OnAvailableCheck;
    Action<T, T> OnAvailableCheckPreCur;
    public ValueChecker(T _default=default) { m_Value = _default; }
    public ValueChecker<T> Bind(Action<T,T> _OnAvailableCheckPreCur = null)
    {
        OnAvailableCheckPreCur = _OnAvailableCheckPreCur;
        return this;
    }
    public ValueChecker<T> Bind(Action<T> _OnAvailableCheck = null)
    {
        OnAvailableCheck = _OnAvailableCheck;
        return this;
    }

    public bool Check(T _value)
    {
        if (Equals(m_Value,_value))
                return false;
        Set(_value);
        return true;
    }
    public ValueChecker<T> Set(T _value)
    {
        T preValue = m_Value;
        m_Value = _value;
        OnAvailableCheckPreCur?.Invoke(preValue, _value);
        OnAvailableCheck?.Invoke(m_Value);
        return this;
    }
    public static implicit operator T(ValueChecker<T> checker) => checker.m_Value;
}
public class Ticker
{
    public float m_Duration { get; private set; }
    public float m_Elapsed { get; private set; }
    public float m_Tick { get; private set; }
    public float m_TickScale { get; private set; }
    public Ticker(float _tick)
    {
        m_Duration = _tick;
        Reset();
    }
    public void Reset()
    {
        m_Elapsed = 0;
        m_Tick = 0;
        m_TickScale = 0f;
    }
    
    public bool Tick(float _delta)
    {
        m_Elapsed += _delta;
        m_Tick += _delta;
        bool availableTick = false;
        if(m_Tick > m_Duration)
        {
            m_Tick -= m_Duration;
            availableTick = true;
        }
        m_TickScale = m_Tick / m_Duration;
        return availableTick;
    }
}
public class Counter
{
    public float m_TimerDuration { get; private set; } = 0;
    public bool m_Counting { get; private set; } = false;
    public float m_TimeLeft { get; private set; } = -1;
    public float m_TimeLeftScale { get; private set; } = 0;
    public float m_TimeElapsed { get; private set; }
    public float m_TimeElapsedScale { get; private set; } = 0;
    public Counter(float countDuration = 0, bool startOff = false)
    {
        Set(countDuration);
        if (startOff)
            Stop();
    }
    public void Set(float duration)
    {
        m_TimerDuration = duration;
        TickDelta(m_TimerDuration);
    }
    void TickDelta(float _timeCheck)
    {
        m_TimeLeft = Mathf.Clamp( _timeCheck,0f,m_TimerDuration);
        m_TimeElapsed = m_TimerDuration - m_TimeLeft;
        m_Counting = m_TimeLeft > 0;
        m_TimeLeftScale = m_TimeLeft / m_TimerDuration;
        m_TimeElapsedScale = 1f - m_TimeLeftScale;
    }
    public void Replay() => TickDelta(m_TimerDuration);
    public void Stop() => TickDelta(0);
    public bool Tick(float deltaTime)
    {
        if (m_TimeLeft <= 0)
            return false;
        TickDelta(m_TimeLeft - deltaTime);
        if (!m_Counting)
        {
            m_TimeLeft = 0;
            return true;
        }
        return false;
    }

    public bool TickValid(float deltaTime)
    {
        if (!m_Counting)
            return false;
        Tick(deltaTime);
        if (m_Counting)
            return false;
        return true;
    }
}
#endregion
#region Swizzling
[Serializable]
public struct Int2:IEquatable<Int2>, IEqualityComparer<Int2>
{
    public int x;
    public int y;
    public Int2(int _x, int _y) { x = _x; y = _y; }
    public static implicit operator (int, int)(Int2 int2) => (int2.x, int2.y);

    public static readonly Int2 One = new Int2(1, 1);
    public static readonly Int2 Zero = new Int2(0, 0);
    
    public static Int2 operator+(Int2 _src,Int2 _dst) =>new Int2(_src.x+_dst.x,_src.y+_dst.y);
    public static explicit operator Int2(Vector2 _src) =>new Int2((int)_src.x,(int)_src.y);
    public static Int2 Max(Int2 _src, Int2 _dst) => new Int2(Mathf.Max(_src.x,_dst.x),Mathf.Max(_src.y,_dst.y));
    public static Int2 Min(Int2 _src, Int2 _dst) => new Int2(Mathf.Min(_src.x,_dst.x),Mathf.Min(_src.y,_dst.y));
    public override string ToString() => $"{x},{y}";

    #region Implement
    public bool Equals(Int2 x, Int2 y)=> x.x == y.x && x.y == y.y;
    public bool Equals(Int2 other)=> x == other.x && y == other.y;
    public int GetHashCode(Int2 obj)
    {
        unchecked
        {
            return (obj.x * 397) ^ obj.y;
        }
    }
    public override bool Equals(object obj)
    {
        return obj is Int2 other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (x * 397) ^ y;
        }
    }
    #endregion
}

[Serializable]
public struct Int3 : IEquatable<Int3>
{
    public int x;
    public int y;
    public int z;
    public Int3(int _x, int _y, int _z) { x = _x; y = _y; z = _z; }
    public Int2 xy => new Int2(x, y);
    public Int2 xz => new Int2(x, z);
    
    public static readonly Int3 One = new Int3(1, 1,1);
    public static readonly Int3 Zero = new Int3(0, 0,0);
    public static readonly Int3 Right = new Int3(1, 0, 0);
    public static readonly Int3 Up = new Int3(0, 1, 0);
    public static readonly Int3 Forward = new Int3(0, 0, 1);
    public static Int3 operator +(Int3 _src, Int3 _dst) => new Int3(_src.x + _dst.x, _src.y + _dst.y, _src.z + _dst.z);
    public static Int3 operator -(Int3 _src, Int3 _dst) => new Int3(_src.x - _dst.x, _src.y - _dst.y, _src.z - _dst.z);
    public static bool operator ==(Int3 _src, Int3 _dst) => _src.x == _dst.x && _src.y == _dst.y && _src.z == _dst.z;
    public static bool operator !=(Int3 _src, Int3 _dst) => _src.x != _dst.x && _src.y != _dst.y && _src.z != _dst.z;
    
    public static Int3 operator -(Int3 _src) => new Int3(-_src.x,- _src.y, -_src.z);
    public static Int3 operator *(Int3 _src, int _scale) => new Int3(_src.x * _scale, _src.y * _scale, _src.z * _scale);

    public int Max() => Mathf.Max(x, y,z);
    #region Implement
    public bool Equals(Int3 other)
    {
        return x == other.x && y == other.y && z == other.z;
    }
    public override bool Equals(object obj)
    {
        return obj is Int3 other && Equals(other);
    }
    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = x;
            hashCode = (hashCode * 397) ^ y;
            hashCode = (hashCode * 397) ^ z;
            return hashCode;
        }
    }

    public override string ToString() => $"{x},{y},{z}";
    #endregion
}
[Serializable]
public struct Int4
{
    public int x;
    public int y;
    public int z;
    public int w;
    public Int4(int _x, int _y, int _z, int _w) { x = _x; y = _y; z = _z; w = _w; }
}

[Serializable]
public struct Matrix2x2
{
    public float m00, m01;
    public float m10, m11;

    public Matrix2x2(float _00,float _01,float _10,float _11)
    {
        m00 = _00;
        m01 = _01;
        m10 = _10;
        m11 = _11;
    }

    public (float x,float y) Multiply(float x, float y) => (
        x * m00 + y * m01,
        x * m10 + y * m11
    );

    public (float x,float y) InvMultiply(float x, float y) => (
        x * m00 + y * m10,
        x * m01 + y * m11
    );

    public (float x, float y) Multiply((float x, float y) float2) => Multiply(float2.x, float2.y);
    public (float x, float y) InvMultiply((float x, float y) float2) => InvMultiply(float2.x, float2.y);

    public Vector2 MultiplyVector(Vector2 _srcVector)
    {
        var float2=Multiply(_srcVector.x,_srcVector.y);
        return new Vector2(float2.x, float2.y);
    }

    public void Multiply(float _x,float _y,out float x,out float y)
    {
        x = _x * m00 + _y * m10;
        y = _x * m01 + _y * m11;
    }
    
    public override string ToString()=>$"{m00} {m01}\n{m10} {m11}";
    public static Matrix2x2 Identity = new Matrix2x2(1f, 0f, 0f, 1f);
}

[Serializable]
public struct Matrix3x3
{
    public float m00, m01, m02;
    public float m10, m11, m12;
    public float m20, m21, m22;
    public Matrix3x3(float _00, float _01, float _02, float _10, float _11, float _12, float _20, float _21, float _22) { m00 = _00; m01 = _01; m02 = _02; m10 = _10; m11 = _11; m12 = _12; m20 = _20; m21 = _21; m22 = _22; }

    public Vector3 MultiplyVector(Vector3 _srcVector) => new Vector3(
        _srcVector.x * m00 + _srcVector.y * m01 + _srcVector.z * m02,
        _srcVector.x * m10 + _srcVector.y * m11 + _srcVector.z * m12,
        _srcVector.x * m20 + _srcVector.y * m21 + _srcVector.z * m22);    
    public Vector3 InvMultiplyVector(Vector3 _srcVector) => new Vector3(
        _srcVector.x * m00 + _srcVector.y * m10 + _srcVector.z * m20,
        _srcVector.x * m01 + _srcVector.y * m11 + _srcVector.z * m21,
        _srcVector.x * m02 + _srcVector.y * m12 + _srcVector.z * m22);
    public static Vector3 operator *(Matrix3x3 _matrix, Vector3 _vector) => _matrix.MultiplyVector(_vector);
    public static Vector3 operator *(Vector3 _vector, Matrix3x3 matrix) => matrix.InvMultiplyVector(_vector);
    public void SetRow(int _index, Vector3 _row)
    {
        switch (_index)
        {
            default: throw new Exception("Invalid Row For Matrix3x3:" + _index.ToString());
            case 0: m00 = _row.x; m01 = _row.y; m02 = _row.z; break;
            case 1: m10 = _row.x; m11 = _row.y; m12 = _row.z; break;
            case 2: m20 = _row.x; m21 = _row.y; m22 = _row.z; break;
        }
    }
    public void SetColumn(int _index, Vector3 column)
    {
        switch (_index)
        {
            default: throw new Exception("Invalid Column For Matrix3x3:" + _index.ToString());
            case 0: m00 = column.x; m10 = column.y; m20 = column.z; break;
            case 1: m01 = column.x; m11 = column.y; m21 = column.z; break;
            case 2: m02 = column.x; m12 = column.y; m22 = column.z; break;
        }
    }
    public static readonly Matrix3x3 identity = new Matrix3x3() { m00 = 0, m01 = 0, m02 = 0, m10 = 0, m11 = 0, m12 = 0, m20 = 0, m21 = 0, m22 = 0 };
    public static explicit operator Matrix3x3(Matrix4x4 _srcMatrix) => new Matrix3x3(_srcMatrix.m00, _srcMatrix.m01, _srcMatrix.m02, _srcMatrix.m10, _srcMatrix.m11, _srcMatrix.m12, _srcMatrix.m20, _srcMatrix.m21, _srcMatrix.m22);
}
#endregion