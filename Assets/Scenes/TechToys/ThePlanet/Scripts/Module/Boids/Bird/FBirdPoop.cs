using System;
using TechToys.ThePlanet.Module.BOIDS;
using Rendering.Pipeline;
using TPool;
using UnityEngine;

public class FBirdPoop : ITransform , IPoolCallback<int>
{
    public Transform transform { get; }

    private Counter m_PoopRecycler = new Counter(15f);
    public Action<int> DoRecycle { get; set; }
    public int identity { get; set; }

    private MeshRenderer m_Mesh;
    private MaterialPropertyBlock m_Block = new MaterialPropertyBlock();
    private Color m_BaseColor;

    private Transform m_Sticking;
    private Vector3 m_LocalPosition;
    public Quaternion m_LocalRotation;
    
    public FBirdPoop(Transform _transform)
    {
        transform = _transform;
        m_Mesh = _transform.GetComponentInChildren<MeshRenderer>();
        m_BaseColor = m_Mesh.sharedMaterial.GetColor(KShaderProperties.kColor);
    }

    public void OnPoolCreate()
    {
    }

    public void OnPoolSpawn()
    {
        m_PoopRecycler.Replay();
    }

    public FBirdPoop Initialize(Vector3 _positionWS,Quaternion _rotationWS,Transform _stickOnTo)
    {
        transform.position = _positionWS;
        transform.rotation = _rotationWS;
        m_Sticking = _stickOnTo;
        if (!m_Sticking)
            return this;
        var worldToLocalMatrix = _stickOnTo.worldToLocalMatrix;
        m_LocalPosition = worldToLocalMatrix.MultiplyPoint(_positionWS);
        m_LocalRotation = worldToLocalMatrix.rotation * _rotationWS;
        return this;
    }
    
    public void Tick(float _deltaTime)
    {
        if (m_PoopRecycler.TickTrigger(_deltaTime))
            DoRecycle(identity);
        m_Block.SetColor(KShaderProperties.kColor,m_BaseColor.SetA(m_PoopRecycler.TimeScale));
        m_Mesh.SetPropertyBlock(m_Block);

        if (m_Sticking == null)
            return;
        var localToWorldMatrix = m_Sticking.localToWorldMatrix;
        transform.SetPositionAndRotation(localToWorldMatrix.MultiplyPoint(m_LocalPosition),localToWorldMatrix.rotation*m_LocalRotation);
    }

    public void OnPoolRecycle()
    {
        m_Sticking = null;
    }

    public void OnPoolDispose()
    {
        m_PoopRecycler = null;
    }
}
