using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public static class TCommonUI
{
    public static void SetAnchor(this RectTransform rect,Vector2 anchor)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
    }
    public static void ReAnchorFillX(this RectTransform rect,Vector2 anchorX)
    {
        rect.anchorMin =new Vector2(anchorX.x, rect.anchorMin.y);
        rect.anchorMax = new Vector2(anchorX.y, rect.anchorMax.y);
        rect.offsetMax = Vector2.zero;
        rect.offsetMin = Vector2.zero;
    }
    public static void ReAnchorReposX(this RectTransform rect,float x)
    {
        rect.anchorMin = new Vector2(x, rect.anchorMin.y);
        rect.anchorMax = new Vector2(x, rect.anchorMax.y);
        rect.anchoredPosition = Vector2.zero;
    }

    public static void SetWorldViewPortAnchor(this RectTransform rect, Vector3 worldPos, Camera camera, float lerpParam=1f)
    {
        Vector2 viewPortAnchor = camera.WorldToViewportPoint(worldPos);
        rect .anchorMin = Vector2.Lerp(rect.anchorMin, viewPortAnchor, lerpParam);
        rect.anchorMax = Vector2.Lerp(rect.anchorMin, viewPortAnchor, lerpParam);
    }
    
    public static void ReparentRestretchUI(this RectTransform rect,Transform targetTrans)
    {
        rect.SetParent(targetTrans);
        rect.offsetMax = Vector2.zero;
        rect.offsetMin = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localPosition = Vector3.zero;
    }

    public static void RaycastAll(Vector2 castPos)      //Bind UIT_EventTriggerListener To Items Need To Raycast By EventSystem
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = castPos;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        for (int i = 0; i < results.Count; i++)
        {
            UIT_EventTriggerListener listener = results[i].gameObject.GetComponent<UIT_EventTriggerListener>();
            if (listener != null)
                listener.OnRaycast();
        }
    }
}

public static class TCommon
{
    #region Transform
    public static bool SetActivate(this MonoBehaviour behaviour, bool active)=>SetActivate(behaviour.gameObject, active);
    public static bool SetActivate(this Transform tra, bool active)=> SetActivate(tra.gameObject, active);
    public static bool SetActivate(this GameObject go, bool active)
    {
        if (go.activeSelf == active)
            return false;

        go.SetActive(active);
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
    public static void SetParentResetTransform(this Transform source,Transform target)
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
    #region Color
    public static Color GetHexColor(string hex)
    {
        if (hex.Length != 8)
        {
            Debug.LogError("Hex Color Length Not Equals 8!");
            return Color.magenta;
        }

        byte br = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte bg = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte bb = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        byte cc = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        float r = br / 255f; float g = bg / 255f; float b = bb / 255f; float a = cc / 255f;
        return new Color(r, g, b, a);
    }
    public static Color ColorAlpha(Color origin, float alpha)=> new Color(origin.r, origin.g, origin.b, alpha);
    #endregion
    #region Vector/Angle
    public static float GetXZDistance(Vector3 start, Vector3 end) =>new Vector2(start.x - end.x, start.z - end.z).magnitude;
    public static float GetXZSqrDistance(Vector3 start, Vector3 end) => new Vector2(start.x - end.x, start.z - end.z).sqrMagnitude;
    public static Vector3 GetXZLookDirection(Vector3 startPoint, Vector3 endPoint)
    {
        Vector3 lookDirection = endPoint - startPoint;
        lookDirection.y = 0;
        lookDirection.Normalize();
        return lookDirection;
    }
    public static Vector3 RotateDirectionClockwise(this Vector3 Direction, Vector3 axis, float angle) => (Quaternion.AngleAxis(angle, axis) * Direction).normalized;

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
    public static float GetIncludedAngle(float angle1, float angle2)
    {
        float angle = 0;
        if (angle1 >= 270 && angle2 < 90)
        {
            angle = (angle1 - (angle2 + 360)) % 180;
        }
        else if (angle1 <= 90 && angle2 >= 270)
        {
            angle = (angle1 + 360 - angle2) % 180;
        }
        else
        {
            angle = (angle1 - angle2);
            if (Mathf.Abs(angle) > 180)
            {
                angle -= 360;
            }
        }
        return angle;
    }
    #endregion
    #region Collections/Array Traversal
    public static T GetIndexKey<T, Y>(this Dictionary<T, Y> dictionary, int index) => dictionary.ElementAt(index).Key;
    public static Y GetIndexValue<T, Y>(this Dictionary<T, Y> dictionary, int index) => dictionary.ElementAt(index).Value;

    public static List<T> DeepCopy<T>(this List<T> list) 
    {
        List<T> copyList = new List<T>();
        list.Traversal((T value) => { copyList.Add(value); });
        return copyList;
    }

    public static Dictionary<T,Y> DeepCopy<T,Y>(this Dictionary<T,Y> dictionary) 
    {
        Dictionary<T, Y> copyDic = new Dictionary<T, Y>();
        dictionary.Traversal((T key, Y value) => { copyDic.Add(key, value); });
        return copyDic;
    }

    public static Dictionary<T,List<Y>> DeepCopy<T,Y>(this Dictionary<T,List<Y>> dictionary) where T:struct where Y:struct
    {
        Dictionary<T, List<Y>> copyDic = new Dictionary<T, List<Y>>();
        dictionary.Traversal((T key, List<Y> value) => { copyDic.Add(key, value.DeepCopy()); });
        return copyDic;
    }

    public static void Traversal<T>(this List<T> list, Action<int, T> OnEachItem,bool shallowCopy=false)
    {
        List<T> tempList = shallowCopy ? new List<T>(list) : list;
        TraversalEnumerableIndex(0, list.Count, (int index) => { OnEachItem(index, tempList[index]); return false; });
    }
    public static void Traversal<T>(this List<T> list, Action<T> OnEachItem, bool shallowCopy = false)
    {
        List<T> tempList = shallowCopy ? new List<T>(list) : list;
        TraversalEnumerableIndex(0, list.Count, (int index) => {  OnEachItem(tempList[index]); return false; });
    }
    public static void TraversalBreak<T>(this List<T> list, Func<T,bool> OnEachItem,bool shallowCopy=false)
    {
        List<T> tempList = shallowCopy ? new List<T>(list) : list;
        TraversalEnumerableIndex(0, list.Count, (int index) => {return OnEachItem(tempList[index]);});
    }
    public static void Traversal<T, Y>(this Dictionary<T, Y> dic, Action<T> OnEachKey,bool shallowCopy=false)
    {
        Dictionary<T, Y> tempDic = shallowCopy ? new Dictionary<T, Y>(dic) : dic;
        foreach (T temp in tempDic.Keys)
            OnEachKey(temp);
    }
    public static void Traversal<T, Y>(this Dictionary<T, Y> dic, Action<Y> OnEachValue,bool shallowCopy =false)
    {
        Dictionary<T, Y> tempDic = shallowCopy  ? new Dictionary<T, Y>(dic) : dic;
        foreach (T temp in tempDic.Keys)
            OnEachValue(tempDic[temp]);
    }
    public static void Traversal<T, Y>(this Dictionary<T, Y> dic, Action<T, Y> OnEachPair,bool shallowCopy =false)
    {
        Dictionary<T, Y> tempDic = shallowCopy  ? new Dictionary<T, Y>(dic) : dic;
        foreach (T temp in tempDic.Keys)
            OnEachPair(temp,tempDic[temp]);
    }
    public static void TraversalBreak<T, Y>(this Dictionary<T, Y> dic, Func<Y, bool> OnEachItem, bool shallowCopy = false)
    {
        Dictionary<T, Y> tempDic = shallowCopy ? new Dictionary<T, Y>(dic) : dic;
        foreach (T temp in tempDic.Keys)
            if (OnEachItem(tempDic[temp]))
                break;
    }
    public static void Traversal<T>(this T[] array, Action<T> OnEachItem)
    {
        int length = array.Length;
        for (int i = 0; i < length; i++)
            OnEachItem(array[i]);
    }
    public static void Traversal<T>(this T[] array, Action<int,T> OnEachItem)
    {
        int length = array.Length;
        for (int i = 0; i < length; i++)
            OnEachItem(i,array[i]);
    }
    public static void Traversal<T>(this T[,] array, Action<T> OnEachItem)
    {
        int length0 = array.GetLength(0);
        int length1 = array.GetLength(1);
        for (int i = 0; i < length0; i++)
            for (int j = 0; j < length1; j++)
                OnEachItem(array[i, j]);
    }
    public static void Traversal<T>(this T[,] array, Action<int,int, T> OnEachItem)
    {
        int length0 = array.GetLength(0);
        int length1 = array.GetLength(1);
        for (int i = 0; i < length0; i++)
            for (int j = 0; j < length1; j++)
                OnEachItem(i,j,array[i, j]);
    }

    public static void TraversalRandomBreak<T>(this List<T> list, Func<int, bool> OnRandomItemStop = null, System.Random seed = null) => TraversalEnumerableIndex(list.RandomIndex(seed), list.Count,  OnRandomItemStop);
    public static void TraversalRandomBreak<T>(this T[] array, Func<int, bool> OnRandomItemStop = null, System.Random seed = null) => TraversalEnumerableIndex(array.RandomIndex(seed), array.Length,  OnRandomItemStop);
    public static void TraversalRandomBreak<T>(this List<T> list, Func<T, bool> OnRandomItemStop = null, System.Random seed = null) => TraversalEnumerableIndex(list.RandomIndex(seed), list.Count, (int index) => OnRandomItemStop != null && OnRandomItemStop(list[index]));
    public static void TraversalRandomBreak<T>(this T[] array, Func<T, bool> OnRandomItemStop = null, System.Random seed = null) => TraversalEnumerableIndex(array.RandomIndex(seed), array.Length, (int index) => OnRandomItemStop != null && OnRandomItemStop(array[index]));
    public static void TraversalRandomBreak<T, Y>(this Dictionary<T, Y> dictionary, Func<T, Y, bool> OnRandomItemStop = null, System.Random seed = null) => TraversalEnumerableIndex(RandomLength(dictionary.Count,seed),dictionary.Count,(int index)=> {
        KeyValuePair<T, Y> element = dictionary.ElementAt(index);
        return OnRandomItemStop != null && OnRandomItemStop(element.Key, element.Value);
    });

    static void TraversalEnumerableIndex(int index, int count,Func<int,bool> OnItemBreak )
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

    #region Enum

    public static void TraversalEnum<T>(Action<T> enumAction)    //Can't Constraint T to System.Enum?
    {
        if (!typeof(T).IsSubclassOf(typeof(Enum)))
        {
            Debug.LogError("Can't Traversal EnEnum Class!");
            return;
        }

        foreach (object temp in Enum.GetValues(typeof(T)))
        {
            if (temp.ToString() == "Invalid")
                continue;
            enumAction((T)temp);
        }
    }
    public static List<T> GetEnumList<T>()
    {
        if (!typeof(T).IsSubclassOf(typeof(Enum)))
        {
            Debug.LogError("Can't Traversal EnEnum Class!");
            return null;
        }

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
    #endregion
    #endregion
    #region Random
    public static int RandomLength(int length, System.Random seed = null) => seed != null ? seed.Next(length) : UnityEngine.Random.Range(0, length);
    public static float RandomLength(float length, System.Random seed = null) => seed != null ? (float)seed.NextDouble() * length : UnityEngine.Random.Range(0,length);
    public static float RandomUnitValue(System.Random seed = null) => seed != null ? (float)seed.NextDouble()*2f-1f : UnityEngine.Random.Range(-1f,1f);
    public static Vector3 RandomXZSphere(System.Random seed = null) => RandomXZCircle(seed) * RandomUnitValue(seed);
    public static Vector3 RandomXZCircle(System.Random seed=null)
    {
        Vector2 randomCirlce = Vector2.zero;
        if (seed != null)
        {
            float radin = RandomUnitValue(seed) * Mathf.PI;
            randomCirlce = new Vector2(Mathf.Sin(radin), Mathf.Cos(radin));
        }
        else
        {
            randomCirlce = UnityEngine.Random.insideUnitCircle;
        }
        return new Vector3(randomCirlce.x, 0, randomCirlce.y);
    }

    public static int Random(this RangeInt ir, System.Random seed = null) => ir.start + RandomLength(ir.length + 1, seed);
    public static float Random(this RangeFloat ir, System.Random seed = null) => seed != null ? seed.Next((int)(ir.start * 1000), (int)(ir.end * 1000)) / 1000f : UnityEngine.Random.Range(ir.start, ir.end);

    public static int RandomIndex<T>(this List<T> randomList, System.Random seed = null) => RandomLength(randomList.Count,seed);
    public static int RandomIndex<T>(this T[] randomArray, System.Random randomSeed = null) => RandomLength(randomArray.Length,randomSeed);

    public static T RandomItem<T>(this List<T> randomList, System.Random randomSeed = null) => randomList[randomSeed != null ? randomSeed.Next(randomList.Count) : UnityEngine.Random.Range(0, randomList.Count)];
    public static T RandomItem<T>(this T[] array, System.Random randomSeed = null) => randomSeed != null ? array[randomSeed.Next(array.Length)] : array[UnityEngine.Random.Range(0, array.Length)];

    public static T RandomItem<T>(this T[,] array, System.Random randomSeed = null) => randomSeed != null ? array[randomSeed.Next(array.GetLength(0)), randomSeed.Next(array.GetLength(1))] : array[UnityEngine.Random.Range(0, array.GetLength(0)), UnityEngine.Random.Range(0, array.GetLength(1))];

    public static T RandomKey<T, Y>(this Dictionary<T, Y> dic, System.Random randomSeed = null) => dic.ElementAt(RandomLength(dic.Count, randomSeed)).Key;
    public static Y RandomValue<T, Y>(this Dictionary<T, Y> dic, System.Random randomSeed = null) => dic.ElementAt(RandomLength(dic.Count, randomSeed)).Value;
    public static bool RandomBool(System.Random seed = null) => seed != null ? seed.Next(0, 2) > 0 : UnityEngine.Random.Range(0, 2) > 0;
    public static int RandomPercentageInt(System.Random random=null)=> random != null ? random.Next(0, 101)  : UnityEngine.Random.Range(0, 101);
    public static float RandomPercentageFloat(System.Random random = null) => RandomLength(100, random);
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

    public static T RandomEnumValues<T>(System.Random _seed=null)        //Can't Constraint T to System.Enum
    {
        if (!typeof(T).IsSubclassOf(typeof(Enum)))
        {
            Debug.LogError("Can't Traversal EnEnum Class!");
            return default(T);
        }
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
        return default(T);
    }
    #endregion

}