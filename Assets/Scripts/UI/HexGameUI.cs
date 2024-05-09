using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HexGameUI : MonoBehaviour {

	public HexGrid grid;

	public HexCell currentCell;

	public HexUnit selectedUnit;

	public bool playerOneTurn = true;

	public string currentTurnTag = "Player 1 Turn";

	public int turnCounter = 1;

	private GameObject[] playerTwoUnits;

    public void SetEditMode (bool toggle) 
	{
		enabled = !toggle;
		grid.ShowUI(!toggle);
		grid.ClearPath();
	}

	void Update () 
	{
		if (!EventSystem.current.IsPointerOverGameObject()) {
			if (Input.GetMouseButtonDown(0)) {
				DoSelection();
			}
			else if (selectedUnit) {
				if(Input.GetMouseButtonDown(1) && currentCell.hasUnit)
                {
					DoAttack();
				}
				else if (Input.GetMouseButtonDown(1)) {
					DoMove();
				}
				else {
					DoPathfinding();
				}
			}
		}
	}

	void DoSelection () 
	{
		if (playerOneTurn)
		{
			grid.ClearUnitHighlight();
			grid.ClearPath();
			UpdateCurrentCell();
				if (currentCell)
				{
					if ( currentCell.Unit && currentCell.Unit.gameObject.tag == "PlayerOneUnit" && (currentCell.Unit.canMove && currentCell.Unit.canAttack))
						{ selectedUnit = currentCell.Unit; }
					else 
					{
						selectedUnit = null;
					}
				}
		}
        if (!playerOneTurn)
        {
            grid.ClearUnitHighlight();
            grid.ClearPath();
            UpdateCurrentCell();
            if (currentCell)
            {
	            if (currentCell && currentCell.Unit.gameObject.tag == "PlayerTwoUnit" && (currentCell.Unit.canMove || currentCell.Unit.canAttack))
					{ selectedUnit = currentCell.Unit; }
				else
				{ 
					Debug.Log("Cannot Select That");
                    selectedUnit = null;
                }
            }
        }
    }

	void DoPathfinding () 
	{
		if (UpdateCurrentCell()) {
            if (currentCell && currentCell.hasUnit)
            {
				grid.ClearPath();
				grid.ClearUnitHighlight();
                grid.HighlightUnit(selectedUnit.Location, currentCell);
            }
            else if (currentCell && selectedUnit.IsValidDestination(currentCell)) {
				grid.FindPath(selectedUnit.Location, currentCell, 10 * selectedUnit.moveSpeed, selectedUnit.canMove);
			}
			else {
				grid.ClearPath();
			}
		}
	}

	void DoAttack()
	{
		if(selectedUnit.canAttack && selectedUnit.Location.coordinates.DistanceTo(currentCell.coordinates) <= selectedUnit.range)
		{
            selectedUnit.Attack(currentCell.Unit);
        }
    }

	void DoMove () 
	{
        if (grid.HasPath && grid.toTileTurn <= 0)
        {
            selectedUnit.Location = currentCell;
            grid.ClearPath();

            selectedUnit.canMove = false;
        }
	}

	bool UpdateCurrentCell () 
	{
		HexCell cell =
			grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
		if (cell != currentCell) {
			currentCell = cell;
			return true;
		}
		return false;
	}

	public void NextTurn()
    {
		grid.ClearPath();
		grid.ClearUnitHighlight();

		selectedUnit = null;
		currentCell = null;

        GameObject[] playerOneUnits = GameObject.FindGameObjectsWithTag("PlayerOneUnit");
        playerTwoUnits = GameObject.FindGameObjectsWithTag("PlayerTwoUnit");


        if (playerOneTurn)
		{
			playerOneTurn = false;
			currentTurnTag = "Player 2 Turn";

            foreach (GameObject u in playerTwoUnits)
            {
                u.GetComponent<HexUnit>().canAttack = true;
                u.GetComponent<HexUnit>().canMove = true;
            }

            DoAITurn();
		}
		else if (!playerOneTurn)
		{ 
			playerOneTurn = true;
			currentTurnTag = "Player 1 Turn";
			turnCounter++;

            foreach (GameObject u in playerOneUnits)
            {
                u.GetComponent<HexUnit>().canAttack = true;
                u.GetComponent<HexUnit>().canMove = true;
            }
        }
	}

	// Processes the AI's turn
	public void DoAITurn()
	{
		// Iterates through every unit
		foreach (GameObject u in playerTwoUnits)
		{
			HexUnit unit = u.GetComponent<HexUnit>();

			HexCell closestVictoryPoint = FindClosestVictoryPoint(unit);

            List<HexCell> possibleMoves = null;


            if (closestVictoryPoint != null)
			{
                possibleMoves = grid.AISearch(unit.Location, closestVictoryPoint, unit.moveSpeed * 10);
				Debug.Log("Possible moves = " +  possibleMoves.Count);
            }

			if( possibleMoves != null && possibleMoves.Count > 0)
			{
                HexCell moveTarget = FindBestMove(possibleMoves, closestVictoryPoint, unit);

				Debug.Log("Moving Target");
                unit.Location = moveTarget;
            }
		}

		NextTurn();
	}

	[SerializeField]
	HexCell best;
	private HexCell FindBestMove(List<HexCell> cells, HexCell target, HexUnit unit)
	{
		best = null;
		int bestValue = int.MinValue;
		foreach(HexCell cell in cells)
		{
            if (best == null) { best = cell; }
			else
			{
				int value = 0;

				value += cell.UrbanLevel + cell.FarmLevel + cell.PlantLevel + cell.Elevation;

				if (cell.HasRiver)
				{
					value += 2;
				}

				if (cell.VictoryPoint)
				{
					value += 30;
				}

				if(HexCoordinates.IsEqual(unit.Location.coordinates, cell.coordinates))
				{
					value -= 5;
				}

				value -= cell.coordinates.DistanceTo(target.coordinates);

                if (value > bestValue)
				{
					best = cell;
					bestValue = value;
				}
			}
		}
		return best;
	}

	// Finds the closest victory point to that unit
	private HexCell FindClosestVictoryPoint(HexUnit unit)
	{

        HexCell closestVictoryPoint = null;

		if( grid.player1VictoryPoints == null || grid.player1VictoryPoints.Count <= 0) 
		{
			Debug.Log("No victorypoint found");
			return null; 
		}

		// Searches through every player victory point
		foreach (HexCell cell in grid.player1VictoryPoints)
		{
			if (closestVictoryPoint == null) { closestVictoryPoint = cell; }
			else
			{
				// Checks if the current cell is closer to the unit than the last one
				if( unit.Location.coordinates.DistanceTo(cell.coordinates) > unit.Location.coordinates.DistanceTo(closestVictoryPoint.coordinates))
				{ closestVictoryPoint = cell; }
			}
		}

		return closestVictoryPoint;
	}
}