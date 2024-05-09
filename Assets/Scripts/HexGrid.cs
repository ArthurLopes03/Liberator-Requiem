using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class HexGrid : MonoBehaviour {

	public int cellCountX = 20, cellCountZ = 15;

	public HexCell cellPrefab;
	public Text cellLabelPrefab;
	public HexGridChunk chunkPrefab;
    public HexUnit hexUnitPrefabP1, hexUnitPrefabP2;

    public List<HexCell> player1VictoryPoints;
    public List<HexCell> player2VictoryPoints;

    public Texture2D noiseSource;

	public int seed;

	public int toTileTurn;

	public bool HasPath {
		get {
			return currentPathExists;
		}
	}

	HexGridChunk[] chunks;
	HexCell[] cells;

	int chunkCountX, chunkCountZ;

	HexCellPriorityQueue searchFrontier;

	int searchFrontierPhase;

	HexCell currentPathFrom, currentPathTo;
	bool currentPathExists;

	[SerializeField]
	List<HexUnit> units = new List<HexUnit>();

	void Awake () {
		HexMetrics.noiseSource = noiseSource;
		HexMetrics.InitializeHashGrid(seed);
		CreateMap(cellCountX, cellCountZ);
		player1VictoryPoints = new List<HexCell>();
		player2VictoryPoints = new List<HexCell>();
	}

	public void AddUnit (HexUnit unit, HexCell location, float orientation, int unitType) {
		units.Add(unit);
		unit.transform.SetParent(transform, false);
		unit.Location = location;
		unit.Orientation = orientation;
		unit.SetType(unit.unitTypes[unitType]);
	}

	public void RemoveUnit (HexUnit unit) {
		units.Remove(unit);
		unit.Die();
	}

	public bool CreateMap (int x, int z) {
		if (
			x <= 0 || x % HexMetrics.chunkSizeX != 0 ||
			z <= 0 || z % HexMetrics.chunkSizeZ != 0
		) {
			Debug.LogError("Unsupported map size.");
			return false;
		}

		ClearPath();
		ClearUnits();
		if (chunks != null) {
			for (int i = 0; i < chunks.Length; i++) {
				Destroy(chunks[i].gameObject);
			}
		}

		cellCountX = x;
		cellCountZ = z;
		chunkCountX = cellCountX / HexMetrics.chunkSizeX;
		chunkCountZ = cellCountZ / HexMetrics.chunkSizeZ;
		CreateChunks();
		CreateCells();
		return true;
	}

	void CreateChunks () {
		chunks = new HexGridChunk[chunkCountX * chunkCountZ];

		for (int z = 0, i = 0; z < chunkCountZ; z++) {
			for (int x = 0; x < chunkCountX; x++) {
				HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
				chunk.transform.SetParent(transform);
			}
		}
	}

	void CreateCells () {
		cells = new HexCell[cellCountZ * cellCountX];

		for (int z = 0, i = 0; z < cellCountZ; z++) {
			for (int x = 0; x < cellCountX; x++) {
				CreateCell(x, z, i++);
			}
		}
	}

	void ClearUnits () {
		for (int i = 0; i < units.Count; i++) {
			units[i].Die();
		}
		units.Clear();
	}

	void OnEnable () {
		if (!HexMetrics.noiseSource) {
			HexMetrics.noiseSource = noiseSource;
			HexMetrics.InitializeHashGrid(seed);
		}
	}

	public HexCell GetCell (Ray ray) {
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {
			return GetCell(hit.point);
		}
		return null;
	}

	public HexCell GetCell (Vector3 position) {
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index =
			coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
		return cells[index];
	}

	public HexCell GetCell (HexCoordinates coordinates) {
		int z = coordinates.Z;
		if (z < 0 || z >= cellCountZ) {
			return null;
		}
		int x = coordinates.X + z / 2;
		if (x < 0 || x >= cellCountX) {
			return null;
		}
		return cells[x + z * cellCountX];
	}

	public void ShowUI (bool visible) {
		for (int i = 0; i < chunks.Length; i++) {
			chunks[i].ShowUI(visible);
		}
	}

	void CreateCell (int x, int z, int i) {
		Vector3 position;
		position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
		position.y = 0f;
		position.z = z * (HexMetrics.outerRadius * 1.5f);

		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

		if (x > 0) {
			cell.SetNeighbor(HexDirection.W, cells[i - 1]);
		}
		if (z > 0) {
			if ((z & 1) == 0) {
				cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
				if (x > 0) {
					cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
				}
			}
			else {
				cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
				if (x < cellCountX - 1) {
					cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
				}
			}
		}

		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.anchoredPosition =
			new Vector2(position.x, position.z);
		cell.uiRect = label.rectTransform;

		cell.Elevation = 0;

		AddCellToChunk(x, z, cell);
	}

	void AddCellToChunk (int x, int z, HexCell cell) {
		int chunkX = x / HexMetrics.chunkSizeX;
		int chunkZ = z / HexMetrics.chunkSizeZ;
		HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

		int localX = x - chunkX * HexMetrics.chunkSizeX;
		int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
		chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
	}

	public void Save (BinaryWriter writer) {
		writer.Write(cellCountX);
		writer.Write(cellCountZ);

		for (int i = 0; i < cells.Length; i++) {
			cells[i].Save(writer);
		}
		writer.Write(units.Count);
		for (int i = 0; i < units.Count; i++) {
			units[i].Save(writer);
		}
	}

	public void Load (BinaryReader reader, int header) {
		ClearPath();
		ClearUnits();
		int x = 20, z = 15;
		if (header >= 1) {
			x = reader.ReadInt32();
			z = reader.ReadInt32();
		}
		if (x != cellCountX || z != cellCountZ) {
			if (!CreateMap(x, z)) {
				return;
			}
		}

		for (int i = 0; i < cells.Length; i++) {
			cells[i].Load(reader);
		}
		for (int i = 0; i < chunks.Length; i++) {
			chunks[i].Refresh();
		}
		if (header >= 2) {
			int unitCount = reader.ReadInt32();
			for (int i = 0; i < unitCount; i++) {
				HexUnit.Load(reader, this);
			}
		}
	}

	public void ClearPath () {
		if (currentPathExists) {

			HexCell current = currentPathTo;
			while (current != currentPathFrom) {
				current.SetLabel(null);
				current.DisableHighlight();
				current = current.PathFrom;
			}
			current.DisableHighlight();
			currentPathExists = false;
			ClearUnitHighlight();
		}
		else if (currentPathFrom) {
			currentPathFrom.DisableHighlight();
			currentPathTo.DisableHighlight();
			ClearUnitHighlight();
		}
		currentPathFrom = currentPathTo = null;
	}

	void ShowPath (int speed, bool canMove) {
		toTileTurn = 0;
		int turn = 0;
		if (currentPathExists) {
			HexCell current = currentPathTo;
			while (current != currentPathFrom) {
				turn = current.Distance / speed;

                if (!canMove)
                {
					turn++;
                }

                current.SetLabel(turn.ToString());
				current.EnableHighlight(Color.white);
				current = current.PathFrom;

				if(toTileTurn < turn) { toTileTurn = turn; }
			}
        }
		currentPathFrom.EnableHighlight(Color.blue);
		currentPathTo.EnableHighlight(Color.red);
	}

	HexCell highlightingCell;
	HexCell selectedUnitCell;

	public void HighlightUnit(HexCell selectedUnitCell,HexCell cellToHighlight)
	{
		ClearUnitHighlight();

		selectedUnitCell.EnableHighlight(Color.blue);
        highlightingCell = cellToHighlight;

        if (selectedUnitCell.coordinates.DistanceTo(cellToHighlight.coordinates) <= selectedUnitCell.Unit.range)
		{
            highlightingCell.EnableHighlight(Color.red);
        }
        else if(selectedUnitCell.coordinates.DistanceTo(cellToHighlight.coordinates) > selectedUnitCell.Unit.range)
		{
			highlightingCell.EnableHighlight(Color.black);
		}
	}

	public void ClearUnitHighlight()
	{
		if(highlightingCell != null)
			highlightingCell.DisableHighlight();

		if(selectedUnitCell != null)
			selectedUnitCell.DisableHighlight();
	}

	public void FindPath (HexCell fromCell, HexCell toCell, int speed, bool canMove) {
		ClearPath();
		currentPathFrom = fromCell;
		currentPathTo = toCell;
		currentPathExists = Search(fromCell, toCell, speed);
		ShowPath(speed, canMove);
	}


    bool Search (HexCell fromCell, HexCell toCell, int speed) {
		searchFrontierPhase += 2;
		if (searchFrontier == null) {
			searchFrontier = new HexCellPriorityQueue();
		}
		else {
			searchFrontier.Clear();
		}

		fromCell.SearchPhase = searchFrontierPhase;
		fromCell.Distance = 0;
		searchFrontier.Enqueue(fromCell);
		while (searchFrontier.Count > 0) {
			HexCell current = searchFrontier.Dequeue();
			current.SearchPhase += 1;

			if (current == toCell) {
				return true;
			}

			int currentTurn = current.Distance / speed;

			// Iterates through each neighbor of the cell
			for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
				HexCell neighbor = current.GetNeighbor(d);
				// Determines if you can move that direction, and the potential move cost
				if (
					// Does this tile exist? And has it been searched yet
					neighbor == null ||
					neighbor.SearchPhase > searchFrontierPhase
				) {
					continue;
				}
				if (neighbor.IsUnderwater || neighbor.Unit) {
					// Is this tile underwater?
					continue;
				}
				HexEdgeType edgeType = current.GetEdgeType(neighbor);
				if (edgeType == HexEdgeType.Cliff) {
					// Is this tile too high up
					continue;
				}
				int moveCost;
				if (current.HasRoadThroughEdge(d)) {
					// Roads have a movement cost of 1
					moveCost = 1;
				}
				else if (current.Walled != neighbor.Walled) {
					// Cannot pass through walls
					continue;
				}
				else {
					// If the edge type is flat, the cost is 5. Otherwise it is 10
					moveCost = edgeType == HexEdgeType.Flat ? 5 : 10;
					moveCost += neighbor.UrbanLevel + neighbor.FarmLevel +
						neighbor.PlantLevel;
				}


				int distance = current.Distance + moveCost;
				int turn = distance / speed;
                if (turn > currentTurn) {
					distance = turn * speed + moveCost;
				}
				if (neighbor.SearchPhase < searchFrontierPhase) {
					neighbor.SearchPhase = searchFrontierPhase;
					neighbor.Distance = distance;
					neighbor.PathFrom = current;
					neighbor.SearchHeuristic =
						neighbor.coordinates.DistanceTo(toCell.coordinates);
					searchFrontier.Enqueue(neighbor);
				}
				else if (distance < neighbor.Distance) {
					int oldPriority = neighbor.SearchPriority;
					neighbor.Distance = distance;
					neighbor.PathFrom = current;
					searchFrontier.Change(neighbor, oldPriority);
				}
			}
		}
		return false;
	}

    public List<HexCell> AISearch(HexCell fromCell, HexCell toCell, int speed)
	{

        List<HexCell> possibleMoves = new List<HexCell> { };
        searchFrontierPhase += 2;
        if (searchFrontier == null)
        {
            searchFrontier = new HexCellPriorityQueue();
        }
        else
        {
            searchFrontier.Clear();
        }

        fromCell.SearchPhase = searchFrontierPhase;
        fromCell.Distance = 0;
        searchFrontier.Enqueue(fromCell);
        while (searchFrontier.Count > 0)
        {
            HexCell current = searchFrontier.Dequeue();
            current.SearchPhase += 1;

            /*if (current == toCell)
            {
                return possibleMoves;
            } */

            int currentTurn = current.Distance / speed;

            // Iterates through each neighbor of the cell
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                // Determines if you can move that direction, and the potential move cost
                if (
                    // Does this tile exist? And has it been searched yet
                    neighbor == null ||
                    neighbor.SearchPhase > searchFrontierPhase
                )
                {
                    continue;
                }
                if (neighbor.IsUnderwater || neighbor.hasUnit)
                {
                    // Is this tile underwater? Is it already occupied?
					if(!neighbor.VictoryPoint)
						continue;
                }
                HexEdgeType edgeType = current.GetEdgeType(neighbor);
                if (edgeType == HexEdgeType.Cliff)
                {
                    // Is this tile too high up
                    continue;
                }
                int moveCost;
                if (current.HasRoadThroughEdge(d))
                {
                    // Roads have a movement cost of 1
                    moveCost = 1;
                }
                else if (current.Walled != neighbor.Walled)
                {
                    // Cannot pass through walls
                    continue;
                }
                else
                {
                    // If the edge type is flat, the cost is 5. Otherwise it is 10
                    moveCost = edgeType == HexEdgeType.Flat ? 5 : 10;
                    moveCost += neighbor.UrbanLevel + neighbor.FarmLevel +
                        neighbor.PlantLevel;
                }


                int distance = current.Distance + moveCost;
                int turn = distance / speed;
                // If it is in move range, it is a possible move
                if (turn == 0 && !current.hasUnit)
				{
					possibleMoves.Add(current);
                }

                if (current == toCell)
                {
                    if (turn == 0 && !current.hasUnit)
                    {
                        possibleMoves.Add(current);
                    }
                    return possibleMoves;
                }

                if (turn > currentTurn)
                {
                    distance = turn * speed + moveCost;
                }
                if (neighbor.SearchPhase < searchFrontierPhase)
                {
                    neighbor.SearchPhase = searchFrontierPhase;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    neighbor.SearchHeuristic =
                        neighbor.coordinates.DistanceTo(toCell.coordinates);
                    searchFrontier.Enqueue(neighbor);
                }
                else if (distance < neighbor.Distance)
                {
                    int oldPriority = neighbor.SearchPriority;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    searchFrontier.Change(neighbor, oldPriority);
                }
            }
        }
		Debug.Log("No possible moves");
        return null;
    }
}