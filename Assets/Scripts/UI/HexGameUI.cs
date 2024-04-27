using UnityEngine;
using UnityEngine.EventSystems;

public class HexGameUI : MonoBehaviour {

	public HexGrid grid;

	HexCell currentCell;

	public HexUnit selectedUnit;

	public bool playerOneTurn = true;

	public HexUnit[] unitPrefabs;

	HexUnit currentUnit;

	public string currentTurnTag = "Player 1 Turn";

	public int turnCounter = 1;

    private void Awake()
    {
        currentUnit = unitPrefabs[0];
		HexUnit.unitPrefab = currentUnit;
    }

	public void SetUnitType(int index)
	{
		currentUnit = unitPrefabs[index];
        HexUnit.unitPrefab = currentUnit;
    }

    public void SetEditMode (bool toggle) {
		enabled = !toggle;
		grid.ShowUI(!toggle);
		grid.ClearPath();
	}

	void Update () {
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

	void DoSelection () {
		if (playerOneTurn)
		{
			grid.ClearUnitHighlight();
			grid.ClearPath();
			UpdateCurrentCell();
				if (currentCell)
				{
					if ( currentCell.Unit && currentCell.Unit.gameObject.tag == "PlayerOneUnit")
						{ selectedUnit = currentCell.Unit; }
					else 
						{ Debug.Log("Cannot Select That"); }
				}
		}
        if (!playerOneTurn)
        {
            grid.ClearUnitHighlight();
            grid.ClearPath();
            UpdateCurrentCell();
            if (currentCell)
            {
	            if (currentCell && currentCell.Unit.gameObject.tag == "PlayerTwoUnit")
					{ selectedUnit = currentCell.Unit; }
				else
					{ Debug.Log("Cannot Select That"); }
            }
        }
    }

	void DoPathfinding () {
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

	void DoMove () {
        if (grid.HasPath && grid.toTileTurn <= 0)
        {
            selectedUnit.Location = currentCell;
            grid.ClearPath();

            selectedUnit.canMove = false;
        }
	}

	bool UpdateCurrentCell () {
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

		if (playerOneTurn)
		{
			playerOneTurn = false;
			currentTurnTag = "Player 2 Turn";
		}
		else if (!playerOneTurn)
		{ 
			playerOneTurn = true;
			currentTurnTag = "Player 1 Turn";
			turnCounter++;
		}


		GameObject[] playerOneUnits = GameObject.FindGameObjectsWithTag("PlayerOneUnit");

		foreach(GameObject u in playerOneUnits)
        {
			u.GetComponent<HexUnit>().canAttack = true;
			u.GetComponent<HexUnit>().canMove = true;
		}

		GameObject[] playerTwoUnits = GameObject.FindGameObjectsWithTag("PlayerTwoUnit");

		foreach (GameObject u in playerTwoUnits)
		{
			u.GetComponent<HexUnit>().canAttack = true;
			u.GetComponent<HexUnit>().canMove = true;
		}
	}

}