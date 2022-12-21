using UnityEngine;

namespace Examples.Rendering.ViewHeat
{
    public class ViewHeatManager : MonoBehaviour
    {
        Transform cameraRoot;
        private float pitch;
        private float yaw;
        private void Awake()
        {
            cameraRoot = transform.Find("CameraRoot");
            GetComponentsInChildren<ViewHeatContainer>().Traversal(p=>p.Init());
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            pitch += -Mathf.Clamp(Input.GetAxis("Mouse Y"),-1,1)*deltaTime*90f;
            yaw += Mathf.Clamp(Input.GetAxis("Mouse X"),-1,1)*deltaTime*90f;
            pitch = Mathf.Clamp(pitch, -45, 45);
            cameraRoot.transform.rotation=Quaternion.Euler(pitch,yaw,0);

            if(Input.GetKeyDown(KeyCode.R))
                GetComponentsInChildren<ViewHeatContainer>().Traversal(p=>p.Clear());
                
            if (!Input.GetMouseButton(0))
                return;

            var ray = Camera.main.ViewportPointToRay(Vector2.one*.5f);
            Debug.DrawRay(ray.origin,ray.direction);
            if(!Physics.Raycast(ray,out var hit,float.MaxValue))
                return;
            var container=hit.collider.GetComponent<ViewHeatContainer>();
            container.HeatUp(hit.point,deltaTime);
            
        }
    }

}