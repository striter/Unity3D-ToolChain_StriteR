using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rendering.Optimize;
public class AnimationInstanceTest : MonoBehaviour
{
    public int m_X=100,m_Y=100;
    public int m_Anim = 0;
    public GameObject m_Prefab;
    List<AnimationInstanceController> m_Controllers=new List<AnimationInstanceController>();
    private void Awake()
    {
        for(int i=0;i<m_X;i++)
        {
            for(int j=0;j<m_Y;j++)
            {
                AnimationInstanceController controller = GameObject.Instantiate(m_Prefab,transform).GetComponent<AnimationInstanceController>().Init(Debug.Log);
                controller.transform.localPosition = new Vector3(i*10,0, j * 10);
                controller.SetAnimation(m_Anim).SetScale(Random.value);
                m_Controllers.Add(controller);
            }
        }
    }

    private void Update()
    {
        float _deltaTime = Time.deltaTime;
        foreach(AnimationInstanceController item in m_Controllers)
        {
            item.Tick(_deltaTime);
        }
    }
}
