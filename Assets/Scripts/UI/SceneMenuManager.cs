using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMenuManager : MonoBehaviour
{
    public void LoadNextScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void exitGame()
    {
        Application.Quit();
    }
}
