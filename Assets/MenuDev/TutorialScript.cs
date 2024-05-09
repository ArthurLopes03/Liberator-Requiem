using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialScript : MonoBehaviour
{
    public SuperTextMesh sptext;
    public string[] dialogue;

    public string goBackToSceneString;

    public int currentText;

    // Update is called once per frame
    private void Start()
    {
        currentText = 0;
        
        sptext.text = dialogue[currentText];
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
        {
            if (!sptext.reading)
            {
                if (currentText >= dialogue.Length - 1)
                {
                    SceneManager.LoadScene(goBackToSceneString);
                }
                else
                {
                    currentText++;

                    sptext.text = dialogue[currentText];

                    sptext.Rebuild();
                }
            }
            else
            {
                sptext.SkipToEnd();
            }

        }
        
        
    }
}
