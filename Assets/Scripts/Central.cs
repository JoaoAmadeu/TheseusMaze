using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System;

/// <summary>
/// Class that connects all the other classes to start the experience.
/// </summary>
public class Central : MonoBehaviour
{
    [SerializeField][Tooltip ("Each element represents a player inside the game. The turn sequence is done through this array.")]
    private List<Player> players;

    private int playerIndex;

    public int PlayerIndex 
    {
        get { return playerIndex; }
        private set 
        {
            playerIndex = value;

            if (playerIndex >= players.Count)
                playerIndex = 0;
        }
    }
    
    [SerializeField][Space]
    private Maze maze;

    [SerializeField]
    private GameObject holePrefab;

    [SerializeField]
    private Pawn theseusPrefab;

    [SerializeField]
    private Pawn minotaurPrefab;

    private List<GameObject> spawnedGameObjects = new();

    [SerializeField]
    private LevelScriptable[] levels;

    public LevelScriptable[] Levels {get { return levels; } }

    private int levelIndex;

    public int LevelIndex {get { return levelIndex; } }

    public LevelScriptable CurrentLevel {get { return currentLevel; } }

    private LevelScriptable currentLevel;

    public event Action<Controller> onControllerCreate;

    public event Action<Controller> onGameEnd;

    public event Action<LevelScriptable> onLevelLoaded;

    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/
    // Methods
    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/

    private void Awake ()
    {
        // No collider  or physics is used on the experience, instead we simply use the pawns and walls positions to
        // recreate the collisions and raycasts with low cost on performance.
        World.collisionEvent += maze.GetTileState;
        World.raycastEvent += maze.GetWallBetweenTiles;
    }

    private void Start ()
    {
        // Create Controllers based on a Player List and fill their individual properties for the game.
        for (int i = 0; i < players.Count; i++)
        {
            var controllerType = players [i].isAi ? typeof (AiController) : typeof (PlayerController);
            GameObject playerGo = new GameObject (controllerType + " " + (i + 1).ToString ());
            
            var thisController = playerGo.AddComponent (controllerType) as Controller;
            thisController.onActionBegin.AddListener (ToggleTileState);
            thisController.onActionDone.AddListener (ToggleTileState);

            players [i].controller = thisController;
            players [i].actionsCounter = 0;
            players [i].controllerIndex = i;

            onControllerCreate?.Invoke (thisController);
        }

        LoadLevel (0);
    }

    public void LoadLevel (int index)
    {
        if (index >= Levels.Length || index < 0) {
            return;
        }

        levelIndex = index;

        var shallowLevel = ScriptableObject.CreateInstance <LevelScriptable> ();
        Levels [levelIndex].ShallowCopy (shallowLevel);
        currentLevel = shallowLevel;
        maze.LoadLevel (shallowLevel);

        // Destroy past GameObjects and start new at each level loaded.
        spawnedGameObjects.ForEach ((g) => Destroy (g));
        spawnedGameObjects.Clear ();

        // For each specific TileState in the maze, we fill with its corresponding Prefab.
        // If it's a Pawn, we assign a Controller to it.
        foreach (var position in maze.GetTilePosition (TileState.Player01))
        {
            var newPosition = (maze.Grid.cellSize * 0.5f) + position;
            Pawn theseus = Instantiate (theseusPrefab, newPosition, Quaternion.identity);
            var players01 = players.FindAll ((player) => player.type == TileState.Player01);
            players01.ForEach ((player01) => player01.controller.Init (maze.Grid, theseus, TileState.Player01));

            theseus.gameObject.name = "Theseus";
            spawnedGameObjects.Add (theseus.gameObject);
        }

        foreach (var position in maze.GetTilePosition (TileState.Player02))
        {
            var newPosition = (maze.Grid.cellSize * 0.5f) + position;
            Pawn minotaur = Instantiate (minotaurPrefab, newPosition, Quaternion.identity);
            var players02 = players.FindAll ((player) => player.type == TileState.Player02);
            players02.ForEach ((player02) => player02.controller.Init (maze.Grid, minotaur, TileState.Player02));

            minotaur.gameObject.name = "Minotaur";
            spawnedGameObjects.Add (minotaur.gameObject);
        }

        foreach (var position in maze.GetTilePosition (TileState.Teleport))
        {
            var newPosition = (maze.Grid.cellSize * 0.5f) + position;
            spawnedGameObjects.Add (Instantiate (holePrefab, newPosition, Quaternion.identity));
        }

        // Set the target for all players controlled by the machine.
        foreach (var item in players)
        {
            item.controller.enabled = false;
            if (item.isAi == true)
            {
                var ai = item.controller as AiController;
                ai.SetTarget (players [playerIndex].controller.Pawn.transform);
            }
        }

        onLevelLoaded?.Invoke (CurrentLevel);
    }

    public void RestartLevel ()
    {
        LoadLevel (LevelIndex);
    }

    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/
    // Controller movement
    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/

    /// <summary>
    /// This method is called before and after the Controller moves. This will uncheck the Controller 
    /// TileState from the previous Tile and will check the same TileState in the new Tile. Also
    /// it will check for the winning condition of all the controllers.
    /// </summary>
    /// <param name="controller"></param>
    private void ToggleTileState (Controller controller)
    {
        foreach (var player in players)
        {
            var callback = GetWinCondition (player.controller);
            callback.Invoke (player.controller);
        }

        var gridPos = controller.GridPosition;
        CurrentLevel.tiles [gridPos.x].array [gridPos.y] ^= controller.Type;
    }

    private void GameWin (Controller controller)
    {
        onGameEnd?.Invoke (controller);
        foreach (var item in players)
        {
            item.controller.enabled = false;
        }
    }

    private UnityAction<Controller> GetWinCondition (Controller controller)
    {
        return controller is AiController ? CollisionBetweenPlayers : TeleportEntered;
    }

    private void CollisionBetweenPlayers (Controller controller)
    {
        if (controller == null || controller.Pawn == null) {
            return;
        }
        
        var result = World.GetCollider (controller.Pawn.transform.position);

        if (controller.Type.HasFlag (TileState.Player01)) {
            if (result.HasFlag (TileState.Player02)) {
                GameWin (controller);
            }
        }

        if (controller.Type.HasFlag (TileState.Player02)) {
            if (result.HasFlag (TileState.Player01)) {
                GameWin (controller);
            }
        }
    }

    private void TeleportEntered (Controller controller)
    {
        var result = World.GetCollider (controller.Pawn.transform.position);

        if (result.HasFlag (TileState.Teleport)) {
            GameWin (controller);
        }
    }
}