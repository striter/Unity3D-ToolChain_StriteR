using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class TResources
{
    public static class EditorDefaultResources
    {
        static Texture3D _Noise3D = null;
        public static Texture3D Noise3D
        {
            get
            {
#if UNITY_EDITOR
                if(!_Noise3D)
                    _Noise3D =  UnityEditor.AssetDatabase.LoadAssetAtPath<Texture3D>("Assets/ArtPreset/Textures/Noise/Texture3D_Noise_1.asset");
#endif
                return _Noise3D;
            }
        }

        static Texture2D _Noise2D = null;
        public static Texture2D Noise2D
        {
            get
            {
#if UNITY_EDITOR
                if (!_Noise2D)
                    _Noise2D = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ArtPreset/Textures/Noise/Texture_Noise_1.jpg");
#endif
                return _Noise2D;
            }
        }
        static Texture2D _Ramp = null;
        public static Texture2D Ramp
        {
            get
            {
#if UNITY_EDITOR
                if (!_Ramp)
                    _Ramp = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ArtPreset/Textures/Ramp/Texture_Ramp1D_Linear0.png");
#endif
                return _Ramp;
            }
        }
    }


    public static GameObject Instantiate(string path, Transform toParent=null)
    {
        GameObject obj = Resources.Load<GameObject>(path);
        if (obj == null)
            throw new Exception("Null Path Of :Resources/" + path.ToString());
        return UnityEngine.Object.Instantiate(obj, toParent);
    }
    public static T Instantiate<T>(string path, Transform toParent = null) where T : UnityEngine.Component=> Instantiate(path, toParent).GetComponent<T>();
    
    public static T Load<T>(string path) where T : UnityEngine.Object
    {
        T prefab = Resources.Load<T>(path);
        if (prefab == null)
            Debug.LogWarning("Invalid Item Found Of |"+typeof(T)+  "|At:" + path);
        return prefab;
    }

    public static T[] LoadAll<T>(string path) where T : UnityEngine.Object
    {
        T[] array = Resources.LoadAll<T>(path);

        if (array.Length == 0)
            Debug.LogWarning("No InnerItems At:" + path);
        return array;
    }

    public static IEnumerator LoadAsync<T>(string resourcePath, Action<T> OnLoadFinished) where T : UnityEngine.Object
    {
        ResourceRequest request = Resources.LoadAsync<T>(resourcePath);
        yield return request;
        if (request.isDone && request.asset != null)
            OnLoadFinished(request.asset as T);
        else
            Debug.LogError("Null Path Of: Resources/" + resourcePath);
    }

}