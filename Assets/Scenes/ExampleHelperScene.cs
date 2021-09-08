using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExampleHelperScene : MonoBehaviour
{
    public static List<string> m_SceneNames { get; private set; } = new List<string>();

    private void Start()
    {
        for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++)
            m_SceneNames.Add(GetSceneName( SceneUtility.GetScenePathByBuildIndex(i)));
        UIT_TouchConsole.InitDefaultCommands();
        UIT_TouchConsole.NewPage("Select");
        UIT_TouchConsole.Command("Scene").EnumSelection(0,  m_SceneNames, index=> ChangeScene(m_SceneNames[index]));
        Application.targetFrameRate = 60;
        GameObject.DontDestroyOnLoad(this);
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
