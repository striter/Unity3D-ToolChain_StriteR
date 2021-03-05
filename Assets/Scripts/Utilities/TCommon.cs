using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
public static class TCommon
{
    #region Transform
    public static bool SetActive(this Transform _transform, bool _active) => SetActive(_transform.gameObject, _active);
    public static bool SetActive(this MonoBehaviour _monobehaviour, bool _active) => SetActive(_monobehaviour.gameObject, _active);
    public static bool SetActive(this GameObject _transform, bool _active)
    {
        if (_transform.activeSelf == _active)
            return false;

        _transform.SetActive(_active);
        return true;
    }
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
    #region Angle
    public static float GetAngle(Vector3 first, Vector3 second, Vector3 up)
    {
        float angle = Vector3.Angle(first, second);
        angle *= Mathf.Sign(Vector3.Dot(up, Vector3.Cross(first, second)));
        return angle;
    }
    public static float GetAngleY(Vector3 first, Vector3 second, Vector3 up)
    {
        Vector3 newFirst = new Vector3(first.x, 0, first.z);
        Vector3 newSecond = new Vector3(second.x, 0, second.z);
        return GetAngle(newFirst, newSecond, up);
    }
    public static float AngleToRadin(float angle) => Mathf.PI * angle / 180f;
    public static float RadinToAngle(float radin) => radin / Mathf.PI * 180f;
    #endregion
    #region Vector
    public static float GetXZDistance(Vector3 start, Vector3 end) => new Vector2(start.x - end.x, start.z - end.z).magnitude;
    public static float GetXZSqrDistance(Vector3 start, Vector3 end) => new Vector2(start.x - end.x, start.z - end.z).sqrMagnitude;
    public static Vector3 GetXZLookDirection(Vector3 startPoint, Vector3 endPoint)
    {
        Vector3 lookDirection = endPoint - startPoint;
        lookDirection.y = 0;
        lookDirection.Normalize();
        return lookDirection;
    }
    public static Vector3 RotateDirectionClockwise(this Vector3 Direction, Vector3 axis, float angle) => (Quaternion.AngleAxis(angle, axis) * Direction).normalized;
    public static float Max(this Vector3 _src) => Mathf.Max(Mathf.Max(_src.x, _src.y), _src.z);
    public static float Min(this Vector3 _src) => Mathf.Min(Mathf.Min(_src.x, _src.y), _src.z);
    public static float Max(this Vector4 _src) => Mathf.Max(Mathf.Max(Mathf.Max(_src.x, _src.y), _src.z), _src.w);
    public static float Min(this Vector4 _src) => Mathf.Min(Mathf.Min(Mathf.Min(_src.x, _src.y), _src.z), _src.w);
    public static Vector3 Multiply(this Vector3 _src, Vector3 _tar) => new Vector3(_src.x * _tar.x, _src.y * _tar.y, _src.z * _tar.z);
    public static Vector3 Divide(this Vector3 _src, Vector3 _tar) => new Vector3(_src.x / _tar.x, _src.y / _tar.y, _src.z / _tar.z);
    public static Vector4 Multiply(this Vector4 _src, Vector4 _tar) => new Vector4(_src.x * _tar.x, _src.y * _tar.y, _src.z * _tar.z, _src.w * _tar.w);
    public static Vector4 Divide(this Vector4 _src, Vector4 _tar) => new Vector4(_src.x / _tar.x, _src.y / _tar.y, _src.z / _tar.z, _src.w / _tar.w);
    #endregion
    #region Basic
    public static int Power(int _src,int _pow)
    {
        if (_pow == 0) return 1;
        if (_pow == 1) return _src;
        int dst = _src;
        for(int i=0;i<_pow-1;i++)
            dst *= _src;
        return dst;
    }
    public static bool InRange(this RangeFloat _value, float _check) => _value.start <= _check && _check <= _value.end;
    public static float InRangeScale(this RangeFloat _value, float _check) => Mathf.InverseLerp(_value.start, _value.end, _check);
    public static Vector3 ToVector3(this Vector4 _vector) => new Vector3(_vector.x,_vector.y,_vector.z);
    public static Vector4 ToVector4(this Vector3 _vector,float _fill=0) => new Vector4(_vector.x,_vector.y,_vector.z,_fill);
    #endregion
    #region Collections & Array 
    public static T GetIndexKey<T, Y>(this Dictionary<T, Y> dictionary, int index) => dictionary.ElementAt(index).Key;
    public static Y GetIndexValue<T, Y>(this Dictionary<T, Y> dictionary, int index) => dictionary.ElementAt(index).Value;
    public static List<T> DeepCopy<T>(this List<T> list)
    {
        List<T> copyList = new List<T>();
        list.Traversal((T value) => { copyList.Add(value); });
        return copyList;
    }
    public static Dictionary<T, Y> DeepCopy<T, Y>(this Dictionary<T, Y> dictionary)
    {
        Dictionary<T, Y> copyDic = new Dictionary<T, Y>();
        dictionary.Traversal((T key, Y value) => { copyDic.Add(key, value); });
        return copyDic;
    }
    public static Dictionary<T, List<Y>> DeepCopy<T, Y>(this Dictionary<T, List<Y>> dictionary) where T : struct where Y : struct
    {
        Dictionary<T, List<Y>> copyDic = new Dictionary<T, List<Y>>();
        dictionary.Traversal((T key, List<Y> value) => { copyDic.Add(key, value.DeepCopy()); });
        return copyDic;
    }
    public static T[] Copy<T>(this T[] _srcArray)
    {
        T[] _dstArray = new T[_srcArray.Length];
        for(int i=0;i<_srcArray.Length;i++)
            _dstArray[i] = _srcArray[i];
        return _dstArray;
    }

    public static void Traversal<T>(this IEnumerable<T> _numerable, Action<T> OnEachItem)
    {
        foreach (T item in _numerable)
            OnEachItem(item);
    }
    public static void Traversal<T>(this IEnumerable<T> _numerable, Action<int,T> OnEachItem)
    {
        int index = 0;
        foreach (T item in _numerable)
            OnEachItem(index++,item);
    }

    public static void TraversalMark<T>(this List<T> targetList,Predicate<T> DoMarkup,Action<T> OnEachMarkup)
    {
        List<T> markupList = new List<T>();
        foreach (T item in targetList)
        {
            if (!DoMarkup(item))
                continue;
            markupList.Add(item);
        }
        markupList.Traversal(item=>OnEachMarkup(item));
    }

    public static void TraversalBreak<T>(this IEnumerable<T> _numerable, Predicate<T> OnEachItemBreak)
    {
        foreach (T item in _numerable)
            if (OnEachItemBreak(item))
                break;
    }


    public static void Traversal<T, Y>(this Dictionary<T, Y> dic, Action<T> OnEachKey)
    {
        foreach (T temp in dic.Keys)
            OnEachKey(temp);
    }
    public static void Traversal<T, Y>(this Dictionary<T, Y> dic, Action<Y> OnEachValue)
    {
        foreach (T temp in dic.Keys)
            OnEachValue(dic[temp]);
    }
    public static void Traversal<T, Y>(this Dictionary<T, Y> dic, Action<T, Y> OnEachPair)
    {
        foreach (T temp in dic.Keys)
            OnEachPair(temp, dic[temp]);
    }
    public static void TraversalBreak<T, Y>(this Dictionary<T, Y> dic, Predicate<Y> OnEachItem)
    {
        foreach (T temp in dic.Keys)
            if (OnEachItem(dic[temp]))
                break;
    }
    public static void TraversalMark<T, Y>(this Dictionary<T, Y> dic, Predicate<Y> DoMarkup,Action<T> OnMarkup)
    {
        List<T> markKeys = new List<T>();
        foreach (T key in dic.Keys)
        {
            if (!DoMarkup(dic[key]))
                continue;
            markKeys.Add(key);
        }
        markKeys.Traversal(key => { OnMarkup(key);});
    }

    public static void Traversal<T>(this T[] array, Action<int, T> OnEachItem)
    {
        int length = array.Length;
        for (int i = 0; i < length; i++)
            OnEachItem(i, array[i]);
    }
    public static void Traversal<T>(this T[,] array, Action<T> OnEachItem)
    {
        int length0 = array.GetLength(0);
        int length1 = array.GetLength(1);
        for (int i = 0; i < length0; i++)
            for (int j = 0; j < length1; j++)
                OnEachItem(array[i, j]);
    }
    public static void Traversal<T>(this T[,] array, Action<int, int, T> OnEachItem)
    {
        int length0 = array.GetLength(0);
        int length1 = array.GetLength(1);
        for (int i = 0; i < length0; i++)
            for (int j = 0; j < length1; j++)
                OnEachItem(i, j, array[i, j]);
    }

    public static void TraversalRandomBreak<T>(this List<T> list, Func<int, bool> OnRandomItemStop = null, System.Random seed = null) => TraversalEnumerableIndex(list.RandomIndex(seed), list.Count, OnRandomItemStop);
    public static void TraversalRandomBreak<T>(this T[] array, Func<int, bool> OnRandomItemStop = null, System.Random seed = null) => TraversalEnumerableIndex(array.RandomIndex(seed), array.Length, OnRandomItemStop);
    public static void TraversalRandomBreak<T>(this List<T> list, Func<T, bool> OnRandomItemStop = null, System.Random seed = null) => TraversalEnumerableIndex(list.RandomIndex(seed), list.Count, (int index) => OnRandomItemStop != null && OnRandomItemStop(list[index]));
    public static void TraversalRandomBreak<T>(this T[] array, Func<T, bool> OnRandomItemStop = null, System.Random seed = null) => TraversalEnumerableIndex(array.RandomIndex(seed), array.Length, (int index) => OnRandomItemStop != null && OnRandomItemStop(array[index]));
    public static void TraversalRandomBreak<T, Y>(this Dictionary<T, Y> dictionary, Func<T, Y, bool> OnRandomItemStop = null, System.Random seed = null) => TraversalEnumerableIndex(Random(dictionary.Count, seed), dictionary.Count, (int index) => {
        KeyValuePair<T, Y> element = dictionary.ElementAt(index);
        return OnRandomItemStop != null && OnRandomItemStop(element.Key, element.Value);
    });

    public static void TraversalEnumerableIndex(int index, int count, Func<int, bool> OnItemBreak)
    {
        if (count == 0)
            return;
        for (int i = 0; i < count; i++)
        {
            if (OnItemBreak != null && OnItemBreak(index))
                break;

            index++;
            if (index == count)
                index = 0;
        }
    }

    public static string ToString_Readable<T>(this IEnumerable<T> inumerable,string breakAppend,Func<T,string> OnEachAppend)
    {
        StringBuilder builder = new StringBuilder();
        int maxIndex = inumerable.Count()-1;
        inumerable.Traversal((int index, T temp) => {
            builder.Append(OnEachAppend(temp));
            if(index!= maxIndex)
                builder.Append(breakAppend);
        });
        return builder.ToString();
    }
    public static string ToString_FieldName(this string _fieldName)
    {
        int index = _fieldName.IndexOf("m_");
        if(index!=-1)
            _fieldName=_fieldName.Remove(index,2);

        int length = _fieldName.Length-1;
        for (int i = 0; i < length; i++)
        {


            if (Char.IsLower(_fieldName[i]) && Char.IsUpper(_fieldName[i + 1]))
            {
                _fieldName = _fieldName.Insert(i + 1, " ");
                i++;
                length ++ ;
            }
        }
        return _fieldName;
    }

    public static void AddRange<T,Y>(this List<T> list,IEnumerable<Y> ienumerable,Func<Y,T> OnEachAddItem)
    {
        ienumerable.Traversal((item => list.Add(OnEachAddItem(item))));
    }
    public static T[] Add<T>(this T[] srcArray, T[] tarArray)
    {
        int srcLength = srcArray.Length;
        int tarLength = tarArray.Length;
        T[] newArray = new T[srcLength + tarLength];
        for(int i=0;i<srcLength;i++)
            newArray[i] =srcArray[i];
        for(int i=0;i<tarLength;i++)
            newArray[srcLength+i] = tarArray[i];
        return newArray;
    }
    public static Y[] ReconstructToArray<T,Y>(this T[] srcArray,Func<T,Y> GetDstItem)
    {
        Y[] dstArray = new Y[srcArray.Length]; ;
        srcArray.Traversal((index, srcItem) =>dstArray[index]=GetDstItem(srcItem));
        return dstArray;
    }

    #region Enum
    public static void TraversalEnum<T>(Action<T> enumAction) where T:Enum 
    {
        foreach (object temp in Enum.GetValues(typeof(T)))
        {
            if (temp.ToString() == "Invalid")
                continue;
            enumAction((T)temp);
        }
    }
    public static List<T> GetEnumList<T>() where T:Enum
    {
        List<T> list = new List<T>();
        Array allEnums = Enum.GetValues(typeof(T));
        for (int i = 0; i < allEnums.Length; i++)
        {
            if (allEnums.GetValue(i).ToString() == "Invalid")
                continue;
            list.Add((T)allEnums.GetValue(i));
        }
        return list;
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
            int curPower = Power(2,i);
            if (curPower > maxPower)
                yield break;
            yield return (flagValues&curPower)==curPower;
        }
        yield break;
    }
    #endregion
    #endregion
    #region Random
    public static int Random(int length, System.Random seed = null) => seed != null ? seed.Next(length) : UnityEngine.Random.Range(0, length);
    public static float Random(float length, System.Random seed = null) => seed != null ? (float)seed.NextDouble() * length : UnityEngine.Random.Range(0,length);
    public static float RandomUnit(System.Random seed = null) => seed != null ? (float)seed.NextDouble()*2f-1f : UnityEngine.Random.Range(-1f,1f);
    public static Vector3 RandomUnitSphere(System.Random seed = null) => RandomUnitCircle(seed) * RandomUnit(seed);
    public static Vector3 RandomUnitCircle(System.Random seed=null)
    {
        float radin =  RandomUnit(seed) * Mathf.PI;
        Vector2 randomCirlce = new Vector2(Mathf.Sin(radin), Mathf.Cos(radin));
        return new Vector3(randomCirlce.x, 0, randomCirlce.y);
    }
    public static int Random(this RangeInt ir, System.Random seed = null) => ir.start + Random(ir.length + 1, seed);
    public static float Random(this RangeFloat ir, System.Random seed = null) => seed != null ? seed.Next((int)(ir.start * 1000), (int)(ir.end * 1000)) / 1000f : UnityEngine.Random.Range(ir.start, ir.end);   
    public static int RandomIndex<T>(this List<T> randomList, System.Random seed = null) => Random(randomList.Count,seed);
    public static int RandomIndex<T>(this T[] randomArray, System.Random randomSeed = null) => Random(randomArray.Length,randomSeed);
    public static T RandomItem<T>(this List<T> randomList, System.Random randomSeed = null) => randomList[randomSeed != null ? randomSeed.Next(randomList.Count) : UnityEngine.Random.Range(0, randomList.Count)];
    public static T RandomItem<T>(this T[] array, System.Random randomSeed = null) => randomSeed != null ? array[randomSeed.Next(array.Length)] : array[UnityEngine.Random.Range(0, array.Length)];
    public static T RandomItem<T>(this T[,] array, System.Random randomSeed = null) => randomSeed != null ? array[randomSeed.Next(array.GetLength(0)), randomSeed.Next(array.GetLength(1))] : array[UnityEngine.Random.Range(0, array.GetLength(0)), UnityEngine.Random.Range(0, array.GetLength(1))];
    public static T RandomKey<T, Y>(this Dictionary<T, Y> dic, System.Random randomSeed = null) => dic.ElementAt(Random(dic.Count, randomSeed)).Key;
    public static Y RandomValue<T, Y>(this Dictionary<T, Y> dic, System.Random randomSeed = null) => dic.ElementAt(Random(dic.Count, randomSeed)).Value;
    public static bool RandomBool(System.Random seed = null) => seed != null ? seed.Next(0, 2) > 0 : UnityEngine.Random.Range(0, 2) > 0;
    public static Color RandomColor(System.Random seed = null, float alpha = -1)=>  new Color(Random(1f,seed), Random(1f, seed), Random(1f, seed), alpha < 0 ? Random(1f, seed):alpha );
    public static int RandomPercentageInt(System.Random random=null)=> random != null ? random.Next(0, 101)  : UnityEngine.Random.Range(0, 101);
    public static float RandomPercentageFloat(System.Random random = null) => Random(100, random);
    public static T RandomPercentage<T>(this Dictionary<T, int> percentageRate, System.Random seed) => RandomPercentage(percentageRate,default(T),seed);
    public static T RandomPercentage<T>(this Dictionary<T, int> percentageRate,T invlaid=default(T), System.Random seed = null)
    {
        float value = RandomPercentageInt(seed);
        T targetLevel = invlaid;
        int totalAmount = 0;
        bool marked = false;
        percentageRate.Traversal((T temp, int amount) => {
            if (marked)
                return;
            totalAmount += amount;
            if (totalAmount >= value)
            {
                targetLevel = temp;
                marked = true;
            }
        });
        return targetLevel;
    }
    public static T RandomPercentage<T>(this Dictionary<T, float> percentageRate, T invalid = default(T), System.Random seed = null)
    {
        float value = RandomPercentageInt(seed);
        T targetLevel = invalid;
        float totalAmount = 0;
        bool marked = false;
        percentageRate.Traversal((T temp, float amount) => {
            if (marked)
                return;
            totalAmount += amount;
            if (totalAmount >= value)
            {
                targetLevel = temp;
                marked = true;
            }
        });
        return targetLevel;
    }

    public static T RandomEnumValues<T>(System.Random _seed=null) where T:Enum
    {
        Array allEnums = Enum.GetValues(typeof(T));
        int randomIndex = _seed != null ? _seed.Next(1, allEnums.Length): UnityEngine.Random.Range(1,allEnums.Length);
        int count=0;
        foreach (object temp in allEnums)
        {
            count++;
            if (temp.ToString() == "Invalid"||count!=randomIndex)
                continue;
            return (T)temp;
        }
        return default;
    }
    #endregion
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
    #region Copy Helper
    static readonly Dictionary<Type, Action<UnityEngine.Object, UnityEngine.Object>> m_CopyHelper = new Dictionary<Type, Action<UnityEngine.Object, UnityEngine.Object>>()
    {
        { typeof(Mesh),(src, dst) => CopyMesh((Mesh)src, (Mesh)dst)}
    };

    public static bool CopyPropertyTo<T>(UnityEngine.Object source,UnityEngine.Object target)
    {
        Type type = typeof(T);
        if(m_CopyHelper.ContainsKey(type))
        {
            m_CopyHelper[type](source, target);
            return true;
        }
        return false;
    }

    public static void CopyMesh(Mesh source,Mesh target)
    {
        target.Clear();
        target.vertices = source.vertices;
        target.normals = source.normals;
        target.tangents = source.tangents;
        target.name = source.name;
        target.bounds = source.bounds;
        target.bindposes = source.bindposes;
        target.colors = source.colors;
        target.boneWeights = source.boneWeights;
        target.triangles = source.triangles;
        target.uv = source.uv;
        target.uv2 = source.uv2;
        target.uv3 = source.uv3;
        target.uv4 = source.uv4;
        target.uv5 = source.uv5;
        target.uv6 = source.uv6;
        target.uv7 = source.uv7;
        target.uv8 = source.uv8;
        for (int i = 0; i < source.subMeshCount; i++)
            target.SetIndices(source.GetIndices(i), MeshTopology.Triangles, i);
    }

    public static Mesh Copy(this Mesh _srcMesh)
    {
        Mesh copy = new Mesh();
        CopyMesh(_srcMesh,copy);
        return copy;
    }

    #endregion

    #region Rect
    public static Rect Reposition(this Rect _rect, float _newPositionX, float _newPositionY) => Reposition(_rect, new Vector2(_newPositionX, _newPositionY));
    public static Rect Reposition(this Rect _rect, Vector2 _newPosition) { _rect.position = _newPosition; return _rect; }
    public static Rect Resize(this Rect _rect, float _newSizeX, float _newSizeY) => Resize(_rect,new Vector2(_newSizeX,_newSizeY));
    public static Rect Resize(this Rect _rect, Vector2 _newSize) { _rect.size = _newSize; return _rect; }
    #endregion
}
