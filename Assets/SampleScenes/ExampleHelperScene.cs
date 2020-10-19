using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExampleHelperScene : MonoBehaviour
{
    public static List<string> m_SceneNames { get; private set; } = new List<string>();
    protected void Awake()
    {
        for(int i=0;i<SceneManager.sceneCountInBuildSettings; i++)
        {
            if (i == 0)
                continue;
            m_SceneNames.Add(i.ToString());
        }
    }
    private void Start()
    {
        GameObject.DontDestroyOnLoad(this);
        UIT_TouchConsole.Instance.AddConsoleBinding().Play(m_SceneNames,0,ChangeScene);
        ChangeScene(m_SceneNames[0]);
    }

    public void ChangeScene(string name)
    {
        SceneManager.LoadScene(int.Parse(name), LoadSceneMode.Single);
    }
}
