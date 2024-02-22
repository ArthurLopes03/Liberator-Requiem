using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour {

	public Color[] colors;

	public HexGrid hexGrid;

	int activeElevation;

	Color activeColor;

	int brushSize;

    bool isDrag;
    HexDirection dragDirection;
    HexCell previousCell;

    enum OptionalToggle
    {
        Ignore, Yes, No
    }

    OptionalToggle riverMode;

    bool applyColor;
	bool applyElevation = true;

	public void SelectColor (int index) {
		applyColor = index >= 0;
		if (applyColor) {
			activeColor = colors[index];
		}
	}

	public void SetApplyElevation (bool toggle) {
		applyElevation = toggle;
	}

	public void SetElevation (float elevation) {
		activeElevation = (int)elevation;
	}

	public void SetBrushSize (float size) {
		brushSize = (int)size;
	}

	public void ShowUI (bool visible) {
		hexGrid.ShowUI(visible);
	}

	void Awake () {
		SelectColor(0);
	}

	void Update () {
		if (
			Input.GetMouseButton(0) &&
			!EventSystem.current.IsPointerOverGameObject()
		) {
			HandleInput();
		}
        else
        {
			// There is no previous cell
            previousCell = null;
        }
    }

	void HandleInput () {
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit))
            {
				//Gets the cell from the raycast hit point
                HexCell currentCell = hexGrid.GetCell(hit.point);
				
			    //If the current cell and previous cell are not the same as the current cell, it is passed to ValidateDrag
				if (previousCell && previousCell != currentCell)
				{
					ValidateDrag(currentCell);
				}
				else
				{
					isDrag = false;
				}

				//The cell is passed to EditCells
				EditCells(currentCell);

				//once its done editing it becomes the previous cell
                previousCell = currentCell;
            }
        else
        {
			// There is no previous cell
            previousCell = null;
        }
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
			if (applyColor) {
				cell.Color = activeColor;
			}
			if (applyElevation) {
				cell.Elevation = activeElevation;
			}

			//Adds outoging river if the option is enabled and there is a drag
            if (riverMode == OptionalToggle.No)
            {
                cell.RemoveRiver();
            }
            else if (isDrag && riverMode == OptionalToggle.Yes)
            {
                HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                if (otherCell)
                {
                    otherCell.SetOutgoingRiver(dragDirection);
                }
            }
        }
	}

	// Adjusts river mode
    public void SetRiverMode(int mode)
    {
        riverMode = (OptionalToggle)mode;
    }

	// Returns whether or not the drag is valid by iterating through each neighbor
    void ValidateDrag(HexCell currentCell)
    {
		// Cycles through each direction
        for (
            dragDirection = HexDirection.NE;
            dragDirection <= HexDirection.NW;
            dragDirection++
        )
        {
			// If the current cell is a neightbor of the previous cell, the drag is valid
            if (previousCell.GetNeighbor(dragDirection) == currentCell)
            {
                isDrag = true;
                return;
            }
        }
		//Otherwise it is not
        isDrag = false;
    }
}