using UnityEngine;
using System.IO;
using JetBrains.Annotations;
using Unity.VisualScripting;

public class HexUnit : MonoBehaviour {

	public string HexUnitName;

	public string UnitDescription;

	public int range;

	public int moveSpeed;

	public bool canMove = true;

	public bool canAttack = true;

	public int attackPow;

	public int defence;

	public int health;

	public int player;

	public int unitId;

	public changeAnimations changeAnimations;

	public Unit_SO[] unitTypes;

	public void SetPlayer(int player)
	{
		this.player = player;
	}

	//Changes the unit's stats to those of the SO
    public void SetType(Unit_SO unitType)
    {
        health = unitType.unitStats.health;
		range = unitType.unitStats.range;
		moveSpeed = unitType.unitStats.moveSpeed;
		attackPow = unitType.unitStats.attackPow;
		defence = unitType.unitStats.defence;
		HexUnitName = unitType.unitStats.HexUnitName;
		UnitDescription = unitType.unitStats.UnitDescription;
		unitId = unitType.unitStats.UnitID;

		changeAnimations.animatedTextures = unitType.unitStats.animatedTextures;

		if(GetComponent<Transform>().tag == "PlayerOneUnit")
		{
			player = 0;
		}

        if (GetComponent<Transform>().tag == "PlayerTwoUnit")
        {
            player = 1;
        }
    }

    public HexCell Location {
		get {
			return location;
		}
		set {
			if (location) {
				location.Unit = null;
			}
			location = value;
			value.Unit = this;
			transform.localPosition = value.Position;

			if(location.VictoryPoint && location.VictoryPointHolder != player)
			{
				location.VictoryPointHolder = player;
				location.Refresh();

				HexGrid grid;
				if(grid = GetComponentInParent<HexGrid>())
				{
					switch(player)
					{
						case 0:
							grid.player2VictoryPoints.Remove(location);
							grid.player1VictoryPoints.Add(location);
							return;
						case 1:
                            grid.player1VictoryPoints.Remove(location);
                            grid.player2VictoryPoints.Add(location);
                            return;
                    }
				}
			}
		}
	}

	HexCell location;

	public float Orientation {
		get {
			return orientation;
		}
		set {
			orientation = value;
			transform.localRotation = Quaternion.Euler(0f, value, 0f);
		}
	}

	float orientation;

	public void ValidateLocation () {
		transform.localPosition = location.Position;
	}

	public bool IsValidDestination (HexCell cell) {
		return !cell.IsUnderwater && !cell.Unit;
	}

	public void Die () {
		location.Unit = null;
		Destroy(gameObject);
	}

	
	public void Save (BinaryWriter writer) {
		location.coordinates.Save(writer);
		writer.Write(orientation);

		writer.Write((byte)unitId);

		writer.Write((byte)player);
	}
	

	
	public static void Load (BinaryReader reader, HexGrid grid) {
		HexCoordinates coordinates = HexCoordinates.Load(reader);
		float orientation = reader.ReadSingle();
        int unitId = reader.ReadByte();

		int player = reader.ReadByte();

		if (player == 0)
		{
			grid.AddUnit(
				Instantiate(grid.hexUnitPrefabP1), grid.GetCell(coordinates), orientation, unitId
			);
		}
        if (player == 1)
        {
            grid.AddUnit(
                Instantiate(grid.hexUnitPrefabP2), grid.GetCell(coordinates), orientation, unitId
            );
        }
    }
	

	public bool IsInRange (HexUnit enemyUnit)
    {
        if (Location.coordinates.DistanceTo(enemyUnit.Location.coordinates) <= range)
        {
			return true;
        }
		return false;
    }
    public void Attack(HexUnit enemyUnit)
    {
        HexCell enemyPosition = enemyUnit.Location;

        int attackTotal = Mathf.FloorToInt(attackPow * (float)Random.Range(1f,1f));

		int defenseTotal = enemyUnit.defence + enemyPosition.UrbanLevel + enemyPosition.PlantLevel;

		if(enemyPosition.HasRiver)
		{
			defenseTotal += 2;
		}

		if(location.Elevation < enemyPosition.Elevation)
		{
            defenseTotal += enemyPosition.Elevation - location.Elevation;
        }
		else if( location.Elevation > enemyPosition.Elevation )
		{
			attackTotal += location.Elevation - enemyPosition.Elevation;
		}

		int totalDamage = Mathf.FloorToInt(attackTotal * (1 - defenseTotal * 0.01f));

		if( totalDamage < 1 ) { totalDamage = 1; }

        enemyUnit.health -= totalDamage;
		
		canAttack = false;

		if (enemyUnit.health <= 0 )
		{
			enemyUnit.Die();
		}

		Debug.Log("Attacked for " + totalDamage);
	}

	public string PrintStatline()
	{
		string statline = "";

		statline += "ATK : " + attackPow;
		statline += "\nDFS : " + defence;
		statline += "\nRNG : " + range;

		return statline;
	}

	public void ChangeAnimation(int anim)
	{
		changeAnimations.currentAnimation = anim;
	}
}