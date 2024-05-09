using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;

public class HexMapEditor : MonoBehaviour {

	public HexGrid hexGrid;

	public Material terrainMaterial;

	int activeElevation;
	int activeWaterLevel;

	int activeUrbanLevel, activeFarmLevel, activePlantLevel, activeSpecialIndex;

	int activeTerrainTypeIndex;

	int brushSize;

	bool applyElevation = true;
	bool applyWaterLevel = true;

	bool applyUrbanLevel, applyFarmLevel, applyPlantLevel, applySpecialIndex, applyVictoryPoint;

	int victoryPointHolder;

	public HexUnit hexUnitPrefabP1, hexUnitPrefabP2;

	int playerUnitSelector = 1;

    public Unit_SO[] unitTypes;

    Unit_SO currentUnitType;

    enum OptionalToggle {
		Ignore, Yes, No
	}

	OptionalToggle riverMode, roadMode, walledMode;

	bool isDrag;
	HexDirection dragDirection;
	HexCell previousCell;

	public void SetTerrainTypeIndex (int index) {
		activeTerrainTypeIndex = index;
	}

	public void SetApplyElevation (bool toggle) {
		applyElevation = toggle;
	}

	public void SetElevation (float elevation) {
		activeElevation = (int)elevation;
	}

	public void SetApplyWaterLevel (bool toggle) {
		applyWaterLevel = toggle;
	}

	public void SetWaterLevel (float level) {
		activeWaterLevel = (int)level;
	}

	public void SetApplyUrbanLevel (bool toggle) {
		applyUrbanLevel = toggle;
	}

	public void SetUrbanLevel (float level) {
		activeUrbanLevel = (int)level;
	}

	public void SetApplyFarmLevel (bool toggle) {
		applyFarmLevel = toggle;
	}

	public void SetFarmLevel (float level) {
		activeFarmLevel = (int)level;
	}

	public void SetApplyPlantLevel (bool toggle) {
		applyPlantLevel = toggle;
	}

	public void SetPlantLevel (float level) {
		activePlantLevel = (int)level;
	}

	public void SetApplySpecialIndex (bool toggle) {
		applySpecialIndex = toggle;
	}

	public void SetSpecialIndex (float index) {
		activeSpecialIndex = (int)index;
	}

	public void SetBrushSize (float size) {
		brushSize = (int)size;
	}

	public void SetRiverMode (int mode) {
		riverMode = (OptionalToggle)mode;
	}

	public void SetRoadMode (int mode) {
		roadMode = (OptionalToggle)mode;
	}

	public void SetWalledMode (int mode) {
		walledMode = (OptionalToggle)mode;
	}

	public void SetApplyVictoryPoint (bool toggle) {
		applyVictoryPoint = toggle;
	}

	public void SetVictoryPointHolder (float holder) {
		Debug.Log(holder);
		victoryPointHolder = (int)holder;
	}

	public void SetEditMode (bool toggle) {
		enabled = toggle;
	}

	public void SetPlayerUnit (int toggle)
	{
		playerUnitSelector = toggle;
	}

	public void ShowGrid (bool visible) {
		if (visible) {
			terrainMaterial.EnableKeyword("GRID_ON");
		}
		else {
			terrainMaterial.DisableKeyword("GRID_ON");
		}
	}

	void Awake () {
		terrainMaterial.DisableKeyword("GRID_ON");
		SetEditMode(false);

        currentUnitType = unitTypes[0];
    }

    void Update () {
		if (!EventSystem.current.IsPointerOverGameObject()) {
			if (Input.GetMouseButton(0)) {
				HandleInput();
				return;
			}
			if (Input.GetKeyDown(KeyCode.U)) {
				if (Input.GetKey(KeyCode.LeftShift)) {
					DestroyUnit();
				}
				else {
					CreateUnit();
				}
				return;
			}
		}
		previousCell = null;
	}

	HexCell GetCellUnderCursor () {
		return
			hexGrid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
	}

    public void SetUnitType(int index)
    {
        currentUnitType = unitTypes[index];
    }

    void CreateUnit () {
		HexCell cell = GetCellUnderCursor();
		if (cell && !cell.Unit && playerUnitSelector == 1) {
			hexGrid.AddUnit(
				Instantiate(hexUnitPrefabP1), cell, Random.Range(0f, 360f), currentUnitType, playerUnitSelector
			);
		}
        else if (cell && !cell.Unit && playerUnitSelector == 2)
        {
            hexGrid.AddUnit(
                Instantiate(hexUnitPrefabP2), cell, Random.Range(0f, 360f), currentUnitType, playerUnitSelector
            );
        }
    }

	void DestroyUnit () {
		HexCell cell = GetCellUnderCursor();
		if (cell && cell.Unit) {
			hexGrid.RemoveUnit(cell.Unit);
		}
	}

	void HandleInput () {
		HexCell currentCell = GetCellUnderCursor();
		if (currentCell) {
			if (previousCell && previousCell != currentCell) {
				ValidateDrag(currentCell);
			}
			else {
				isDrag = false;
			}
			EditCells(currentCell);
			previousCell = currentCell;
		}
		else {
			previousCell = null;
		}
	}

	void ValidateDrag (HexCell currentCell) {
		for (
			dragDirection = HexDirection.NE;
			dragDirection <= HexDirection.NW;
			dragDirection++
		) {
			if (previousCell.GetNeighbor(dragDirection) == currentCell) {
				isDrag = true;
				return;
			}
		}
		isDrag = false;
	}

	void EditCells (HexCell center) {
		int centerX = center.coordinates.X;
		int centerZ = center.coordinates.Z;

		for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++) {
			for (int x = centerX - r; x <= centerX + brushSize; x++) {
				EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
			}
		}
		for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++) {
			for (int x = centerX - brushSize; x <= centerX + r; x++) {
				EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
			}
		}
	}

	void EditCell (HexCell cell) {
		if (cell) {
			if (activeTerrainTypeIndex >= 0) {
				cell.TerrainTypeIndex = activeTerrainTypeIndex;
			}
			if (applyElevation) {
				cell.Elevation = activeElevation;
			}
			if (applyWaterLevel) {
				cell.WaterLevel = activeWaterLevel;
			}
			if (applySpecialIndex) {
				cell.SpecialIndex = activeSpecialIndex;
			}
			if (applyUrbanLevel) {
				cell.UrbanLevel = activeUrbanLevel;
			}
			if (applyFarmLevel) {
				cell.FarmLevel = activeFarmLevel;
			}
			if (applyPlantLevel) {
				cell.PlantLevel = activePlantLevel;
			}
			if (riverMode == OptionalToggle.No) {
				cell.RemoveRiver();
			}
			if (roadMode == OptionalToggle.No) {
				cell.RemoveRoads();
			}
			if (walledMode != OptionalToggle.Ignore) {
				cell.Walled = walledMode == OptionalToggle.Yes;
			}
			if (isDrag) {
				HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
				if (otherCell) {
					if (riverMode == OptionalToggle.Yes) {
						otherCell.SetOutgoingRiver(dragDirection);
					}
					if (roadMode == OptionalToggle.Yes) {
						otherCell.AddRoad(dragDirection);
					}
				}
			}
            if (applyVictoryPoint)
            {
                cell.VictoryPoint = applyVictoryPoint;
                cell.VictoryPointHolder = victoryPointHolder;

                switch (victoryPointHolder)
                {
                    case 0:
                        if (!hexGrid.player1VictoryPoints.Exists(x => HexCoordinates.IsEqual(x.coordinates, cell.coordinates)))
                            hexGrid.player1VictoryPoints.Add(cell);
                        return;
                    case 1:
                        if (!hexGrid.player2VictoryPoints.Exists(x => HexCoordinates.IsEqual(x.coordinates, cell.coordinates)))
                            hexGrid.player2VictoryPoints.Add(cell);
                        return;
                }
            }
            if (!applyVictoryPoint)
            {
                cell.VictoryPoint = applyVictoryPoint;

                switch (cell.VictoryPointHolder)
                {
                    case 0:
                        if (hexGrid.player1VictoryPoints.Exists(x => HexCoordinates.IsEqual(x.coordinates, cell.coordinates)))
                            hexGrid.player1VictoryPoints.Remove(cell);
                        return;
                    case 1:
                        if (hexGrid.player2VictoryPoints.Exists(x => HexCoordinates.IsEqual(x.coordinates, cell.coordinates)))
                            hexGrid.player2VictoryPoints.Remove(cell);
                        return;
                }
            }
        }
	}
}