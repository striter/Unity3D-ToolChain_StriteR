using UnityEngine;


[ExecuteInEditMode, RequireComponent(typeof(Camera))]
public class CameraController : SingletonMono<CameraController>
{
    public Camera m_Camera { get; private set; }
    public Transform m_BindRoot;
    [Header("Position Param")]
    public Vector3 m_BindPosOffset;
    [Range(0f, 5f)]
    public float m_MoveDamping = 1f;
    [Header("Rotation Param")]
    public Transform m_ForceLookAt;
    [Range(0f, 5f)]
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

        UpdateCameraPositionRotation(m_MoveDamping, m_RotateDamping, Time.deltaTime);
    }
    Matrix4x4 GetBindRootMatrix() => Matrix4x4.TRS(m_BindRoot.position, m_BindRoot.rotation, Vector3.one);
    void UpdateCameraPositionRotation(float _moveDamping, float _rotateDamping, float _deltaTime)
    {
        if (!m_BindRoot)
            return;

        Matrix4x4 rootMatrix = GetBindRootMatrix();
        Vector3 cameraPosition = rootMatrix.MultiplyPoint(m_BindPosOffset + GetRootOffsetAdditive(_deltaTime));
        transform.position = Vector3.Lerp(transform.position, cameraPosition, _moveDamping==0?1f: _deltaTime / _moveDamping);

        Vector3 cameraEuler = m_ForceLookAt ? Quaternion.LookRotation(m_ForceLookAt.position - transform.position, Vector3.up).eulerAngles : m_BindRoot.eulerAngles;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(cameraEuler + GetRootRotateAdditive(_deltaTime)),_rotateDamping==0?1f: _deltaTime / _rotateDamping);
    }

    public void ForceSetCamera() => UpdateCameraPositionRotation(1f, 1f, 1f);
    protected virtual Vector3 GetRootOffsetAdditive(float _deltaTime) => Vector3.zero;
    protected virtual Vector3 GetRootRotateAdditive(float _deltaTime) => Vector3.zero;
#if UNITY_EDITOR
    #region Editor
    public bool m_Gizmos=true;
    private void OnDrawGizmos()
    {
        if (!m_BindRoot || !m_Gizmos)
            return;

        if(!Application.isPlaying)
        ForceSetCamera();


        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(m_Camera.transform.position,.2f);
        Gizmos_Extend.DrawArrow(m_Camera.transform.position, m_Camera.transform.rotation,.8f,.2f);

        Gizmos.color = Color.green;
        Gizmos_Extend.DrawCylinder(m_BindRoot.position, Quaternion.LookRotation(Vector3.up), .2f, 2f);

        Gizmos.matrix = GetBindRootMatrix();
        Gizmos.DrawWireSphere(m_BindPosOffset, .5f);
    }
    #endregion
#endif
}
