using UnityEngine;
using System.IO;
using JetBrains.Annotations;

public class HexUnit : MonoBehaviour {

	public string HexUnitName;

	public string UnitDescription;

	public static HexUnit unitPrefab;

	public int range;

	public int moveSpeed;

	public bool canMove = true;

	public bool canAttack = true;

	public int attackPow;

	public int defence;

	public int health;

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
	}

	public static void Load (BinaryReader reader, HexGrid grid) {
		HexCoordinates coordinates = HexCoordinates.Load(reader);
		float orientation = reader.ReadSingle();
		grid.AddUnit(
			Instantiate(unitPrefab), grid.GetCell(coordinates), orientation
		);
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
}