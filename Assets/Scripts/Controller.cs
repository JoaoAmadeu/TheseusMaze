using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controller to move pawns in a scenario with a Grid component
/// </summary>
public class Controller : MonoBehaviour
{
    [SerializeField][Tooltip("The Pawn instance we are responsible for controlling.")]
    protected Pawn pawn;

    /// <summary>
    /// A reference to the grid where the pawn resides. Necessary to place the the pawn on the exact spot.
    /// </summary>
    protected Grid grid;

    /// <summary>
    /// Delay between movements.
    /// </summary>
    [SerializeField]
    protected float timeToMove = 0.5f;

    [SerializeField]
    [ReadOnly]
    [Tooltip("The type of TileState the Pawn will represent when on top of a tile.")]
    protected TileState type;

    public UnityEvent<Controller> onActionBegin = new UnityEvent<Controller>();

    public UnityEvent<Controller> onActionDone = new UnityEvent<Controller>();

    public Vector3Int GridPosition {get { return grid.WorldToCell (pawn.transform.position); } }

    /// <summary>
    /// The Pawn instance we are responsible for controlling.
    /// </summary>
    public Pawn Pawn {get { return pawn; }}

    /// <summary>
    /// The type of TileState the Pawn will represent when on top of a tile.
    /// </summary>
    public TileState Type { get { return type; } }

    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/
    // Methods
    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/

    // This method is here just to create a enable/disable checkmark on the editor.
    private void OnEnable () {}

    public void Init (Grid grid, Pawn pawn, TileState type)
    {
        this.grid = grid;
        this.pawn = pawn;
        this.type = type;
    }

    /// <summary>
    /// Move the current pawn to the right.
    /// </summary>
    public bool MoveRight ()
    {
        return MoveInGrid (1, 0);
    }

    /// <summary>
    /// Move the current pawn to the left.
    /// </summary>
    public bool MoveLeft ()
    {
        return MoveInGrid (-1, 0);
    }

    /// <summary>
    /// Move the current pawn upwards.
    /// </summary>
    public bool MoveUp ()
    {
        return MoveInGrid (0, 1);
    }

    /// <summary>
    /// Move the current pawn downwards.
    /// </summary>
    public bool MoveDown ()
    {
        return MoveInGrid (0, -1);
    }

    /// <summary>
    /// Does nothing.
    /// </summary>
    public void DoNothing ()
    {
        onActionBegin?.Invoke (this);
        StartCoroutine (CallOnMovementDone (0.1f));
    }

    /// <summary>
    /// Move the pawn by a certain number of cells inside the grid.
    /// </summary>
    /// <param name="xCells"> Cells quantity on the horizontal. </param>
    /// <param name="yCells"> Cells quantity on the vertical. </param>
    private bool MoveInGrid (int xCells, int yCells)
    {
        if (pawn == null) {
            return false;
        }

        // Find the exact position on the grid and attempt to move there.
        Vector3Int newPosition = GridPosition + new Vector3Int (xCells, yCells);
        Vector3 worldPosition = grid.CellToWorld (newPosition);
        worldPosition += grid.cellSize * 0.5f; // center on the cell
        var success = pawn.Walk (worldPosition, timeToMove);

        if (success) 
        {
            onActionBegin?.Invoke (this);
            StartCoroutine (CallOnMovementDone (timeToMove));
        }
        return success;
    }

    protected IEnumerator CallOnMovementDone (float delay)
    {
        yield return new WaitForSeconds (delay + 0.1f);

        onActionDone?.Invoke (this);
    }
}