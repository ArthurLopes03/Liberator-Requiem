using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class healthManager : MonoBehaviour
{
    private HexUnit _unit;
    private Material _mat;
    private float _incommingDamage;

    private HexGameUI _uiManager;

    public float maxHealth = 10f;
    // Start is called before the first frame update
    void Start()
    {
        _unit = gameObject.GetComponentInParent<HexUnit>();
        _mat = gameObject.GetComponent<Renderer>().material;

        _uiManager = FindAnyObjectByType<HexGameUI>(FindObjectsInactive.Include).GetComponent<HexGameUI>();

        _incommingDamage = 0f;

        maxHealth = _unit.health;
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.eulerAngles = new Vector3(0, 0, 0);

        CheckForHighlightDamage();

        _mat.SetFloat("_MaxHealth", maxHealth);
        _mat.SetFloat("_Health", _unit.health);
        _mat.SetFloat("_ShowDamage", _incommingDamage);
    }

    void CheckForHighlightDamage()
    {
        if (_uiManager.selectedUnit != null)
        {


            if (_uiManager.currentCell.Unit == _unit && _uiManager.selectedUnit.gameObject.tag != _unit.gameObject.tag && _uiManager.selectedUnit.canAttack)
            {

                _incommingDamage = _uiManager.selectedUnit.attackPow * (1 - _unit.defence * 0.01f);

            }
            else
            {
                _incommingDamage = 0;
            }
        }
        else
        {
            _incommingDamage = 0;
        }
    }
}
