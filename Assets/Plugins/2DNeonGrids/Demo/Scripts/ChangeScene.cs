using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    private int maxScenes;

    private void Start()
    {
        maxScenes = 5;
    }

    public void LoadGridDemoScene()
    {
        SceneManager.LoadScene("DemoRandomizer");
    }

    public void LoadSpriteMaskDemoScene()
    {
        SceneManager.LoadScene("DemoSpriteMasks");
    }

    public void LoadNextScene()
    {
        var currentSceneName = SceneManager.GetActiveScene().name;
        var currentSceneNumber = int.Parse(currentSceneName.Split('-')[1]);
        if (currentSceneNumber >= maxScenes)
        {
            SceneManager.LoadScene("2DNeonGrids-1");
        }
        else
        {
            var next = "2DNeonGrids-" + (currentSceneNumber + 1);
            SceneManager.LoadScene(next);
        }
    }
}