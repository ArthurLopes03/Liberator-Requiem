using UnityEngine;

public class HexCell : MonoBehaviour {

	public HexCoordinates coordinates;

	public RectTransform uiRect;

	public HexGridChunk chunk;

    bool hasIncomingRiver, hasOutgoingRiver;
    HexDirection incomingRiver, outgoingRiver;


	//Gets incoming and outgoing river qualities for triangulating cells
    public bool HasIncomingRiver
    {
        get
        {
            return hasIncomingRiver;
        }
    }

    public bool HasOutgoingRiver
    {
        get
        {
            return hasOutgoingRiver;
        }
    }

    public HexDirection IncomingRiver
    {
        get
        {
            return incomingRiver;
        }
    }

    public HexDirection OutgoingRiver
    {
        get
        {
            return outgoingRiver;
        }
    }

	// Returns if the cell has a river or not
    public bool HasRiver
    {
        get
        {
            return hasIncomingRiver || hasOutgoingRiver;
        }
    }

	// Returns whether the cell has an incoming or outgoing river
    public bool HasRiverBeginOrEnd
    {
        get
        {
            return hasIncomingRiver != hasOutgoingRiver;
        }
    }

    // Returns whether a river is flowing through a certain edge, regardless whether it's incoming or outgoing
    public bool HasRiverThroughEdge(HexDirection direction)
    {
        return
            hasIncomingRiver && incomingRiver == direction ||
            hasOutgoingRiver && outgoingRiver == direction;
    }

    public Color Color {
		get {
			return color;
		}
		set {
			if (color == value) {
				return;
			}
			color = value;
			Refresh();
		}
	}

	public int Elevation {
		get {
			return elevation;
		}
		set {
			if (elevation == value) {
				return;
			}
			elevation = value;
			Vector3 position = transform.localPosition;
			position.y = value * HexMetrics.elevationStep;
			position.y +=
				(HexMetrics.SampleNoise(position).y * 2f - 1f) *
				HexMetrics.elevationPerturbStrength;
			transform.localPosition = position;

			Vector3 uiPosition = uiRect.localPosition;
			uiPosition.z = -position.y;
			uiRect.localPosition = uiPosition;

            // Makes sure that there are no uphill rivers being created:
            // If the new elevation is higher than its neighbor, and the two share a river, the river is removed
            if (
                hasOutgoingRiver &&
                elevation < GetNeighbor(outgoingRiver).elevation
            )
            {
                RemoveOutgoingRiver();
            }
            if (
                hasIncomingRiver &&
                elevation > GetNeighbor(incomingRiver).elevation
            )
            {
                RemoveIncomingRiver();
            }

            Refresh();
		}
	}

	public Vector3 Position {
		get {
			return transform.localPosition;
		}
	}

	Color color;

	int elevation = int.MinValue;

	[SerializeField]
	HexCell[] neighbors;

	public HexCell GetNeighbor (HexDirection direction) {
		return neighbors[(int)direction];
	}

	public void SetNeighbor (HexDirection direction, HexCell cell) {
		neighbors[(int)direction] = cell;
		cell.neighbors[(int)direction.Opposite()] = this;
	}

	public HexEdgeType GetEdgeType (HexDirection direction) {
		return HexMetrics.GetEdgeType(
			elevation, neighbors[(int)direction].elevation
		);
	}

	public HexEdgeType GetEdgeType (HexCell otherCell) {
		return HexMetrics.GetEdgeType(
			elevation, otherCell.elevation
		);
	}

    // Returns elevation of stream bed
    public float StreamBedY
    {
        get
        {
            return
                (elevation + HexMetrics.streamBedElevationOffset) *
                HexMetrics.elevationStep;
        }
    }

    // Sets outgoing rivers
    public void SetOutgoingRiver(HexDirection direction)
    {
        // If it already has a river, the method ends
        if (hasOutgoingRiver && outgoingRiver == direction)
        {
            return;
        }

        // Makes sure there is a neighbor in the direction
        // It also makes sure that rivers don't flow uphill, because it is impossible
        HexCell neighbor = GetNeighbor(direction);
        if (!neighbor || elevation < neighbor.elevation)
        {
            return;
        }

        // Removes the outgoing river, and incoming river if it overlaps with the new one
        RemoveOutgoingRiver();
        if (hasIncomingRiver && incomingRiver == direction)
        {
            RemoveIncomingRiver();
        }

        // Adds new outgoing river information, then refreshes the cell
        hasOutgoingRiver = true;
        outgoingRiver = direction;
        RefreshSelfOnly();

        // Sets incoming river information of neighboring cell
        neighbor.RemoveIncomingRiver();
        neighbor.hasIncomingRiver = true;
        neighbor.incomingRiver = direction.Opposite();
        neighbor.RefreshSelfOnly();
    }

    // Removes outgoing rivers
    public void RemoveOutgoingRiver()
    {
		// If does not have a river, the method ends
        if (!hasOutgoingRiver)
        {
            return;
        }

		// If it does, it removes the property and refreshes the cell
        hasOutgoingRiver = false;
        RefreshSelfOnly();

        // If a cell has an incoming river, than its neighbor must have an outgoing river
        // We remove that one as well
        HexCell neighbor = GetNeighbor(outgoingRiver);
        neighbor.hasIncomingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    // Removes incoming rivers
    // The logic works the same, but adapted to incoming rivers
    public void RemoveIncomingRiver()
    {
        if (!hasIncomingRiver)
        {
            return;
        }
        hasIncomingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(incomingRiver);
        neighbor.hasOutgoingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    // Removes the whole thing
    public void RemoveRiver()
    {
        RemoveOutgoingRiver();
        RemoveIncomingRiver();
    }

    void Refresh () {
		if (chunk) {
			chunk.Refresh();
			for (int i = 0; i < neighbors.Length; i++) {
				HexCell neighbor = neighbors[i];
				if (neighbor != null && neighbor.chunk != chunk) {
					neighbor.chunk.Refresh();
				}
			}
		}
	}

	// Refreshes a single cell only
    void RefreshSelfOnly()
    {
        chunk.Refresh();
    }


}