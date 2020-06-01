using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.U2D;
using UnityEngine.UI;

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

public class TimerBase
{
    public float m_TimerDuration { get; private set; } = 0;
    public bool m_Timing { get; private set; } = false;
    public float m_TimeCheck { get; private set; } = -1;
    public float m_TimeLeftScale { get; private set; } = 0;
    public TimerBase(float duration = 0,bool startOff=false) {
        SetTimerDuration(duration);
        if (startOff)
            Stop();
    }
    public void SetTimerDuration(float duration)
    {
        m_TimerDuration = duration;
        OnTimeCheck(m_TimerDuration);
    }

    void OnTimeCheck(float _timeCheck)
    {
        m_TimeCheck = _timeCheck;
        m_Timing = m_TimeCheck > 0;
        m_TimeLeftScale = m_TimerDuration == 0 ? 0 : m_TimeCheck / m_TimerDuration;
        if (m_TimeLeftScale < 0)
            m_TimeLeftScale = 0;
    }

    public void Replay() => OnTimeCheck(m_TimerDuration);
    public void Stop() => OnTimeCheck(0);

    public void Tick(float deltaTime)
    {
        if (m_TimeCheck <= 0)
            return;
        OnTimeCheck(m_TimeCheck-deltaTime);
        if (!m_Timing)
            m_TimeCheck = 0;
    }
}

public class ExpRankBase
{
    public int m_Rank { get; private set; }
    public int m_TotalExpOwned { get; private set; }
    public int m_ExpCurRankOwned { get; private set; }
    public int m_ExpCurRankRequired { get; private set; }
    public int m_ExpLeftToNextRank => m_ExpCurRankRequired - m_ExpCurRankOwned;
    public float m_ExpCurRankScale => m_ExpCurRankOwned / (float)m_ExpCurRankRequired;
    Func<int, int> GetExpToNextLevel;
    public ExpRankBase(Func<int, int> GetExpToNextLevel)
    {
        this.GetExpToNextLevel = GetExpToNextLevel;
        m_TotalExpOwned = 0;
        m_Rank = 0;
        m_ExpCurRankOwned = 0;
    }
    public void OnExpSet(int totalExp)
    {
        m_TotalExpOwned = 0;
        m_Rank = 0;
        m_ExpCurRankOwned = 0;
        OnExpGainCheckLevelOffset(totalExp);
    }

    public int OnExpGainCheckLevelOffset(int exp)
    {
        int startRank = m_Rank;
        m_TotalExpOwned += exp;
        m_ExpCurRankOwned += exp;
        for (; ; )
        {
            m_ExpCurRankRequired = GetExpToNextLevel(m_Rank);
            if (m_ExpCurRankOwned < m_ExpCurRankRequired)
                break;
            m_ExpCurRankOwned -= m_ExpCurRankRequired;
            m_Rank++;
        }
        return m_Rank - startRank;
    }
}

public static class TimeScaleController<T> where T:struct
{
    static Dictionary<T, float> m_TimeScales=new Dictionary<T, float>();
    public static void Clear() => m_TimeScales.Clear();

    static float GetLowestScale()
    {
        float scale = 1f;
        m_TimeScales.Traversal((float value) => { if (scale > value) scale = value; });
        return scale;
    }

    public static float GetScale(T index) => m_TimeScales.ContainsKey(index) ? m_TimeScales[index] : 1f;
    public static void SetScale(T scaleIndex,float scale)
    {
        if (!m_TimeScales.ContainsKey(scaleIndex))
            m_TimeScales.Add(scaleIndex,1f);
        m_TimeScales[scaleIndex] = scale;
    }
    static ValueChecker<float> m_BulletTimeChecker = new ValueChecker<float>(1f);

    public static void Tick()
    {
        if (m_BulletTimeChecker.Check(GetLowestScale()))
            Time.timeScale = m_BulletTimeChecker.value1;
    }
}

#region UI Classes
public class AtlasLoader
{
    protected Dictionary<string, Sprite> m_SpriteDic { get; private set; } = new Dictionary<string, Sprite>();
    public bool Contains(string name) => m_SpriteDic.ContainsKey(name);
    public string m_AtlasName { get; private set; }
    public Sprite this[string name]
    {
        get
        {
            if (!m_SpriteDic.ContainsKey(name))
            {
                Debug.LogWarning("Null Sprites Found |" + name + "|"+m_AtlasName);
                return m_SpriteDic.Values.First();
            }
            return m_SpriteDic[name];
        }
    }
    public AtlasLoader(SpriteAtlas atlas)
    {
        m_AtlasName = atlas.name;
        Sprite[] allsprites=new Sprite[atlas.spriteCount];
        atlas.GetSprites(allsprites);
        allsprites.Traversal((Sprite sprite)=> { string name = sprite.name.Replace("(Clone)", ""); m_SpriteDic.Add(name, sprite); });
    }
}

public class AtlasAnim:AtlasLoader
{
    int animIndex=0;
    List<Sprite> m_Anims;
    public AtlasAnim(SpriteAtlas atlas):base(atlas)
    {
        m_Anims = m_SpriteDic.Values.ToList();
        m_Anims.Sort((a,b) =>
        {
            int index1 = int.Parse(System.Text.RegularExpressions.Regex.Replace(a.name, @"[^0-9]+", ""));
            int index2 = int.Parse(System.Text.RegularExpressions.Regex.Replace(b.name, @"[^0-9]+", ""));
            return   index1- index2;
        });
    }

    public Sprite Reset()
    {
        animIndex = 0;
        return m_Anims[animIndex];
    }

    public Sprite Tick()
    {
        animIndex++;
        if (animIndex == m_Anims.Count)
            animIndex = 0;
        return m_Anims[animIndex];
    }
}

class EnumSelection : TReflection.UI.CPropertyFillElement
{
    Text m_Text;
    ObjectPoolListComponent<int, Button> m_ChunkButton;
    public EnumSelection(Transform transform) : base(transform)
    {
        m_Text = transform.Find("Text").GetComponent<Text>();
        m_ChunkButton = new ObjectPoolListComponent<int, Button>(transform.Find("Grid"), "GridItem");
        transform.GetComponent<Button>().onClick.AddListener(() => {
            m_ChunkButton.transform.SetActivate(!m_ChunkButton.transform.gameObject.activeSelf);
        });
        m_ChunkButton.transform.SetActivate(false);
    }

    public void Init<T>(T defaultValue, Action<int> OnClick)
    {
        m_Text.text = defaultValue.ToString();
        m_ChunkButton.Clear();
        TCommon.TraversalEnum((T temp) =>
        {
            int index = (int)((object)temp);
            Button btn = m_ChunkButton.AddItem(index);
            btn.onClick.RemoveAllListeners();
            btn.GetComponentInChildren<Text>().text = temp.ToString();
            btn.onClick.AddListener(() => {
                m_Text.text = temp.ToString();
                OnClick(index);
                m_ChunkButton.transform.SetActivate(false);
            });
        });
    }
}
#endregion

#if UNITY_EDITOR
#region GizmosExtend
public static class Gizmos_Extend
{
    public static void DrawWireCapsule(Vector3 _pos, Quaternion _rot, Vector3 _scale, float _radius, float _height)
    {
        using (new UnityEditor.Handles.DrawingScope(Gizmos.color, Matrix4x4.TRS(_pos, _rot, _scale)))
        {
            if (_height > _radius * 2)
            {
                Vector3 offsetPoint = Vector3.up * (_height - (_radius * 2)) / 2;

                UnityEditor.Handles.DrawWireArc(offsetPoint, Vector3.forward, Vector3.right, 180, _radius);
                UnityEditor.Handles.DrawWireArc(offsetPoint, Vector3.right, Vector3.forward, -180, _radius);
                UnityEditor.Handles.DrawWireArc(-offsetPoint, Vector3.forward, Vector3.right, -180, _radius);
                UnityEditor.Handles.DrawWireArc(-offsetPoint, Vector3.right, Vector3.forward, 180, _radius);

                UnityEditor.Handles.DrawWireDisc(offsetPoint, Vector3.up, _radius);
                UnityEditor.Handles.DrawWireDisc(-offsetPoint, Vector3.up, _radius);

                UnityEditor.Handles.DrawLine(offsetPoint + Vector3.left * _radius, -offsetPoint + Vector3.left * _radius);
                UnityEditor.Handles.DrawLine(offsetPoint - Vector3.left * _radius, -offsetPoint - Vector3.left * _radius);
                UnityEditor.Handles.DrawLine(offsetPoint + Vector3.forward * _radius, -offsetPoint + Vector3.forward * _radius);
                UnityEditor.Handles.DrawLine(offsetPoint - Vector3.forward * _radius, -offsetPoint - Vector3.forward * _radius);
            }
            else
            {
                UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.up, _radius);
                UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.right, _radius);
                UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.forward, _radius);
            }
        }
    }

    public static void DrawWireCube(Vector3 _pos, Quaternion _rot, Vector3 _cubeSize)
    {
        using (new UnityEditor.Handles.DrawingScope(Gizmos.color, Matrix4x4.TRS(_pos, _rot, UnityEditor.Handles.matrix.lossyScale)))
        {
            float halfWidth, halfHeight, halfLength;
            halfWidth = _cubeSize.x / 2;
            halfHeight = _cubeSize.y / 2;
            halfLength = _cubeSize.z / 2;

            UnityEditor.Handles.DrawLine(new Vector3(halfWidth, halfHeight, halfLength), new Vector3(-halfWidth, halfHeight, halfLength));
            UnityEditor.Handles.DrawLine(new Vector3(halfWidth, halfHeight, -halfLength), new Vector3(-halfWidth, halfHeight, -halfLength));
            UnityEditor.Handles.DrawLine(new Vector3(halfWidth, -halfHeight, halfLength), new Vector3(-halfWidth, -halfHeight, halfLength));
            UnityEditor.Handles.DrawLine(new Vector3(halfWidth, -halfHeight, -halfLength), new Vector3(-halfWidth, -halfHeight, -halfLength));

            UnityEditor.Handles.DrawLine(new Vector3(halfWidth, halfHeight, halfLength), new Vector3(halfWidth, -halfHeight, halfLength));
            UnityEditor.Handles.DrawLine(new Vector3(-halfWidth, halfHeight, halfLength), new Vector3(-halfWidth, -halfHeight, halfLength));
            UnityEditor.Handles.DrawLine(new Vector3(halfWidth, halfHeight, -halfLength), new Vector3(halfWidth, -halfHeight, -halfLength));
            UnityEditor.Handles.DrawLine(new Vector3(-halfWidth, halfHeight, -halfLength), new Vector3(-halfWidth, -halfHeight, -halfLength));

            UnityEditor.Handles.DrawLine(new Vector3(halfWidth, halfHeight, halfLength), new Vector3(halfWidth, halfHeight, -halfLength));
            UnityEditor.Handles.DrawLine(new Vector3(-halfWidth, halfHeight, halfLength), new Vector3(-halfWidth, halfHeight, -halfLength));
            UnityEditor.Handles.DrawLine(new Vector3(halfWidth, -halfHeight, halfLength), new Vector3(halfWidth, -halfHeight, -halfLength));
            UnityEditor.Handles.DrawLine(new Vector3(-halfWidth, -halfHeight, halfLength), new Vector3(-halfWidth, -halfHeight, -halfLength));
        }
    }
    public static void DrawArrow(Vector3 _pos, Quaternion _rot, Vector3 _arrowSize)
    {
        using (new UnityEditor.Handles.DrawingScope(Gizmos.color, Matrix4x4.TRS(_pos, _rot, UnityEditor.Handles.matrix.lossyScale)))
        {
            Vector3 capBottom = Vector3.forward * _arrowSize.z / 2;
            Vector3 capTop = Vector3.forward * _arrowSize.z;
            float rootRadius = _arrowSize.x / 4;
            float capBottomSize = _arrowSize.x / 2;
            UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.forward, rootRadius);
            UnityEditor.Handles.DrawWireDisc(capBottom, Vector3.forward, rootRadius);
            UnityEditor.Handles.DrawLine(Vector3.up * rootRadius, capBottom + Vector3.up * rootRadius);
            UnityEditor.Handles.DrawLine(-Vector3.up * rootRadius, capBottom - Vector3.up * rootRadius);
            UnityEditor.Handles.DrawLine(Vector3.right * rootRadius, capBottom + Vector3.right * rootRadius);
            UnityEditor.Handles.DrawLine(-Vector3.right * rootRadius, capBottom - Vector3.right * rootRadius);

            UnityEditor.Handles.DrawWireDisc(capBottom, Vector3.forward, capBottomSize);
            UnityEditor.Handles.DrawLine(capBottom + Vector3.up * capBottomSize, capTop);
            UnityEditor.Handles.DrawLine(capBottom - Vector3.up * capBottomSize, capTop);
            UnityEditor.Handles.DrawLine(capBottom + Vector3.right * capBottomSize, capTop);
            UnityEditor.Handles.DrawLine(capBottom + -Vector3.right * capBottomSize, capTop);
        }
    }
    public static void DrawCylinder(Vector3 _pos, Quaternion _rot, float _radius, float _height)
    {
        using (new UnityEditor.Handles.DrawingScope(Gizmos.color, Matrix4x4.TRS(_pos, _rot, UnityEditor.Handles.matrix.lossyScale)))
        {
            Vector3 top = Vector3.forward * _height;

            UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.forward, _radius);
            UnityEditor.Handles.DrawWireDisc(top, Vector3.forward, _radius);

            UnityEditor.Handles.DrawLine(Vector3.right * _radius, top + Vector3.right * _radius);
            UnityEditor.Handles.DrawLine(-Vector3.right * _radius, top - Vector3.right * _radius);
            UnityEditor.Handles.DrawLine(Vector3.up * _radius, top + Vector3.up * _radius);
            UnityEditor.Handles.DrawLine(-Vector3.up * _radius, top - Vector3.up * _radius);
        }
    }
    public static void DrawTrapezium(Vector3 _pos, Quaternion _rot, Vector4 trapeziumInfo)
    {
        using (new UnityEditor.Handles.DrawingScope(Gizmos.color, Matrix4x4.TRS(_pos, _rot, UnityEditor.Handles.matrix.lossyScale)))
        {
            Vector3 backLeftUp = -Vector3.right * trapeziumInfo.x / 2 + Vector3.forward * trapeziumInfo.y / 2 - Vector3.up * trapeziumInfo.z / 2;
            Vector3 backLeftDown = -Vector3.right * trapeziumInfo.x / 2 - Vector3.forward * trapeziumInfo.y / 2 - Vector3.up * trapeziumInfo.z / 2;
            Vector3 backRightUp = Vector3.right * trapeziumInfo.x / 2 + Vector3.forward * trapeziumInfo.y / 2 - Vector3.up * trapeziumInfo.z / 2;
            Vector3 backRightDown = Vector3.right * trapeziumInfo.x / 2 - Vector3.forward * trapeziumInfo.y / 2 - Vector3.up * trapeziumInfo.z / 2;

            Vector3 forwardLeftUp = -Vector3.right * trapeziumInfo.w / 2 + Vector3.forward * trapeziumInfo.y / 2 + Vector3.up * trapeziumInfo.z / 2;
            Vector3 forwardLeftDown = -Vector3.right * trapeziumInfo.w / 2 - Vector3.forward * trapeziumInfo.y / 2 + Vector3.up * trapeziumInfo.z / 2;
            Vector3 forwardRightUp = Vector3.right * trapeziumInfo.w / 2 + Vector3.forward * trapeziumInfo.y / 2 + Vector3.up * trapeziumInfo.z / 2;
            Vector3 forwardRightDown = Vector3.right * trapeziumInfo.w / 2 - Vector3.forward * trapeziumInfo.y / 2 + Vector3.up * trapeziumInfo.z / 2;

            UnityEditor.Handles.DrawLine(backLeftUp, backLeftDown);
            UnityEditor.Handles.DrawLine(backLeftDown, backRightDown);
            UnityEditor.Handles.DrawLine(backRightDown, backRightUp);
            UnityEditor.Handles.DrawLine(backRightUp, backLeftUp);

            UnityEditor.Handles.DrawLine(forwardLeftUp, forwardLeftDown);
            UnityEditor.Handles.DrawLine(forwardLeftDown, forwardRightDown);
            UnityEditor.Handles.DrawLine(forwardRightDown, forwardRightUp);
            UnityEditor.Handles.DrawLine(forwardRightUp, forwardLeftUp);

            UnityEditor.Handles.DrawLine(backLeftUp, forwardLeftUp);
            UnityEditor.Handles.DrawLine(backLeftDown, forwardLeftDown);
            UnityEditor.Handles.DrawLine(backRightUp, forwardRightUp);
            UnityEditor.Handles.DrawLine(backRightDown, forwardRightDown);
        }
    }
}
#endregion
#endif