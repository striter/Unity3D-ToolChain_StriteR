using System;
using System.Collections.Generic;
using System.Linq;
using TPool;
using TObjectPool;
using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

public class SurfaceEffectController : MonoBehaviour
{
    public readonly List<EntityEffectAnimation> m_SurfaceAnimations = new List<EntityEffectAnimation>();
    private Renderer[] m_Renderers;
    private MeshFilter[] m_MeshFilters;
    private GameObject[] m_AnimateObjects;
    private Material[][] m_BaseMaterials;
    // private Light m_Light;
    private SurfaceEffectCollection _animCollection;
    private ObjectPoolClass<int,FadeElement> m_FadeOut;
    private void Update()
    {
        Tick(Time.deltaTime);
    }

    public void Initialize(SurfaceEffectCollection animCollection)
    {
        // m_Light = new GameObject("Light").AddComponent<Light>();
        // m_Light.transform.SetParent(transform);
        // m_Light.transform.localPosition = Vector3.up * .5f;
        // m_Light.transform.localRotation = Quaternion.identity;
        // m_Light.cullingMask = animCollection.m_LightLayer;

        _animCollection = animCollection;

        m_SurfaceAnimations.Clear();
        m_Renderers = GetComponentsInChildren<Renderer>();
        m_MeshFilters = m_Renderers.Select(p => p.GetComponent<MeshFilter>()).ToArray();
        m_AnimateObjects = m_Renderers.Select(p => p.gameObject).ToArray();//.Add(m_Light.gameObject);
        Debug.Assert(m_Renderers.Length != 0, "Invalid Length of renderers");
        m_BaseMaterials = m_Renderers.Select(p => p.sharedMaterials).ToArray();

        var fadeRoot = new GameObject("FadeRoot");
        fadeRoot.transform.SetParentAndSyncPositionRotation(transform);
        var fadeOutEntity = new GameObject("Item");
        fadeOutEntity.transform.SetParentAndSyncPositionRotation(fadeRoot.transform);
        m_FadeOut = new ObjectPoolClass<int, FadeElement>(fadeOutEntity.transform, () => new object[]{ m_Renderers,m_MeshFilters});
    }

    public void Clear()
    {
        m_SurfaceAnimations.Clear();
        Reforge();
    }
    
    public void Dispose()
    {
        m_SurfaceAnimations.Clear();
        _animCollection = null;
        m_FadeOut.Dispose();
        m_BaseMaterials = null;
    }

    public void Play(string name)
    {
        if (_animCollection == null)
            return;
        var clip = _animCollection.m_AnimationClips.Find(clip => clip.name == name);
        if (clip != null)
            Play(clip);
    }

    public void Play(EntityEffectClip _clip)
    {
        // Debug.LogWarning(_clip.name);
        if (m_SurfaceAnimations.TryFind(_p => _p.Clip == _clip, out var anim))
        {
            anim.Refresh();
            return;
        }

        m_SurfaceAnimations.Add(new EntityEffectAnimation(_clip));
        Reforge();
    }

    public bool PlayFade(string _clipName,Vector3 _velocity,out int _fadeHandle)
    {
        _fadeHandle = -1;
        var clip = _animCollection.m_AnimationClips.Find(_clip => _clip.name == _clipName);
        if (clip == null)
            return false;

        _fadeHandle = m_FadeOut.Count;
        m_FadeOut.Spawn().Play(new EntityEffectAnimation(clip),_velocity,m_Renderers);
        return true;
    }

    public bool PlayFade(string _clipName, Vector3 _velocity) {
        var clip = _animCollection.m_AnimationClips.Find(_clip => _clip.name == _clipName);
        if (clip == null)
            return false;

        m_FadeOut.Spawn().Play(new EntityEffectAnimation(clip), _velocity, m_Renderers);
        return true;
    }

    public bool StopFade(int _fadeHandle) => m_FadeOut.TryRecycle(_fadeHandle);

    public void Stop(string _name)
    {
        if (_animCollection == null)
            return;
        var clip = _animCollection.m_AnimationClips.Find(_clip => _clip.name == _name);
        if (clip != null)
            Stop(clip);
    }

    public void Stop(EntityEffectClip _clip)
    {       
        // Debug.LogError(_clip.name);
        if (m_SurfaceAnimations.TryFind(_p => _p.Clip == _clip, out var anim))
        {
            m_SurfaceAnimations.Remove(anim);
            Reforge();
        }
    }

    public void StopAll()
    {
        m_SurfaceAnimations.Clear();
        Reforge();
    }

    public void Tick(float _deltaTime)
    {
        for (int i = m_SurfaceAnimations.Count - 1; i >= 0; i--)
        {
            var animation = m_SurfaceAnimations[i];
            animation.Tick(_deltaTime, m_AnimateObjects, out var recycle);
            if (recycle)
            {
                m_SurfaceAnimations.RemoveAt(i);
                Reforge();
            }
        }

        TSPoolList<int>.Spawn(out var expired);
        if (m_FadeOut == null)
            return;
        foreach (var fade in m_FadeOut.m_Dic.Keys)
        {
            if(m_FadeOut[fade].Tick(_deltaTime))
                expired.Add(fade);
        }

        foreach (var expire in expired)
            m_FadeOut.Recycle(expire);
        
        TSPoolList<int>.Recycle(expired);
    }

    void Reforge()      //Reforge
    {
        var array = m_SurfaceAnimations.Select(p => p.Clip.material).ToArray();
        // m_Light.enabled = array.Length > 0;
        for (int i = 0; i < m_BaseMaterials.Length; i++)
            m_Renderers[i].sharedMaterials = m_BaseMaterials[i].Concat(array).ToArray();
    }
}

public class EntityEffectAnimation : IEquatable<EntityEffectAnimation>
{
    public float timeElapsed;
    public EntityEffectClip Clip;

    public EntityEffectAnimation(EntityEffectClip _clip)
    {
        Clip = _clip;
        timeElapsed = 0f;
    }

    public void Tick(float _deltaTime, GameObject[] _objects, out bool _selfRecycle)
    {
        timeElapsed += _deltaTime;
        foreach (var p in _objects)
            Clip.animation.SampleAnimation(p, timeElapsed);
        _selfRecycle = Clip.animation.wrapMode == WrapMode.Once && timeElapsed > Clip.animation.length;
    }

    public void Refresh()
    {
        timeElapsed = 0f;
    }

    public bool Equals(EntityEffectAnimation other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return timeElapsed.Equals(other.timeElapsed) && Equals(Clip, other.Clip);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != this.GetType())
            return false;
        return Equals((EntityEffectAnimation)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (timeElapsed.GetHashCode() * 397) ^ (Clip != null ? Clip.GetHashCode() : 0);
        }
    }
}

public class FadeElement:ITransformHandle,IPoolCallback<int>
{
    struct FadeMesh
    {
        public Mesh mesh;
        public bool isSkinned;
        public MeshRenderer renderer;
    }
    FadeMesh[] m_Renderers;
    GameObject[] m_AnimateObjects;
    public Transform Transform { get; }
    public EntityEffectAnimation m_Animation;
    public Vector3 m_Position { get; private set; }
    public Quaternion m_Rotation { get; private set; }
    public Vector3 m_Velocity { get; private set; }

    public void OnPoolCreate(Action<int> _doRecycle)
    {
    }

    public void OnPoolSpawn(int _identity)
    {
    }

    
    public FadeElement(Transform _transform, Renderer[] _renderers, MeshFilter[] _filters)
    {
        Transform = _transform;
        m_Renderers = new FadeMesh[_renderers.Length];
        m_AnimateObjects = new GameObject[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
        {
            var originRenderer = _renderers[i];
            var originFilter = _filters[i];
            bool isSkinned = originRenderer is SkinnedMeshRenderer;
            GameObject element = new GameObject(originRenderer.name);
            m_AnimateObjects[i] = element;
            element.transform.SetParentAndSyncPositionRotation(_transform);
            var filter = element.AddComponent<MeshFilter>();
            var renderer = element.AddComponent<MeshRenderer>();
            var mesh = isSkinned ? new Mesh() { name = $"{originRenderer.name}(Fade)", hideFlags = HideFlags.HideAndDontSave } : originFilter.sharedMesh;
            filter.sharedMesh = mesh;

            m_Renderers[i] = new FadeMesh()
            {
                mesh = filter.sharedMesh,
                isSkinned = isSkinned,
                renderer = renderer,
            };
        }
    }
    public void OnPoolDispose()
    {
        for (int i = 0; i < m_Renderers.Length; i++)
        {
            var fadeMesh = m_Renderers[i];
            if (fadeMesh.isSkinned)
                UnityEngine.Object.Destroy(fadeMesh.mesh);
            fadeMesh.mesh = null;
            fadeMesh.renderer = null;
        }
    }

    public void Play(EntityEffectAnimation _animation, Vector3 _velocity, Renderer[] _renderers)
    {
        m_Position = Transform.position;
        m_Rotation = Transform.rotation;
        m_Velocity = _velocity;
        m_Animation = _animation;
        for (int i = 0; i < _renderers.Length; i++)
        {
            var fadeMesh = m_Renderers[i];
            if (fadeMesh.isSkinned)
                (_renderers[i] as SkinnedMeshRenderer).BakeMesh(fadeMesh.mesh);
            fadeMesh.renderer.transform.SetPositionAndRotation(_renderers[i].transform.position, _renderers[i].transform.rotation);
            fadeMesh.renderer.sharedMaterial = _animation.Clip.material;
        }
    }

    
    public bool Tick(float _deltaTime)
    {
        m_Animation.Tick(_deltaTime,m_AnimateObjects,out var recycle);
        Transform.position = m_Position + m_Velocity * m_Animation.timeElapsed;
        Transform.rotation = m_Rotation;
        return recycle;   
    }

    public void OnPoolRecycle()
    {
        for (int i = 0; i < m_Renderers.Length; i++)
        {
            var fadeMesh = m_Renderers[i];
            if (fadeMesh.isSkinned)
                fadeMesh.mesh.Clear();
        }
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(SurfaceEffectController))]
public class SurfaceEffectControllerEditor : Editor
{
    private SurfaceEffectController m_Controller;
    private void OnEnable() {
        m_Controller = target as SurfaceEffectController;
    }

    private void OnDisable() {
        m_Controller = null;
    }

    private string fadeAnim = "Fade";
    private Vector3 fadeVelocity = Vector3.forward * 5f;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.BeginVertical();
        foreach (var animation in m_Controller.m_SurfaceAnimations)
            GUILayout.Label($"{animation.Clip.name}({animation.Clip.animation.wrapMode}):{animation.timeElapsed:F2}");

        if (!Application.isPlaying)
            return;
        
        fadeAnim = GUILayout.TextField(fadeAnim);
        fadeVelocity = EditorGUILayout.Vector3Field("Velocity", fadeVelocity);
        if (GUILayout.Button("Play Fade"))
            m_Controller.PlayFade(fadeAnim, fadeVelocity, out var handle);
        EditorGUILayout.EndVertical();
    }
}
#endif