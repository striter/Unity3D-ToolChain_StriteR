using System;
using System.Collections.Generic;
using UnityEngine;
#region Unit
public class Ref<T> 
{
    public T Value=default;
    public override bool Equals(object obj) => this == (Ref<T>)obj;
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator !(Ref<T> refValue) => refValue is null;
    public static bool operator !=(Ref<T> _src, Ref<T> _tar) => !(_src == _tar);
    public static bool operator ==(Ref<T> _src,Ref<T> _tar)
    {
        if (!!_src && !!_tar)
            return _src.Value.Equals(_tar.Value);
        return !_src && !_tar;
    }
    public static bool operator true(Ref<T> refValue) => !!refValue;
    public static bool operator  false(Ref<T> refValue) => !refValue;
    public static implicit operator Ref<T>(T value)=>new Ref<T>() { Value=value};
    public static explicit operator T(Ref<T> refValue) => !refValue?default:refValue.Value;
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
    public T value1 { get; private set; }
    public ValueChecker(T _check)
    {
        value1 = _check;
    }

    public bool Check(T target)
    {
        if (value1.Equals(target))
            return false;
        value1 = target;
        return true;
    }
}
public class ValueChecker<T, Y> : ValueChecker<T>
{
    public Y value2 { get; private set; }
    public ValueChecker(T temp1, Y temp2) : base(temp1)
    {
        value2 = temp2;
    }

    public bool Check(T target1, Y target2)
    {
        bool check1 = Check(target1);
        bool check2 = Check(target2);
        return check1 || check2;
    }
    public bool Check(Y target2)
    {
        if (value2.Equals(target2))
            return false;
        value2 = target2;
        return true;
    }
}
public class Timer
{
    public float m_TimerDuration { get; private set; } = 0;
    public bool m_Timing { get; private set; } = false;
    public float m_TimeLeft { get; private set; } = -1;
    public float m_TimeElapsed => m_TimerDuration - m_TimeLeft;
    public float m_TimeLeftScale { get; private set; } = 0;
    protected virtual bool CheckTiming() => m_TimeLeft > 0;
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
        m_Timing = CheckTiming();
        m_TimeLeftScale = m_TimerDuration == 0 ? 0 : m_TimeLeft / m_TimerDuration;
        if (m_TimeLeftScale < 0)
            m_TimeLeftScale = 0;
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
public struct Matrix3x3
{
    public float m00, m01, m02;
    public float m10, m11, m12;
    public float m20, m21, m22;
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
}
[Serializable]
public struct Polygon
{
    public Vector3 m_Point0 => m_Points[0];
    public Vector3 m_Point1 => m_Points[1];
    public Vector3 m_Point2 => m_Points[2];
    public int m_Indice0=>m_Triangles[0];
    public int m_Indice1=>m_Triangles[1];
    public int m_Indice2=>m_Triangles[2];
    public Vector3[] m_Points;
    public int[] m_Triangles;
    public Polygon(Vector3 _point1, Vector3 _point2, Vector3 _point3, int _indice0, int _indice1, int _indice2)
    {
        m_Points = new Vector3[3] { _point1,_point2,_point3};
        m_Triangles = new int[3] { _indice0, _indice1, _indice2 };
    }
}
#endregion