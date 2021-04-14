using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExampleHelperScene : MonoBehaviour
{
    public static List<string> m_SceneNames { get; private set; } = new List<string>();
    private void Start()
    {
        GameObject.DontDestroyOnLoad(this);
        for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++)
            m_SceneNames.Add(GetSceneName( SceneUtility.GetScenePathByBuildIndex(i)));
        UIT_TouchConsole.Command("Select Scene").EnumSelection(0,  m_SceneNames, ChangeScene);
        ChangeScene(m_SceneNames[0]);
    }
    static string GetSceneName(string scenePath)
    {
        int start = scenePath.LastIndexOf('/')+1;
        int end = scenePath.LastIndexOf('.');
        return scenePath.Substring(start,end-start);

    }
    public void ChangeScene(string name)
    {
        SceneManager.LoadScene(name, LoadSceneMode.Single);
    }
}
