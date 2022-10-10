using System;
using System.Collections;
using UnityEngine;
public class TResources
{
    public static class EditorDefaultResources
    { static T LoadEditorResources<T>(string _path) where T:UnityEngine.Object
        {
            T temp = null;
#if UNITY_EDITOR
            temp = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(_path);
#endif
            return temp;
        }
        
        public static readonly PassiveInstance<Texture3D> Noise3D = new PassiveInstance<Texture3D>(() => LoadEditorResources<Texture3D>( "Assets/ArtPreset/Textures/Noise/Texture3D_Noise_1.asset"),null);

        public static readonly PassiveInstance<Texture2D> Noise2D = new PassiveInstance<Texture2D>(() =>LoadEditorResources<Texture2D>("Assets/ArtPreset/Textures/Noise/Texture_Noise_1.jpg"),null);

        public static readonly PassiveInstance<Texture2D> Ramp = new PassiveInstance<Texture2D>(() =>LoadEditorResources<Texture2D>("Assets/ArtPreset/Textures/Ramp/Texture_Ramp1D_Linear0.png"),null);
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