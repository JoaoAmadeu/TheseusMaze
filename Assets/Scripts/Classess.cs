using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags][System.Serializable]
public enum TileState
{
    None = 0,
    xWall = 1,
    yWall = 2,
    Player01 = 4,
    Player02 = 8,
    Teleport = 16,
}

/// <summary>
/// Class that encapsulate an array of TileState. This class exists due to the fact the editor
/// don't serialize two dimensional arrays.
/// </summary>
[System.Serializable]
public class TileStateArray
{
    //[SerializeField]
    public TileState[] array;

    public TileStateArray ()
    {
        array = new TileState[0];
    }
}

[System.Serializable]
public class Player
{
    [Min(1)]
    public int actionsAllowedPerTurn = 1;

    public bool isAi;

    // TODO: Make this enum GUI control behave like a non bitwise mask
    public TileState type;

    [ReadOnly]
    public Controller controller;

    [ReadOnly]
    public int controllerIndex;

    [ReadOnly]
    public int actionsCounter;
}

[System.Serializable]
public class Movement
{
    public int counter;

    public int limit;

    public Movement (int counter, int limit)
    {
        this.counter = counter;
        this.limit = limit;
    }
}