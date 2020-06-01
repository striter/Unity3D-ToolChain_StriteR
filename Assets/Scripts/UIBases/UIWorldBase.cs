using UnityEngine;
using UnityEngine.UI;

public class UIWorldBase : MonoBehaviour, ICoroutineHelperClass
{
    public static T Attach<T>(Transform toTrans,bool useAnim=true) where T : UIWorldBase
    {
        T template = TResources.Instantiate<T>("UI/World/" + typeof(T).ToString(), toTrans);
        template.Init(useAnim);
        return template;
    }
    public bool B_AutoRotate = true;
    protected Transform tf_Container;
    protected RectTransform rtf_Canvas;
    public virtual void Init(bool useAnim)
    {
        if (rtf_Canvas)
            return;
        rtf_Canvas = transform.Find("Canvas").GetComponent<RectTransform>();
        tf_Container = rtf_Canvas.transform.Find("Container");
        if (useAnim)
            this.StartSingleCoroutine(0,TIEnumerators.ChangeValueTo((float value) => { tf_Container.localScale=Vector3.one*value; }, 0, 1, .5f));
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
