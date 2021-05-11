using System;
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
    Action<float> OnValueChanged;
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
        if (value == m_targetValue)
            return;
        m_duration = duration;
        m_check = m_duration;
        m_previousValue = m_value;
        m_targetValue = value;
    }

    public void SetFinalValue(float value)
    {
        if (value == m_value)
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
    float m_perSecondValue;
    float m_maxDuration;
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

public class Timer
{
    public float m_TimerDuration { get; private set; } = 0;
    public bool m_Timing { get; private set; } = false;
    public float m_TimeLeft { get; private set; } = -1;
    public float m_TimeLeftScale { get; private set; } = 0;
    public float m_TimeElapsed { get; private set; }
    public float m_TimeElapsedScale { get; private set; } = 0;
    public Timer(float countDuration = 0, bool startOff = false)
    {
        Set(countDuration);
        if (startOff)
            Stop();
    }
    public void Set(float duration)
    {
        m_TimerDuration = duration;
        OnTimeCheck(m_TimerDuration);
    }
    void OnTimeCheck(float _timeCheck)
    {
        m_TimeLeft = _timeCheck;
        m_TimeElapsed = m_TimerDuration - m_TimeLeft;
        m_Timing = m_TimeLeft > 0;
        m_TimeLeftScale = Mathf.Max(m_TimerDuration == 0 ? 0 : m_TimeLeft / m_TimerDuration, 0);
        m_TimeElapsedScale = 1f - m_TimeLeftScale;
    }
    public void Replay() => OnTimeCheck(m_TimerDuration);
    public void Stop() => OnTimeCheck(0);
    public void Tick(float deltaTime)
    {
        if (m_TimeLeft <= 0)
            return;
        OnTimeCheck(m_TimeLeft - deltaTime);
        if (!m_Timing)
            m_TimeLeft = 0;
    }
}
#endregion
#region Render
[Serializable]
public struct Matrix3x3
{
    public float m00, m01, m02;
    public float m10, m11, m12;
    public float m20, m21, m22;
    public Matrix3x3(float _00,float _01,float _02,float _10,float _11,float _12,float _20,float _21,float _22) { m00 = _00;m01 = _01;m02 = _02;m10 = _10;m11 = _11;m12 = _12;m20 = _20;m21 = _21;m22 = _22; }
    public Vector3 InvMultiplyVector(Vector3 _srcVector) => new Vector3(
        _srcVector.x * m00 + _srcVector.y * m10 + _srcVector.z * m20,
        _srcVector.x * m01 + _srcVector.y * m11 + _srcVector.z * m21,
        _srcVector.x * m02 + _srcVector.y * m12 + _srcVector.z * m22);
    public Vector3 MultiplyVector(Vector3 _srcVector) => new Vector3(
        _srcVector.x * m00 + _srcVector.y * m01 + _srcVector.z * m02,
        _srcVector.x * m10 + _srcVector.y * m11 + _srcVector.z * m12,
        _srcVector.x * m20 + _srcVector.y * m21 + _srcVector.z * m22);
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

[Serializable]
public struct Triangle
{
    public Vector3 m_Vertex1;
    public Vector3 m_Vertex2;
    public Vector3 m_Vertex3;
    public Vector3[] m_Verticies { get; private set; }
    public Vector3[] GetDrawLinesVerticies() => new Vector3[] { m_Vertex1, m_Vertex2, m_Vertex3, m_Vertex1};
    public Vector3 this[int index]
    {
        get
        {
            switch(index)
            {
                default:Debug.LogError( "Invalid Index:" + index);return m_Vertex1;
                case 0:return m_Vertex1;
                case 1:return m_Vertex2;
                case 2:return m_Vertex3;
            }
        }
    }
    public Triangle(Vector3[] _verticies):this(_verticies[0],_verticies[1],_verticies[2])
    {
        Debug.Assert(_verticies.Length!=3,"Triangles' Vertices Count Must Equals 3!");
    }
    public Triangle(Vector3 _vertex1,Vector3 _vertex2,Vector3 _vertex3)
    {
        m_Vertex1 = _vertex1;
        m_Vertex2 = _vertex2;
        m_Vertex3 = _vertex3;
        m_Verticies = new Vector3[] { _vertex1, _vertex2, _vertex3 };
    }
}

[Serializable]
public struct DirectedTriangle
{
    public Triangle m_Triangle;
    public Vector3 m_UOffset => m_Triangle.m_Vertex2 - m_Triangle.m_Vertex1;
    public Vector3 m_VOffset => m_Triangle.m_Vertex3 - m_Triangle.m_Vertex1;
    public Vector3 m_Normal => Vector3.Cross(m_UOffset, m_VOffset);
    public Vector3 this[int index]=>m_Triangle[index];
    public DirectedTriangle(Vector3 _vertex1, Vector3 _vertex2, Vector3 _vertex3)
    {
        m_Triangle = new Triangle(_vertex1, _vertex2, _vertex3);
    }
    public Vector3 GetUVPoint(Vector2 uv) => (1f - uv.x - uv.y) * m_Triangle.m_Vertex1 + uv.x * m_UOffset + uv.y * m_VOffset;
}
[Serializable]
public struct MeshPolygon
{
    public int m_Indice0=>m_Indices[0];
    public int m_Indice1=>m_Indices[1];
    public int m_Indice2=>m_Indices[2];
    public int[] m_Indices;
    public MeshPolygon(int _indice0, int _indice1, int _indice2)
    {
        m_Indices = new int[3] { _indice0, _indice1, _indice2 };
    }
    public Triangle GetTriangle(Vector3[] verticies) => new Triangle(verticies[m_Indice0], verticies[m_Indice1], verticies[m_Indice2]);
    public DirectedTriangle GetDirectedTriangle(Vector3[] verticies)=>new DirectedTriangle(verticies[m_Indice0], verticies[m_Indice1], verticies[m_Indice2]);
}

[Serializable]
public struct DistancePlane
{
    public Vector3 m_Normal;
    public float m_Distance;
    public DistancePlane(Vector3 _normal,float _distance) { m_Normal = _normal;m_Distance = _distance; }
}

[Serializable]
public struct Int2
{
    public int m_X;
    public int m_Y;
    public Int2(int _x, int _y) { m_X = _x; m_Y = _y;  }
}
[Serializable]
public struct Int3
{

    public int m_X;
    public int m_Y;
    public int m_Z;
    public Int3(int _x, int _y, int _z) { m_X = _x; m_Y = _y; m_Z = _z;  }
}
[Serializable]
public struct Int4
{
    public int m_X;
    public int m_Y;
    public int m_Z;
    public int m_W;
    public Int4(int _x,int _y,int _z,int _w) { m_X = _x;m_Y = _y;m_Z = _z;m_W = _w; }
}
#endregion