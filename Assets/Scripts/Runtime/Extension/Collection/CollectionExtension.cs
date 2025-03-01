using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

namespace System.Linq.Extensions
{
    public static class UCollection
    {
        #region IEnumrable
            public static void Execute(this IEnumerator _iterator)
            {
                while (_iterator.MoveNext()) {
                    //Empty;
                }
            }

            public static IEnumerable<T> Resolve<T>(this IEnumerable<IEnumerable<T>> _collections)
            {
                foreach (var collection in _collections)
                {
                    foreach (var element in collection)
                    {
                        yield return element;
                    }
                }
            }
            public static IEnumerable<Y> Resolve<T,Y>(this IEnumerable<T> _collections) where T: IEnumerable<Y>
            {
                foreach (var collection in _collections)
                {
                    foreach (var element in collection)
                    {
                        yield return element;
                    }                
                }
            }

            public static IEnumerable<T> Stride<T>(this IEnumerable<T> _collection, int _stride)
            {
                var index = 0;
                foreach (var element in _collection)
                {
                    if (index++ % _stride == 0)
                        yield return element;
                }
            }
            
            public static IEnumerable<(int index,T value)> LoopIndex<T>(this IEnumerable<T> _collection)
            {
                int index = 0;
                foreach (T element in _collection)
                    yield return (index++,element);
            }
            public static IEnumerable<T> Extend<T>(this IEnumerable<T> _collection, IEnumerable<T> _extend)
            {
                foreach (T element in _collection)
                    yield return element;
                foreach (T element in _extend)
                    yield return element;
            }
            public static IEnumerable<T> Extend<T>(this IEnumerable<T> _collection, T _extend)
            {
                foreach (T element in _collection)
                    yield return element;
                yield return _extend;
            }

            public static IEnumerable<T> Collect<T>(this IEnumerable<T> _collection, Predicate<T> _predicate)
            {
                foreach (T element in _collection)
                {
                    if (!_predicate(element))
                        continue;
                    yield return element;
                }
            }

            public static IEnumerable<T> UnCollect<T>(this IEnumerable<T> _collection, Predicate<T> _predicate)
            {
                foreach (T element in _collection)
                {
                    if (_predicate(element))
                        continue;
                    yield return element;
                }
            }

            public static IEnumerable<T> Exclude<T>(this IEnumerable<T> _collection, T _exclude)
            {
                foreach (T element in _collection)
                {
                    if (EqualityComparer<T>.Default.Equals(element, _exclude))
                        continue;
                    yield return element;
                }
            }
            
            public static IEnumerable<T> ExcludeIndex<T>(this IEnumerable<T> _collection, int _index)
            {
                var index = 0;
                foreach (T element in _collection)
                {
                    if (index++ == _index)
                        continue;
                    yield return element;
                }
            }

            public static  IEnumerable<Y> CollectAs<T,Y>(this IEnumerable<T> _collection) where T:class
            {
                foreach (T element in _collection)
                {
                    if (!(element is Y element1))
                        continue;
                    yield return element1;
                }
            }
            public static  IEnumerable<Y> CollectAs<T,Y>(this IEnumerable<T> _collection,Func<T,Y> _collect) where T:class where Y:class
            {
                foreach (T element in _collection)
                    yield return _collect(element);
            }
            
            public static IEnumerable<T> Collect<T>(this IEnumerable<T> _collections, Func<int,T,bool> _Predicate)
            {
                foreach (var (index,value) in _collections.LoopIndex())
                {
                    if(!_Predicate(index,value))
                        continue;
                    yield return value;
                }
            }

            public static IEnumerable<T> SingleExecute<T>(T _element)
            {
                yield return _element;
            }

            public static IEnumerable<T> Range<T>(this IEnumerable<T> _collection, RangeInt _range)
            {
                var index = 0;
                foreach (var element in _collection)
                {
                    if (_range.Contains(index))
                        yield return element;
                    index++;
                }
            }

            public static IEnumerable<T> CollectIndex<T>(this IEnumerable<T> _collection, IEnumerable<int> _indexes)
            {
                var index = 0;
                foreach (var element in _collection)
                {
                    if (_indexes.Contains(index))
                        yield return element;
                    index++;
                }
            }
            public static IEnumerable<int> CollectAsIndex<T>(this IEnumerable<T> _collection, Predicate<T> _Predicate)
            {
                foreach (var (index,value) in _collection.LoopIndex())
                {
                    if (!_Predicate(value))
                        continue;  
                    yield return index;
                }
            }
            
            public static IEnumerable<int> CollectAsIndex<T>(this IEnumerable<T> _collection, Func<int,T,bool> _Predicate)
            {
                foreach (var (index,value) in _collection.LoopIndex())
                {
                    if (!_Predicate(index,value))
                        continue;  
                    yield return index;
                }
            }

            public static void Traversal<T>(this IEnumerable<T> _collection, Action<T> _onEach, bool _copy = false)
            {
                if (_copy)
                    _collection = UList.Traversal(_collection);

                if (_collection is IList<T> list)
                {
                    var count = list.Count;
                    for (var i = 0; i < count; i++)
                        _onEach(list[i]);
                }
                else
                {
                    foreach (T element in _collection)
                        _onEach(element);
                }
            }
            public static void Traversal<T>(this IEnumerable<T> _collection, Action<int,T> _onEach)
            {
                if (_collection is IList<T> list)
                {
                    int count = list.Count;
                    for (int i = 0; i < count; i++)
                        _onEach(i,list[i]);
                }
                else
                {
                    int index = 0;
                    foreach (T element in _collection)
                        _onEach(index++,element);
                }
            }


            public static int LastIndex<T>(this IEnumerable<T> _collection,Predicate<T> _OnEachElement )
            {
                int index = -1;
                int countIndex = -1;
                foreach (T element in _collection)
                {
                    countIndex++;
                    if (_OnEachElement(element))
                        index = countIndex;
                }
                return index;
            }

            public static float MinIndex(this IEnumerable<float> _collection)
            {
                var minValue = float.MaxValue;
                var minIndex = -1;
                var index = 0;
                foreach (var element in _collection)
                {
                    if(minValue<element)
                        continue;
                    minValue = element;
                    minIndex = index;
                    index++;
                }
                return minIndex;
            }

            public static float Min<T>(this IEnumerable<T> _collection, Func<T, float> _getValue, out int _minIndex)
            {
                _minIndex = -1;
                float minValue = float.MaxValue;
                foreach (var (index,element) in _collection.LoopIndex())
                {
                    var value = _getValue(element);
                    if(minValue<value)
                        continue;
                    minValue = value;
                    _minIndex = index;
                }
                return minValue;
            }
            
            public static T MinElement<T>(this IEnumerable<T> _collection, Func<T, float> _getValue)
            {
                T minElement = default;
                float minValue = float.MaxValue;
                foreach (var (_,element) in _collection.LoopIndex())
                {
                    var value = _getValue(element);
                    if(minValue<=value)
                        continue;
                    minValue = value;
                    minElement = element;
                }
                return minElement;
            }
            public static T MinElement<T>(this IEnumerable<T> _collection, Func<T, float> _getValue,out int _minIndex)
            {
                T minElement = default;
                _minIndex = default;
                float minValue = float.MaxValue;
                foreach (var (index,element) in _collection.LoopIndex())
                {
                    var value = _getValue(element);
                    if(minValue<value)
                        continue;
                    minValue = value;
                    minElement = element;
                    _minIndex = index;
                }
                return minElement;
            }

            public static void MinmaxElement<T>(this IEnumerable<T> _collection, Func<T, float> _getValue,out T _minElement,out T _maxElement)
            {
                _minElement = default;
                _maxElement = default;
                var minValue = float.MaxValue;
                var maxValue = float.MinValue;
                foreach (var element in _collection)
                {
                    var value = _getValue(element);
                    if (minValue >= value)
                    {
                        minValue = value;
                        _minElement = element;
                    }

                    if (maxValue <= value)
                    {
                        maxValue = value;
                        _maxElement = element;                        
                    }
                }
            }
            
            public static void MinmaxIndex<T>(this IEnumerable<T> _collection, Func<T, float> _getValue,out int _minIndex,out int _maxIndex)
            {
                var minValue = float.MaxValue;
                var maxValue = float.MinValue;
                _minIndex = -1;
                _maxIndex = -1;
                var index = 0;
                foreach (var element in _collection)
                {
                    var value = _getValue(element);
                    if (minValue >= value)
                    {
                        minValue = value;
                        _minIndex = index;
                    }

                    if (maxValue <= value)
                    {
                        maxValue = value;
                        _maxIndex = index;                        
                    }
                    
                    index += 1;
                }
            }

            public static T MaxElement<T>(this IEnumerable<T> _collection, Func<T, float> _getValue)
            {
                T maxElement = default;
                float maxValue = float.MinValue;
                foreach (var (_,element) in _collection.LoopIndex())
                {
                    var value = _getValue(element);
                    if(maxValue>value)
                        continue;
                    maxValue = value;
                    maxElement = element;
                }
                return maxElement;
            }
            
            public static Y Find<T, Y>(this IEnumerable<T> _collection) where Y : T
            {
                foreach(var element in _collection)
                    if (element is Y target)
                        return target;
                return default;
            }
            
            public static int MaxIndex<T>(this IEnumerable<T> _collection, Func<T, float> _getValue)
            {
                var maxIndex = -1;
                var maxValue = float.MinValue;
                foreach (var (index,element) in _collection.LoopIndex())
                {
                    var value = _getValue(element);
                    if(maxValue>value)
                        continue;
                    maxValue = value;
                    maxIndex = index;
                }
                return maxIndex;
            }

            public static T Last<T>(this IEnumerable<T> _collection, Func<T, Vector3> _getPosition,Vector3 _origin,bool _minimum)
            {
                T elementRecorded = default;
                float distanceRecord = _minimum?float.MaxValue:float.MinValue;
                foreach (var element in _collection)
                {
                    var position = _getPosition(element);
                    float sqrDistance = (position - _origin).sqrMagnitude;
                    if (_minimum)
                    {
                        if (sqrDistance > distanceRecord)
                            continue;
                    }
                    else
                    {
                        if (sqrDistance < distanceRecord)
                            continue;
                    }

                    elementRecorded = element;
                    distanceRecord = sqrDistance;
                }
                return elementRecorded;
            }

            public static void Minmax(out float _min, out float _max,params float[] _values)
            {
                _min = float.MaxValue;
                _max = float.MinValue;
                foreach (var element in _values)
                {
                    if (_min > element)
                        _min = element;
                    if (_max < element)
                        _max = element;
                }
            }
            public static T Find<T>(this IEnumerable<T> _collection,Predicate<T> _OnEachElement)
            {
                foreach (var element in _collection)
                    if (_OnEachElement(element))
                        return element;
                return default;
            }

            public static bool TryFind<T>(this IEnumerable<T> _collection,Predicate<T> _OnEachElement, out T _element)
            {
                _element = default;
                foreach (var element in _collection)
                    if (_OnEachElement(element))
                    {
                        _element= element;
                        return true;
                    }
                return false;
            }

            public static T Find<T>(this IEnumerable<T> _collection, Predicate<T> _validate,out int index)
            {
                index = -1;
                foreach (var element in _collection)
                {
                    index++;
                    if (_validate(element))
                        return element;
                }
                index = -1;
                return default;
            }
            public static int FindIndex<T>(this IEnumerable<T> _collection, T _equaler)
            {
                int index = -1;
                foreach (T items in _collection)
                {
                    index++;
                    if (_equaler.Equals(items))
                        return index;
                }
                return -1;
            }
            
            public static int FindIndex<T>(this IEnumerable<T> _collection, Predicate<T> _OnEachElement)
            {
                int index = -1;
                foreach (T items in _collection)
                {
                    index++;
                    if (_OnEachElement(items))
                        return index;
                }
                return -1;
            }
            public static bool FindIndex<T>(this IEnumerable<T> _collection, Predicate<T> _OnEachElement,out int _index)
            {
                _index = FindIndex(_collection,_OnEachElement);
                return _index != -1;
            }
            
            public static string ToString<T>(this IEnumerable<T> _collections, char breakAppend, Func<T, string> OnEachAppend=null)
            {
                StringBuilder builder = new StringBuilder();
                int maxIndex = _collections.Count() - 1;
                int curIndex = 0;
                foreach (var element in _collections)
                {
                    builder.Append(OnEachAppend!=null?OnEachAppend(element):element.ToString());
                    if (curIndex != maxIndex)
                        builder.Append(breakAppend);
                    curIndex++;
                }
                return builder.ToString();
            }

            public static IEnumerable<T> Iterate<T>(this IList<T> _collection, int _startIndex,int _endIndex = -1)
            {
                if(_endIndex== -1)
                    _endIndex = _collection.Count;
                for (int i = _startIndex; i < _endIndex; i++)
                    yield return _collection[i];
            }

            public static void FillDefault<T>(this IList<T> _src, T _dst = default)
            {
                int length = _src.Count;
                for (int i = 0; i < length; i++)
                    _src[i] = _dst;
            }
            
            public static void FillArray<T>(this IList<T> _src, IList<T> _dst)
            {
                int length = _src.Count;
                for (int i = 0; i < length; i++)
                    _dst[i] =  _src[i];
            }

            public static void FillArray<T, Y>(this IList<T> _src, IList<Y> _dst, Func<T, Y> _convert)
            {
                int length = _src.Count;
                for (int i = 0; i < length; i++)
                {
                    var element = _src[i];
                    _dst[i] = _convert(element);
                }
            }
            public static int FillArray<T>(this IList<T> _src, IList<T> _dst, Predicate<T> _validate) 
            {
                int index = 0;
                int length = _src.Count;
                for (int i = 0; i < length; i++)
                {
                    var element = _src[i];
                    if(!_validate(element))
                        continue;
                    _dst[index++] = element;
                }
                return index;
            }        
            //Vector2 
            public static (Vector2 total,int count) Sum(this IEnumerable<Vector2> _collection)      //To Be Continued With Demical
            {
                Vector2 sum = Vector2.zero;
                int count = 0;
                foreach (var element in _collection)
                {
                    sum += element;
                    count += 1;
                }
                return (sum, count);
            }
            public static (Vector2 value, int count) Sum<T>(this IEnumerable<T> _collection, Func<T, Vector2> _OnEachElement)      //To Be Continued With Demical
            {
                Vector2 sum = Vector2.zero;
                int count = 0;
                foreach (var element in _collection)
                {
                    sum += _OnEachElement(element);
                    count += 1;
                }
                return (sum, count);
            }
            public static Vector2 Average<T>(this IEnumerable<T> _collection, Func<T, Vector2> _OnEachElement)      //To Be Continued With Demical
            {
                var tuple = _collection.Sum(_OnEachElement);
                return tuple.value / Mathf.Max(tuple.count,1);
            }
            public static Vector2 Average(this IEnumerable<Vector2> _collection)      //To Be Continued With Demical
            {
                var tuple = _collection.Sum();
                return tuple.total / Mathf.Max(tuple.count,1);
            }
            
            public static (Vector3 value, int count) Sum<T>(this IEnumerable<T> _collection, Func<T, Vector3> _OnEachElement)      //To Be Continued With Demical
            {
                Vector3 sum = Vector3.zero;
                int count = 0;
                foreach (var element in _collection)
                {
                    sum += _OnEachElement(element);
                    count += 1;
                }
                return (sum, count);
            }
            //Vector3 
            public static Vector3 Average<T>(this IEnumerable<T> _collection, Func<T, Vector3> _OnEachElement)      //To Be Continued With Demical
            {
                var tuple = _collection.Sum(_OnEachElement);
                return tuple.value / Mathf.Max(tuple.count,1);
            }
            
            //Vector3 
            public static Vector3 Average(this IEnumerable<Vector3> _collection)      //To Be Continued With Demical
            {
                Vector3 sum = Vector3.zero;
                int index = 0;
                foreach (var element in _collection)
                {
                    sum += element;
                    index++;
                }
                return index==0?Vector3.zero:sum / index;
            }
            
            public static T Closest<T>(this IEnumerable<T> _collection,Vector3 _targetPoint,Func<T,Vector3> _onEachElement)
            {
                T target = default;
                float maxSQRdst = float.MaxValue;
                foreach (var element in _collection)
                {
                    var pos = _onEachElement(element);
                    float sqrDst = (_targetPoint - pos).sqrMagnitude;
                    if (maxSQRdst > sqrDst)
                    {
                        maxSQRdst = sqrDst;
                        target = element;
                    }
                }
                return target;
            }
            
        #endregion
        #region Array
        public static IEnumerable<object> GetEnumerable(this Array _array)
        {
            foreach(var element in _array)
                yield return element;
        }
        public static T[] DeepCopy<T>(this T[] _srcArray)
        {
            T[] dstArray = new T[_srcArray.Length];
            for (int i = 0; i < _srcArray.Length; i++)
                dstArray[i] = _srcArray[i];
            return dstArray;
        }

        public static T[] MemberCopy<T>(this T[] _srcArray, T[] _dstArray)
        {
            for (int i = 0; i < _srcArray.Length; i++)
                _dstArray[i] = _srcArray[i];
            return _dstArray;
        }
        public static T[] Add<T>(this T[] _srcArray,T _element)
        {
            T[] newArray= new T[_srcArray.Length + 1];
            for (int i = 0; i < _srcArray.Length; i++)
                newArray[i] = _srcArray[i];
            newArray[_srcArray.Length] = _element;
            return newArray;
        }
        
        public static T[] Add<T>(this T[] _srcArray, T[] _tarArray)
        {
            int srcLength = _srcArray.Length;
            int tarLength = _tarArray.Length;
            T[] newArray = new T[srcLength + tarLength];
            for (int i = 0; i < srcLength; i++)
                newArray[i] = _srcArray[i];
            for (int i = 0; i < tarLength; i++)
                newArray[srcLength + i] = _tarArray[i];
            return newArray;
        }
        
        public static T[] RemoveLast<T>(this T[] _srcArray)
        {
            T[] dstArray = new T[_srcArray.Length-1];
            for (int i = 0; i < dstArray.Length; i++)
                dstArray[i] = _srcArray[i];
            return dstArray;
        }
        
        public static T[] Resize<T>(this T[] _srcArray,int _length,bool _fillWithLast=true)
        {
            T[] dstArray = new T[_length];
            if (_srcArray.Length == 0)
                return dstArray;
            
            for (int i = 0; i < dstArray.Length; i++)
            {
                if (i >= _srcArray.Length && !_fillWithLast)
                    break;
                dstArray[i] = _srcArray[Mathf.Min(i, Mathf.Max(_srcArray.Length - 1))];
            }
            return dstArray;
        }
        
        public static T Sum<T>( this T[] _collection,Func<T,T,T> _add) where T:struct
        {
            T sum = default;
            foreach (T element in _collection)
                sum = _add(sum, element);
            return sum;
        }

        public static T Average<T>( this T[] _collection,Func<T,T,T> _add,Func<T,int,T> _divide) where T:struct
        {
            return _divide(Sum(_collection,_add), _collection.Count());
        }
        public static bool Contains<T>(this T[] _collection, T _comparer) where T:IEquatable<T>
        {
            foreach (T element in _collection)
                if (element.Equals(_comparer))
                    return true;
            return false;
        }
        public static bool All<T>(this T[] _collection, Predicate<T> _comparer)
        {
            foreach (T element in _collection)
                if (!_comparer(element))
                    return false;
            return true;
        }
        public static bool Any<T>(this T[] _collection, Predicate<T> _comparer)
        {
            foreach (T element in _collection)
                if (_comparer(element))
                    return true;
            return false;
        }
        
        public static void SortIndex<T>(this T[] _array, IEnumerable<int> _indexes)
        {
            var tempList  = UList.Empty<T>();
            foreach (var index in _indexes)
                tempList.Add(_array[index]);

            for (int i = 0; i < tempList.Count; i++)
                _array[i] = tempList[i];
        }

        public static T Last<T>(this T[] _array)=> _array[^1];
        public static void FillArray<T>(this IEnumerable<T> _collection, T[] _array,int _offset=0)
        {
            int index=_offset;
            foreach (var element in _collection)
                _array[index++] = element;
        }

        public static int IndexOf<T>(this T[] _array, T _target, int _offset=0)
        {
            var count = _array.Length;
            for (int i = 0; i < count; i++)
            {
                int curIndex = (_offset + i) % count;
                if(!_array[curIndex].Equals(_target))
                    continue;
                return curIndex;
            }

            return -1;
        }
        
        public static T[] Remake<T>(this T[] _collection, Func<T, T> _convert)
        {
            for (int i = 0; i < _collection.Length; i++)
                _collection[i] = _convert(_collection[i]);
            return _collection;
        }
        public static T[] Remake<T>(this T[] _array, Func<int, T, T> _onEach)
        {
            var count = _array.Length;
            for (int i = 0; i < count; i++)
            {
                var element = _array[i];
                _array[i] = _onEach(i, element);
            }

            return _array;
        }
        

        public static T[] Cut<T>(this T[] _collection, int _startInclusive, int _endExclusive)
        {
            var template = new T[_endExclusive - _startInclusive];
            for (var i = 0; i < template.Length; i++)
                template[i] = _collection[i + _startInclusive];
            return template;
        }
        #endregion
        #region List
        public static List<T> DeepCopy<T>(this List<T> _list) where T:class
        {
            var copyList = new List<T>(_list.Count);
            foreach (var element in _list)
                copyList.Add(element.DeepCopyInstance());
            return copyList;
        }
        
        public static T LastElement<T>(this List<T> _list) => _list[_list.Count - 1];
        
        public static void RemoveLast<T>(this List<T> _list) => _list.RemoveAt(_list.Count - 1);
        
        public static void RemoveRange<T>(this List<T> _list, IEnumerable<T> _collections, bool containsCheck = false)
        {
            foreach (T element in _collections)
            {
                if(containsCheck&&!_list.Contains(element))
                    continue;
                _list.Remove(element);
            }
        }
        
        public static void RemoveRange<T>(this List<T> _list, T[] _array,int length)
        {
            for(int i=0;i<length;i++)
                _list.Remove(_array[length]);
        }

        public static bool TryAdd<T>(this List<T> _list, T _element)
        {
            if (_list.Contains(_element))
                return false;
            _list.Add(_element);
            return true;
        }
        
        public static void TryAddRange<T>(this List<T> _list, IEnumerable<T> _collection)
        {
            foreach (var element in _collection)
                _list.TryAdd(element);
        }

        public static bool TryRemove<T>(this List<T> _list, T _element)
        {
            if (!_list.Contains(_element))
                return false;
            _list.Remove(_element);
            return true;
        }

        public static void SortIndex<T>(this List<T> _list, IEnumerable<int> _indexes)
        {
            var tempList  = UList.Empty<T>();
            foreach (var index in _indexes)
                tempList.Add(_list[index]);
            _list.Clear();
            _list.AddRange(tempList);
        }
        
        public static void Reindex<T>(this List<T> _list,int _index)
        {
            for (int i = 0; i < _index; i++)
            {
                var first = _list[0];
                _list.RemoveAt(0);
                _list.Add(first);
            }
        }

        public static int FindLastIndex<T>(this List<T> _list, Comparison<T> _compare)
        {
            T current = _list[0];
            int index = 0;
            var count = _list.Count;
            for (int i = 1; i < count; i++)
            {
                var compare = _list[i];
                if(_compare(current,compare)!=1)
                    continue;
                current = compare;
                index = i;
            }
            return index;
        }

        
        public static List<T> Remake<T>(this List<T> _collection, Func<T, T> _convert)
        {
            for (int i = 0; i < _collection.Count; i++)
                _collection[i] = _convert(_collection[i]);
            return _collection;
        }

        public static (T start, T end, float value,float repeat) Gradient<T>(this IList<T> _collection,float _value)
        {
            Debug.Assert(_collection!=null, "collection can't be null");
            Debug.Assert(_collection.Count!=0, "collection can't be 0");
            Debug.Assert(_value>=0, "value must be non-negative");
                
            
            var count = _collection.Count;
            var start = (int)_value % count;
            var end = (start + 1) % count;
            return (_collection[start], _collection[end], _value%1,_value % count);
        }
        
        public static bool Contains<T>(this IList<T> _collection1, IList<T> _collection2) where T:IEquatable<T>
        {
            foreach (T element1 in _collection1)
            {
                foreach (T element2 in _collection2)
                    if (element1.Equals(element2))
                        return true;
            }
            return false;
        }

        public static byte Max(this IList<byte> _collection)
        {
            var maxByte = byte.MinValue;
            var count = _collection.Count;
            for (int i = 0; i < count; i++)
            {
                var compare = _collection[i];
                maxByte = maxByte < compare ? compare : maxByte;
            }
            return maxByte;

        }
        
        public static void FillHashset<T>(this IEnumerable<T> _collection, HashSet<T> _hashset,bool _sameCheck = false)
        {
            _hashset.Clear();
            foreach (var element in _collection)
            {
                if (_sameCheck)
                    _hashset.TryAdd(element);
                else
                    _hashset.Add(element);
            }
        }

        public static List<T> FillList<T>(this IEnumerable<T> _collection, List<T> _list,bool _sameCheck = false)
        {
            _list.Clear();
            foreach (var element in _collection)
            {
                if (_sameCheck)
                    _list.TryAdd(element);
                else
                    _list.Add(element);
            }

            return _list;
        }
        
        public static IList<T> FillList<T>(this IEnumerable<T> _collection, IList<T> _list)
        {
            _list.Clear();
            foreach (var element in _collection)
                _list.Add(element);

            return _list;
        }
        public static void AddRange<T>(this IList<T> _src, IList<T> _dst)
        {
            int count = _dst.Count;
            for (int i = 0; i < count; i++)
                _src.Add(_dst[i]);
        }
        
        public static void AddRange<T>(this IList<T> _src, IEnumerable<T> _collection)
        {
            foreach (var element in _collection)
                _src.Add(element);
        }

        public static void Resize<T>(this List<T> _src,int _newSize)
        {
            for(int i=_src.Count;i<_newSize;i++)
                _src.Add(default);
            
            for(int i=_src.Count-1;i>=_newSize;i--)
                _src.RemoveAt(i);
        }
        #endregion
        #region Dictionary

        public static T SelectKey<T, Y>(this Dictionary<T, Y> dictionary, int index) => dictionary.ElementAt(index).Key;
        
        public static Y SelectValue<T, Y>(this Dictionary<T, Y> dictionary, int index) => dictionary.ElementAt(index).Value;
        
        public static Dictionary<T, Y> DeepCopy<T, Y>(this Dictionary<T, Y> dictionary)
        {
            Dictionary<T, Y> copyDic = new Dictionary<T, Y>();
            foreach (var pair in copyDic)
                copyDic.Add(pair.Key,pair.Value);
            return copyDic;
        }
        
        public static void RemoveRange<T, Y>(this Dictionary<T, Y> _dic, IEnumerable<T> _keysCollections,
            bool containsCheck = false)
        {
            foreach (T key in _keysCollections)
            {
                if (containsCheck && !_dic.ContainsKey(key))
                    continue;
                _dic.Remove(key);
            }
        }

        public static bool TryAdd<T, Y>(this Dictionary<T, Y> _dic, T _key, Y _value)
        {
            if (_dic.ContainsKey(_key))
                return false;
            _dic.Add(_key,_value);
            return true;
        }
        public static bool TryAdd<T, Y>(this Dictionary<T, Y> _dic, T _key, Func<Y> _getValue)
        {
            if (_dic.ContainsKey(_key))
                return false;
            _dic.Add(_key,_getValue());
            return true;
        }
        
        public static bool TryRemove<T, Y>(this Dictionary<T, Y> _dic,  T _key)
        {
            if (!_dic.ContainsKey(_key))
                return false;
            _dic.Remove(_key);
            return true;
        }
        public static IEnumerable<(T key, Y value)> SelectPairs<T,Y>(this Dictionary<T,Y> dictionary)
        {
            foreach (var pair in dictionary)
                yield return (pair.Key, pair.Value);
        }
        #endregion
        #region Queue
        public static void EnqueueRange<T>(this Queue<T> _queue,IEnumerable<T> _collection)
        {
            foreach (var element in _collection)
                _queue.Enqueue(element);
        }
        #endregion
        #region Stack
        public static Stack<T> PushRange<T>(this Stack<T> _stack,IEnumerable<T> _collection)
        {
            foreach (var element in _collection)
                _stack.Push(element);
            return _stack;
        }

        public static bool TryPush<T>(this Stack<T> _stack, T _item)
        {
            if (_stack.Contains(_item))
                return false;
            _stack.Push(_item);
            return true;
        }
        #endregion
        #region HashSet

        public static void AddRange<T>(this HashSet<T> _hashSet, IEnumerable<T> _collections)
        {
            foreach (var element in _collections)
                _hashSet.Add(element);
        }  

        
        public static bool TryAdd<T>(this HashSet<T> _hashSet, T _element)
        {
            if (_hashSet.Contains(_element))
                return false;

            _hashSet.Add(_element);
            return true;
        }  
        
        
        public static void TryAddRange<T>(this HashSet<T> _hashSet,IEnumerable<T> _collections)
        {
            foreach (var element in _collections)
                _hashSet.TryAdd(element);
        }  
        public static bool TryRemove<T>(this HashSet<T> _hashSet, T _element)
        {
            if (!_hashSet.Contains(_element))
                return false;

            _hashSet.Remove(_element);
            return true;
        }

        public static void RemoveRange<T>(this HashSet<T> _hashSet, IEnumerable<T> _collection)
        {
            foreach (var element in _collection)
                _hashSet.Remove(element);
        }
        #endregion
        
        #region Decimal
        public static (float3 value,int count) Sum(this IEnumerable<float3> _collection)   
        {
            var sum = float3.zero;
            int count = 0;
            foreach (var element in _collection)
            {
                sum += element;
                count += 1;
            }
            return (sum,count);
        }
        public static float3 Average(this IEnumerable<float3> _collection) 
        {
            var tuple = _collection.Sum();
            return tuple.value / Mathf.Max(tuple.count,1);
        }    
        
        public static (float2 value,int count) Sum(this IEnumerable<float2> _collection) 
        {
            var sum = float2.zero;
            int count = 0;
            foreach (var element in _collection)
            {
                sum += element;
                count += 1;
            }
            return (sum,count);
        }
        
        public static float2 Average(this IEnumerable<float2> _collection)   
        {
            var tuple = _collection.Sum();
            return tuple.value / Mathf.Max(tuple.count,1);
        }
        
        #endregion
    }

}