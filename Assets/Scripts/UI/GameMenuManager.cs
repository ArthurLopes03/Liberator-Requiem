using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMenuManager : MonoBehaviour
{
    public GameObject MapEditorMenu;
    public GameObject HexGameUI;

    bool caseSwitch = false;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        { SwitchMenu(); }
    }


    void SwitchMenu()
    {
        switch(caseSwitch)
        {
            case true:
                MapEditorMenu.SetActive(true);
                HexGameUI.SetActive(false);
                caseSwitch = false;
                return;
            case false:
                MapEditorMenu.SetActive(false);
                HexGameUI.SetActive(true);
                caseSwitch = true;
                return;
        }
    }
}
