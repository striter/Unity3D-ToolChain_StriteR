using System.Collections;
using System.Collections.Generic;
using Rendering.ImageEffect;
using UnityEngine;

public class PostEffectTest : MonoBehaviour
{
    private void Awake()
    {
        TouchInputManager.Instance.Init(new TouchCheckDown(OnTouchCheck,OnPressCheck));
    }

    void OnTouchCheck(bool down, Vector2 pos)
    {
        PostEffect_DepthCircleArea area = GetComponentInChildren<PostEffect_DepthCircleArea>();
        Camera areaCamera = area.GetComponent<Camera>();

        if (areaCamera.InputRayCheck(pos, out RaycastHit _hit))
            area.SetDepthAreaCircle(down, _hit.point, 2.5f, 0.2f, .2f);

        if (!down)
            return;

        PostEffect_DepthCircleScan scan = GetComponentInChildren<PostEffect_DepthCircleScan>();
        Camera scanCamera = scan.GetComponent<Camera>();
        if (scanCamera.InputRayCheck(pos, out _hit))
            scan.StartDepthScanCircle(_hit.point,Color.green,.5f,10f,.5f);
    }
    void OnPressCheck(Vector2 pos)
    {
        PostEffect_DepthCircleArea area = GetComponentInChildren<PostEffect_DepthCircleArea>();
        Camera areaCamera = area.GetComponent<Camera>();
        if (areaCamera.InputRayCheck(pos, out RaycastHit _hit))
            GetComponentInChildren<PostEffect_DepthCircleArea>().SetAreaOrigin(_hit.point);

    }
}
