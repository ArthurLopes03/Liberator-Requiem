using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScrip : MonoBehaviour
{
    public string gameSceneString;
    public string tutorialSceneString;

    public void startGame()
    {
        SceneManager.LoadScene(gameSceneString);
    }

    public void startTutorial()
    {
        SceneManager.LoadScene(tutorialSceneString);
    }

    public void exitGame()
    {
        Application.Quit();
    }
}
