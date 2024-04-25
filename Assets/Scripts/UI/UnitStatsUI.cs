using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitStatsUI : MonoBehaviour
{
    public HexGameUI gameUI;

    public Text nameTag;
    public Text descriptionTag;
    public Text healthTag;
    public Text movementTag;
    public Text statLine;

    void Update()
    {
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if(gameUI.selectedUnit != null)
        {
            HexUnit unit = gameUI.selectedUnit;

            nameTag.text = unit.HexUnitName;
            descriptionTag.text = unit.UnitDescription;

            healthTag.text = "HEALTH : " + unit.health;

            movementTag.text = "MOVEMENT : " + unit.moveSpeed;

            statLine.text = unit.PrintStatline();
        }
        else
        {
            nameTag.text = "No Unit Selected";
            descriptionTag.text = "You have not selected a unit";

            healthTag.text = "";

            movementTag.text = "";

            statLine.text = "";
        }
    }
}
