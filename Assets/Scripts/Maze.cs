using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

/// <summary>
/// Responsible for creating the arena based on a LevelScriptable.
/// </summary>
public class Maze : MonoBehaviour
{
    [SerializeField]
    private Tilemap groundMap;

    [SerializeField]
    private LevelScriptable level;

    [SerializeField]
    private Grid grid;

    public Grid Grid {get { return grid; } }

    [SerializeField][Header ("Visual feedback")][Rename("Show Gizmo")]
    private bool drawTileStateOnGizmo;

    [SerializeField]
    private Tile guide;

    [SerializeField]
    public Tilemap guideMap;

    public LevelScriptable Level {get { return level; } set { level = value; } }

    private List<SpriteRenderer> wallSprites = new List<SpriteRenderer>();


    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/
    // Methods
    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/

    /// <summary>
    /// Create a shallow copy from the parameter and load it's state into the grid
    /// </summary>
    /// <param name="level"></param>
    public void LoadLevel (LevelScriptable level)
    {
        Level = level;
        RegenerateTiles ();
    }

    /// <summary>
    /// Get the position of all tiles in the grid that matches a specific TileState.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public List<Vector3> GetTilePosition (TileState state)
    {
        List<Vector3> positions = new List<Vector3> ();
        for (int i = 0; i < Level.mapSize.x; i++)
        {
            for (int j = 0; j < Level.mapSize.y; j++)
            {
                if (Level.tiles[i].array[j].HasFlag (state)) {
                    positions.Add (grid.CellToWorld (new Vector3Int (i, j)));
                }
            }   
        }
        return positions;
    }

    /// <summary>
    /// Use a world position to find a tile on the grid and get its respective TileState.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public TileState GetTileState (Vector3 position)
    {
        var gridPosition = grid.WorldToCell (position);
        TileState tileState = Level.tiles [gridPosition.x].array [gridPosition.y];

        if (gridPosition.x >= Level.mapSize.x) {
            return (TileState.xWall | TileState.yWall);
        }
        if (gridPosition.x <= -1) {
            return (TileState.xWall | TileState.yWall);
        }
        if (gridPosition.y >= Level.mapSize.y) {
            return (TileState.xWall | TileState.yWall);
        }
        if (gridPosition.y <= 0) {
            return (TileState.xWall | TileState.yWall);
        }

        return Level.tiles [gridPosition.x].array [gridPosition.y];
    }

    /// <summary>
    /// Return what TileState is inside the Ray.
    /// </summary>
    /// <param name="ray"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public TileState GetWallBetweenTiles (Ray ray, float distance)
    {
        var finalPosition = grid.WorldToCell (ray.origin + (ray.direction * distance));
        var initialPosition = grid.WorldToCell (ray.origin);

        // Verify for out of bounds location
        if (finalPosition.x >= Level.mapSize.x) {
            return TileState.yWall;
        }
        if (finalPosition.x < 0) {
            return TileState.yWall;
        }
        if (finalPosition.y >= Level.mapSize.y) {
            return TileState.xWall;
        }
        if (finalPosition.y < 0) {
            return TileState.xWall;
        }

        // Wall is searched on the same tile when the movement is to the right
        if (finalPosition.x > initialPosition.x)  
        {
            if (Level.tiles [initialPosition.x].array [initialPosition.y].HasFlag (TileState.yWall))
                return TileState.yWall;
        }
        // Wall is searched on the tile to the left when the movement is to the left
        if (finalPosition.x < initialPosition.x)
        {
            if (Level.tiles [initialPosition.x - 1].array [initialPosition.y].HasFlag (TileState.yWall))
                return TileState.yWall;
        }
        // Wall is searched on the tile above when the movement is to the top
        if (finalPosition.y > initialPosition.y) 
        {
            if (Level.tiles [initialPosition.x].array [initialPosition.y + 1].HasFlag (TileState.xWall))
                return TileState.xWall;
        }
        // Wall is searched on the same tile when the movement is to the bottom
        if (finalPosition.y < initialPosition.y)
        {
            if (Level.tiles [initialPosition.x].array [initialPosition.y].HasFlag (TileState.xWall))
                return TileState.xWall;
        }

        return TileState.None;
    }

    /// <summary>
    /// Spawn the wall prefab.
    /// </summary>
    /// <param name="isHorizontal"></param>
    /// <param name="position"></param>
    private void CreateWall (bool isHorizontal, Vector2Int position)
    {
        Vector3 pos = grid.CellToWorld (new Vector3Int (position.x, position.y));
        SpriteRenderer prefab = isHorizontal ? Level.xWall : Level.yWall;
        SpriteRenderer wall = Instantiate (prefab, pos, Quaternion.identity, transform);
        wallSprites.Add (wall);
    }

    /// <summary>
    /// Change the ground tiles sprite, update the size and create a set of walls around the arena, for each direction
    /// </summary>
    public void RegenerateTiles () 
    {
        // GetComponents is more expensive than GetChild, but it works all the time on the editor
        Transform [] walls = GetComponentsInChildren<Transform> ();
        foreach (var item in walls)
        {
            if (item != transform)
                DestroyImmediate (item.gameObject);
        }

        wallSprites.Clear ();
        
        if (Level == null)
            return;

        var mapSize = Level.mapSize;
        var groundTile = Level.groundTile;
        var verticalWall = Level.xWall;
        var horizontalWall = Level.yWall;

        // Create the ground tiles and the lines between them
        for (int i = 0; i < mapSize.x; i++)
        {
            for (int j = 0; j < mapSize.y; j++)
            {
                Vector3Int pos = new Vector3Int (i, j);
                groundMap.SetTile (pos, groundTile);
                guideMap.SetTile (pos, guide);
            }
        }

        // Due to the sprite orthogonal nature, there is only need to create the left and top walls
        // The right and bottom walls are created on the Level, if needed

        // Border horizontal walls
        for (int i = 0; i < mapSize.y; i++)
        {
            CreateWall (true, new Vector2Int (i, mapSize.y));
            //CreateWall (true, new Vector2Int (i, -(mapSize.y - 1)), ((mapSize.x - 1) * (mapSize.y - 1)) + (i + 1) + (i * 1));
        }

        // Border vertical walls
        for (int i = 0; i < mapSize.x; i++)
        {
            CreateWall (false, new Vector2Int (0, i+1));
            //CreateWall (false, new Vector2Int (mapSize.x, -i), ((mapSize.x - 1) * (mapSize.y - 1)) + (i + 2) + (i * 1));
        }

        // All the other walls inside the borders
        for (int i = 0; i < Level.mapSize.x; i++)
        {
            for (int j = 0; j < Level.mapSize.y; j++)
            {
                if (Level.tiles [i].array [j].HasFlag (TileState.xWall))
                {
                    CreateWall (true, new Vector2Int (i, j));
                }
                if (Level.tiles [i].array [j].HasFlag (TileState.yWall))
                {
                    CreateWall (false, new Vector2Int (i + 1, j + 1));
                }
            }
        }
    }

    // Deactivated for creating a build of the game
    // private void OnDrawGizmosSelected ()
    // {
    //     if (drawTileStateOnGizmo == false)
    //         return;
        
    //     if (Level == null)
    //         return;
        
    //     for (int i = 0; i < Level.mapSize.x; i++)
    //     {
    //         for (int j = 0; j < Level.mapSize.y; j++)
    //         {
    //             var state = Level.tiles [i].array [j];
    //             GUIStyle style  = new GUIStyle (GUI.skin.label);
    //             style.fontSize = 10;
    //             Vector3 position = grid.CellToWorld (new Vector3Int (i, j));
    //             position += new Vector3 (0.08f, 0.16f);
    //             Handles.Label (position, i.ToString()+","+j.ToString()+"\n "+state.ToString(), style);   
    //         }
    //     }
    // }
}
