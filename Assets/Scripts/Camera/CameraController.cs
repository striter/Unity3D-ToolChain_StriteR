using UnityEngine;


[ExecuteInEditMode,RequireComponent(typeof(Camera))]
public class CameraController : SingletonMono<CameraController>
{

    public Camera m_Camera { get; private set; }
    [Header("Position Param")]
    [SerializeField]
    protected Transform m_BindRoot;
    [SerializeField]
    protected Vector3 m_BindPosOffset;
    [Range(0.01f, 5f)]
    public float m_MoveDamping = 1f;
    [Header("Rotation Param")]
    [SerializeField]
    protected Transform m_LookAt;
    [SerializeField]
    protected Vector3 m_InputRotEuler;
    [SerializeField]
    protected RangeFloat m_InputPitchClamp;
    [Range(0.01f, 5f)]
    public float m_RotateDamping = 1f;

    protected override void Awake()
    {
        base.Awake();
        m_Camera = GetComponent<Camera>();
    }
    protected virtual void LateUpdate()
    {
        if (!m_BindRoot)
            return;

        UpdateCameraPositionRotation(m_MoveDamping, m_RotateDamping,Time.deltaTime);
    }
    Matrix4x4 GetBindRootMatrix() => Matrix4x4.TRS(m_BindRoot.position, Quaternion.Euler(0, m_InputRotEuler.y, 0), Vector3.one);
    void UpdateCameraPositionRotation(float _moveDamping, float _rotateDamping,float _deltaTime)
    {
        Matrix4x4 rootMatrix = GetBindRootMatrix(); 
        Vector3 cameraPosition = rootMatrix.MultiplyPoint(m_BindPosOffset + GetRootOffsetAdditive());
        transform.position = Vector3.Lerp(transform.position, cameraPosition, _deltaTime / _moveDamping); ;

        Vector3 cameraEuler = m_LookAt ? Quaternion.LookRotation(m_LookAt.position - transform.position, Vector3.up).eulerAngles : m_InputRotEuler;
        
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(cameraEuler + GetRootRotateAdditive()), _deltaTime / _rotateDamping); 
    }

    public void ForceSetCamera() => UpdateCameraPositionRotation(1f, 1f,1f);
    public CameraController SetCameraBindRoot(Transform _bindRoot)
    {
        m_BindRoot = _bindRoot;
        return this;
    }
    public CameraController LookAt(Transform lookAtTrans)
    {
        m_LookAt = lookAtTrans;
        return this;
    }
    public void AddPitchYawRollDelta(Vector3 _delta) 
    {
        m_InputRotEuler += _delta;
        m_InputRotEuler.y = Mathf.Clamp(m_InputRotEuler.y, m_InputPitchClamp.start, m_InputPitchClamp.length);   
    }
    protected virtual Vector3 GetRootOffsetAdditive() => Vector3.zero;
    protected virtual Vector3 GetRootRotateAdditive() => Vector3.zero;


#if UNITY_EDITOR
    #region Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos_Extend.DrawCylinder(m_BindRoot.position,Quaternion.LookRotation( Vector3.up), .2f,2f);

        Gizmos.matrix = GetBindRootMatrix();
        Gizmos.DrawWireSphere(m_BindPosOffset ,.5f);
    }
    #endregion
#endif
}
