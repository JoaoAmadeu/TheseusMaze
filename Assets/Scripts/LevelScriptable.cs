using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Hold all the information for spawning a new level. Have its own Editor.
/// </summary>
[CreateAssetMenu (fileName = "NewLevel", menuName = "Level")]
public class LevelScriptable : ScriptableObject
{
    public Vector2Int mapSize = new Vector2Int (16, 16);

    public Tile groundTile;

    [Rename ("Horizontal Wall")]
    public SpriteRenderer xWall;

    [Rename ("Vertical Wall")]
    public SpriteRenderer yWall;

    [Multiline (5)]
    public string description;

    [HideInInspector]
    public TileStateArray[] tiles;

    public void ShallowCopy (LevelScriptable other)
    {
        other.mapSize = mapSize;
        other.groundTile = groundTile;
        other.xWall = xWall;
        other.yWall = yWall;
        other.description = description;

        // Neither Array.CopyTo or Array.Clone worked
        other.tiles = new TileStateArray [other.mapSize.x];
        for (int i = 0; i < other.mapSize.x; i++)
        {
            other.tiles[i] = new TileStateArray ();
            other.tiles[i].array = new TileState [other.mapSize.y];
        }

        for (int i = 0; i < other.mapSize.x; i++)
        {
            for (int j = 0; j < other.mapSize.y; j++)
            {
                other.tiles [i].array [j] = tiles [i].array [j];
            }
        }

    }
}