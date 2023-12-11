using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Serialization;

[Serializable]
public class Ref<T> where T:struct
{
    [FormerlySerializedAs("m_RefValue")] public T value = default;
    public Ref() { }
    public Ref(T _refValue) { value = _refValue; }
    public override bool Equals(object obj) => this == (Ref<T>)obj;
    public override int GetHashCode() => value.GetHashCode();
    public static bool operator !=(Ref<T> _src, Ref<T> _tar) => !(_src == _tar);
    public static bool operator ==(Ref<T> _src,Ref<T> _tar)
    {
        if (_src!=null && _tar!=null)
            return _src.value.Equals(_tar.value);
        return _src==null && _tar==null;
    }
    public static implicit operator Ref<T>(T _value)=>new Ref<T>(_value) ;
    public static implicit operator T(Ref<T> _refValue) => _refValue.value;
    public void SetValue(T _value) => value = _value;
}

public class PassiveInstance<T>
{
    private T m_Instance;
    private readonly Func<T> CreateInstance;
    private readonly Action<T> DisposeInstance;
    public bool m_Instanced => m_Instance != null;

    public PassiveInstance(Func<T> _createInstance,Action<T> _DisposeInstance=null)
    {
        CreateInstance = _createInstance;
        DisposeInstance = _DisposeInstance;
    }

    public T Value
    {
        get
        {
            if (!m_Instanced)
                m_Instance = CreateInstance();
            return m_Instance;
        }
    }

    public void Dispose()
    {
        if (m_Instanced)
            DisposeInstance?.Invoke(m_Instance);
    }

    public static implicit operator T(PassiveInstance<T> _passiveInstance) => _passiveInstance.Value;
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

    public static readonly RangeFloat k01 = new RangeFloat(0f,1f);
    public float Clamp(float _value)=>math.clamp(_value,start,end);
    public bool Contains(float _check) => start <= _check && _check <= end;
    public float NormalizedAmount(float _check) => umath.invLerp(start, end, _check);
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

    public static RangeInt operator +(RangeInt _src, RangeInt _dst) => new RangeInt(_src.start+_dst.start,_src.length+_dst.length);
    public int GetValue(float _normalized) => (int) (start + length * _normalized);
    public int GetValueContains(float _normalized) => (int) (start + length * _normalized) + (_normalized>=.99f?1:0);
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
    public bool m_Playing { get; private set; } = false;
    public float m_TimeLeft { get; private set; } = -1;
    public float m_TimeLeftScale { get; private set; } = 0;
    public float m_TimeElapsed { get; private set; }
    public float m_TimeElapsedScale { get; private set; } = 0;
    public Counter(float _countDuration = 0, bool _startOff = false)
    {
        Set(_countDuration);
        if (_startOff)
            Stop();
    }
    void Validate(float _timeLeft)
    {
        m_TimeLeft = Mathf.Clamp( _timeLeft,0f,m_TimerDuration);
        m_TimeElapsed = m_TimerDuration - m_TimeLeft;
        m_Playing = m_TimeLeft > 0;
        m_TimeLeftScale = m_TimeLeft / m_TimerDuration;
        m_TimeElapsedScale = 1f - m_TimeLeftScale;
    }
    
    public void Set(float _duration)
    {
        m_TimerDuration = _duration;
        Validate(m_TimerDuration);
    }
    public void Tick(float _deltaTime) => Validate(m_TimeLeft - _deltaTime);
    public void Replay() => Validate(m_TimerDuration);
    public void Stop() => Validate(0);
    public bool TickTrigger(float _deltaTime)
    {
        if (!m_Playing)
            return false;
        Tick(_deltaTime);
        return !m_Playing;
    }
}

[Serializable]
public struct Int2:IEquatable<Int2>, IEqualityComparer<Int2>
{
    public int x;
    public int y;
    public Int2(int _x, int _y) { x = _x; y = _y; }
    public static implicit operator (int, int)(Int2 int2) => (int2.x, int2.y);

    public static Int2 operator -(Int2 a) => new Int2(-a.x, -a.y);
    public static bool operator ==(Int2 a, Int2 b) => a.x == b.x && a.y == b.y;
    public static bool operator !=(Int2 a, Int2 b) => a.x != b.x || a.y != b.y;
    public static Int2 operator -(Int2 a, Int2 b) => new Int2(a.x - b.x, a.y - b.y);
    public static Int2 operator +(Int2 a, Int2 b) => new Int2(a.x + b.x, a.y + b.y);
    public static Int2 operator *(Int2 a, Int2 b) => new Int2(a.x * b.x, a.y * b.y);
    public static Int2 operator /(Int2 a, Int2 b) => new Int2(a.x / b.x, a.y / b.y);

    public static Int2 operator *(Int2 a, int b) => new Int2(a.x * b, a.y * b);
    public static Int2 operator /(Int2 a, int b) => new Int2(a.x / b, a.y / b);
    public Int2 Inverse() => new Int2(y, x);
    public override string ToString() => x + "," + y;
    public int sqrMagnitude => x * x + y * y;
    
    public static readonly Int2 kZero = new Int2(0, 0);
    public static readonly Int2 kOne = new Int2(1, 1);
    public static readonly Int2 kNegOne = new Int2(-1, -1);
    public static readonly Int2 kBack = new Int2(0, -1);
    public static readonly Int2 kRight = new Int2(1, 0);
    public static readonly Int2 kLeft = new Int2(-1, 0);
    public static readonly Int2 kForward = new Int2(0, 1);

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
    public static readonly Int3 kRight = new Int3(1, 0, 0);
    public static readonly Int3 kLeft = new Int3(-1, 0, 0);
    public static readonly Int3 kUp = new Int3(0, 1, 0);
    public static readonly Int3 kDown = new Int3(0, -1, 0);
    public static readonly Int3 kForward = new Int3(0, 0, 1);
    public static readonly Int3 kBack = new Int3(0, 0, -1);
    public static Int3 operator +(Int3 _src, int _dst) => new Int3(_src.x + _dst, _src.y + _dst, _src.z + _dst);
    public static Int3 operator -(Int3 _src, int _dst) => new Int3(_src.x - _dst, _src.y - _dst, _src.z - _dst);
    public static Int3 operator %(Int3 _src, int _dst) => new Int3(_src.x % _dst, _src.y % _dst, _src.z % _dst);
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
