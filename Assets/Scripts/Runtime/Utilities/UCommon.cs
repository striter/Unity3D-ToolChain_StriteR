using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public static class UCommon
{
    public static bool InRange(this RangeFloat _value, float _check) => _value.start <= _check && _check <= _value.end;
    public static float InRangeScale(this RangeFloat _value, float _check) => Mathf.InverseLerp(_value.start, _value.end, _check);
    public static IEnumerable<object> GetEnumValues(Type _enumType)
    {
        foreach(object enumObj in Enum.GetValues(_enumType))
        {
            if (Convert.ToInt32(enumObj) == -1||_enumType.ToString() == "Invalid" )
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

public static class UUnityEngine
{
    public static bool SetActive(this Transform _transform, bool _active) => SetActive(_transform.gameObject, _active);
    public static bool SetActive(this MonoBehaviour _monobehaviour, bool _active) => SetActive(_monobehaviour.gameObject, _active);
    public static bool SetActive(this GameObject _transform, bool _active)
    {
        if (_transform.activeSelf == _active)
            return false;

        _transform.SetActive(_active);
        return true;
    }

    #region Transform
    public static void DestroyChildren(this Transform trans)
    {
        int count = trans.childCount;
        if (count <= 0)
            return;
        for (int i = 0; i < count; i++)
            GameObject.Destroy(trans.GetChild(i).gameObject);
    }
    public static void SetParentResetTransform(this Transform source, Transform target)
    {
        source.SetParent(target);
        source.transform.localPosition = Vector3.zero;
        source.transform.localScale = Vector3.one;
        source.transform.localRotation = Quaternion.identity;
    }
    public static void SetChildLayer(this Transform trans, int layer)
    {
        foreach (Transform temp in trans.gameObject.GetComponentsInChildren<Transform>(true))
            temp.gameObject.layer = layer;
    }
    public static Transform FindInAllChild(this Transform trans, string name)
    {
        foreach (Transform temp in trans.gameObject.GetComponentsInChildren<Transform>(true))
            if (temp.name == name) return temp;
        Debug.LogWarning("Null Child Name:" + name + ",Find Of Parent:" + trans.name);
        return null;
    }

    public static T Find<T>(this T[,] array, Predicate<T> predicate)
    {
        int length0 = array.GetLength(0);
        int length1 = array.GetLength(1);
        for (int i = 0; i < length0; i++)
            for (int j = 0; j < length1; j++)
                if (predicate(array[i, j])) return array[i, j];
        return default(T);
    }


    public static void SortChildByNameIndex(Transform transform, bool higherUpper = true)
    {
        List<Transform> childList = new List<Transform>();
        List<int> childIndexList = new List<int>();

        for (int i = 0; i < transform.childCount; i++)
        {
            childList.Add(transform.GetChild(i));
            childIndexList.Add(int.Parse(childList[i].gameObject.name));
        }
        childIndexList.Sort((a, b) => { return a <= b ? (higherUpper ? 1 : -1) : (higherUpper ? -1 : 1); });

        for (int i = 0; i < childList.Count; i++)
        {
            childList[i].SetSiblingIndex(childIndexList.FindIndex(p => p == int.Parse(childList[i].name)));
        }
    }
    #endregion

    public static Rect Reposition(this Rect _rect, float _newPositionX, float _newPositionY) => Reposition(_rect, new Vector2(_newPositionX, _newPositionY));
    public static Rect Reposition(this Rect _rect, Vector2 _newPosition) { _rect.position = _newPosition; return _rect; }
    public static Rect Resize(this Rect _rect, float _newSizeX, float _newSizeY) => Resize(_rect, new Vector2(_newSizeX, _newSizeY));
    public static Rect Resize(this Rect _rect, Vector2 _newSize) { _rect.size = _newSize; return _rect; }
    public static Rect Expand(this Rect _rect, Vector2 _size) { _rect.position -= _size / 2; _rect.size += _size; return _rect; }
    public static Rect Collapse(this Rect _rect,Vector2 _size) { _rect.position += _size / 2;_rect.size -= _size;return _rect; }

    public static Vector3 GetPoint(this Bounds _bound, Vector3 _normalizedSize) => _bound.center + _bound.size.Multiply(_normalizedSize);
    public static Bounds Resize(this Bounds _srcBounds,Bounds _dstBounds)
    {
        Vector3 min = Vector3.Min(_srcBounds.min, _dstBounds.min);
        Vector3 max = Vector3.Max(_srcBounds.max, _dstBounds.max);
        Vector3 size = min - max;
        return new Bounds(min + size / 2, size);
    }

    #region Camera Helper
    public static bool InputRayCheck(this Camera _camera, Vector2 _inputPos, out RaycastHit _hit, int _layerMask = -1)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            _hit = new RaycastHit();
            return false;
        }

        return Physics.Raycast(_camera.ScreenPointToRay(_inputPos), out _hit, 1000, _layerMask);
    }
    public static Quaternion CameraProjectionOnPlane(this Camera _camera, Vector3 _position) => Quaternion.LookRotation(Vector3.ProjectOnPlane(_position - _camera.transform.position, _camera.transform.right), _camera.transform.up);
    #endregion
}
