using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Ref<T> where T:struct
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

    public T m_Value
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

    public static implicit operator T(PassiveInstance<T> _passiveInstance) => _passiveInstance.m_Value;
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

[Serializable]
public struct Matrix2x2
{
    public float m00, m01;
    public float m10, m11;
    public float determinant;
    public Matrix2x2(float _00,float _01,float _10,float _11)
    {
        m00 = _00;
        m01 = _01;
        m10 = _10;
        m11 = _11;
        determinant = m00 * m11 - m01 * m10;
    }

    public readonly (float x,float y) Multiply(float x, float y) => (
        x * m00 + y * m01,
        x * m10 + y * m11
    );

    public readonly (float x,float y) InvMultiply(float x, float y) => (
        x * m00 + y * m10,
        x * m01 + y * m11
    );

    public readonly (float x, float y) Multiply((float x, float y) float2) => Multiply(float2.x, float2.y);
    public readonly (float x, float y) InvMultiply((float x, float y) float2) => InvMultiply(float2.x, float2.y);

    public readonly Vector2 MultiplyVector(Vector2 _srcVector)
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
    public float determinant;
    public Matrix3x3(float _00, float _01, float _02,
        float _10, float _11, float _12,
        float _20, float _21, float _22)
    {
        m00 = _00; m01 = _01; m02 = _02; 
        m10 = _10; m11 = _11; m12 = _12; 
        m20 = _20; m21 = _21; m22 = _22;
        determinant = m00 * (m11 * m22 - m12 * m21) 
                    - m01 * (m10 * m22 - m12 * m20) 
                    + m02 * (m10 * m21 - m20 * m21);
    }

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
    public void SetColumn(int _index, Vector3 _column)
    {
        switch (_index)
        {
            default: throw new Exception("Invalid Column For Matrix3x3:" + _index.ToString());
            case 0: m00 = _column.x; m10 = _column.y; m20 = _column.z; break;
            case 1: m01 = _column.x; m11 = _column.y; m21 = _column.z; break;
            case 2: m02 = _column.x; m12 = _column.y; m22 = _column.z; break;
        }
    }
    public static readonly Matrix3x3 kIdentity = new Matrix3x3() { m00 = 0, m01 = 0, m02 = 0, m10 = 0, m11 = 0, m12 = 0, m20 = 0, m21 = 0, m22 = 0 };
    public static explicit operator Matrix3x3(Matrix4x4 _srcMatrix) => new Matrix3x3(_srcMatrix.m00, _srcMatrix.m01, _srcMatrix.m02, _srcMatrix.m10, _srcMatrix.m11, _srcMatrix.m12, _srcMatrix.m20, _srcMatrix.m21, _srcMatrix.m22);
}

[Serializable]
public struct Matrix3x4     //To be continued
{
    public float m00, m01, m02, m03;
    public float m10, m11, m12, m13;
    public float m20, m21, m22, m23;
    public Matrix3x4(float _00, float _01, float _02,float _03,
        float _10, float _11, float _12, float _13,
        float _20, float _21, float _22, float _23)
    { 
        m00 = _00; m01 = _01; m02 = _02; m03 = _03;
        m10 = _10; m11 = _11; m12 = _12; m13 = _13;
        m20 = _20; m21 = _21; m22 = _22; m23 = _23;
    }

    public static Vector3 operator *(Matrix3x4 _matrix, Vector3 _vector) => _matrix.MultiplyVector(_vector);
    public Vector3 MultiplyVector(Vector3 _srcVector) => new Vector3(
        _srcVector.x * m00 + _srcVector.y * m01 + _srcVector.z * m02,
        _srcVector.x * m10 + _srcVector.y * m11 + _srcVector.z * m12,
        _srcVector.x * m20 + _srcVector.y * m21 + _srcVector.z * m22);

    public static Vector3 operator *(Matrix3x4 _matrix, Vector4 _vector)
    {
        return new Vector4(_vector.x * _matrix.m00 + _vector.y * _matrix.m01 + _vector.z * _matrix.m02 + _vector.w*_matrix.m03,
                                    _vector.x * _matrix.m10 + _vector.y * _matrix.m11 + _vector.z * _matrix.m12 + _vector.w*_matrix.m13,
                                    _vector.x * _matrix.m20 + _vector.y * _matrix.m21 + _vector.z * _matrix.m22 + _vector.w*_matrix.m23);    
    }
    public Vector3 MultiplyPoint(Vector3 _position) => new Vector3(
        _position.x * m00 + _position.y * m01 + _position.z * m02 + m03,
        _position.x * m10 + _position.y * m11 + _position.z * m12 + m13,
        _position.x * m20 + _position.y * m21 + _position.z * m22 + m23);

    public static Matrix3x4 TS(Vector3 _t, Vector3 _s) => new Matrix3x4(
        _t.x,0f,0f,_s.x,
        0f,_t.y,0f,_s.y,
        0f,0f,_t.z,_s.z
    );
}