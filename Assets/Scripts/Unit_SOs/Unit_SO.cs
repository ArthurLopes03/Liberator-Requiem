using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct unitStats
{
    public string HexUnitName;

    public string UnitDescription;

    public int range;

    public int moveSpeed;

    public int attackPow;

    public int defence;

    public int health;

    public Texture[] animatedTextures;
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/InfantryUnit_SO", order = 1)]
public class Unit_SO : ScriptableObject
{
    public unitStats unitStats;
}
