using UnityEngine;
using UnityEngine.UI;

public class UIWorldBase : MonoBehaviour
{
    public static T Attach<T>(Transform toTrans) where T : UIWorldBase
    {
        T template = TResources.Instantiate<T>("UI/World/" + typeof(T).ToString(), toTrans);
        template.Init();
        return template;
    }
    public bool B_AutoRotate = true;
    protected Transform tf_Container;
    protected RectTransform rtf_Canvas;
    public virtual void Init()
    {
        if (rtf_Canvas)
            return;
        rtf_Canvas = transform.Find("Canvas").GetComponent<RectTransform>();
        tf_Container = rtf_Canvas.transform.Find("Container");
    }
    protected virtual void Update()
    {
        if(B_AutoRotate)
            transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane( transform.position- CameraController.MainCamera.transform.position, CameraController.MainCamera.transform.right), CameraController.MainCamera.transform.up);
    }
    protected void Hide()
    {
        Destroy(this.gameObject);
    }

}
