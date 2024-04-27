using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healthManager : MonoBehaviour
{
    private HexUnit unit;
    private Material mat;

    public float maxHealth = 10f;
    // Start is called before the first frame update
    void Start()
    {
        unit = gameObject.GetComponentInParent<HexUnit>();
        mat = gameObject.GetComponent<Renderer>().material;

        maxHealth = unit.health;
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.eulerAngles = new Vector3(0, 0, 0);
        
        mat.SetFloat("_MaxHealth", maxHealth);
        mat.SetFloat("_Health", unit.health);
    }
}
