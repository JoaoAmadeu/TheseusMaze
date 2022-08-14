using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.Sprites;

public class ArenaWindow : EditorWindow
{
    // The reason SerializeField is used, is because the window can't save it's data after the editor is reloaded
    [SerializeField]
    private LevelScriptable level;

    private SerializedObject target;

    private SerializedProperty mapSize;

    private Tile groundTilePrefab;

    private SpriteRenderer xWallPrefab;

    private SpriteRenderer yWallPrefab;

    private double lastRegenerationTime;

    public Maze arena;

    private TileState cursorState;

    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/
    // Initialization
    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/

    private void OnFocus ()
    {
        wantsMouseMove = true;
        if (level != null) {
            UpdateWindow (level);
        }
    }

    private void OnLostFocus ()
    {
        wantsMouseMove = false;
    }

    /// <summary>
    /// Open a new window or try to get one already open.
    /// </summary>
    /// <param name="level">Central object of this window.</param>
    public static void StartEditor (LevelScriptable level)
    {
        ArenaWindow window = (ArenaWindow) EditorWindow.GetWindow (typeof (ArenaWindow));
        window.UpdateWindow (level);
        window.Show ();
    }

    /// <summary>
    /// This method ensure the LevelScriptable will have the right properties to begin editing
    /// </summary>
    /// <param name="level">The asset wich will be used during the editing</param>
    private void UpdateWindow (LevelScriptable level)
    {
        this.level = level;

        if (level == null)
            return;

        target = new SerializedObject (level);
        mapSize = target.FindProperty ("mapSize");

        // If any asset from the Level is not found, we fill with a default value
        var groundProperty = target.FindProperty ("groundTile");
        var xProperty = target.FindProperty ("xWall");
        var yProperty = target.FindProperty ("yWall");
        
        if (groundProperty.objectReferenceValue == null) {
            groundProperty.objectReferenceValue = 
                (Tile) AssetDatabase.LoadAssetAtPath ("Assets/Tiles/Ti_Ground01", typeof (Tile));
        }
        if (xProperty.objectReferenceValue == null) {
            xProperty.objectReferenceValue = 
                (SpriteRenderer) AssetDatabase.LoadAssetAtPath ("Assets/Tiles/X Wall.prefab", typeof (SpriteRenderer));
        }
        if (yProperty.objectReferenceValue == null) {
            yProperty.objectReferenceValue = 
                (SpriteRenderer) AssetDatabase.LoadAssetAtPath ("Assets/Tiles/Y Wall.prefab", typeof (SpriteRenderer));
        }

        groundTilePrefab = groundProperty.objectReferenceValue as Tile;
        xWallPrefab = xProperty.objectReferenceValue as SpriteRenderer;
        yWallPrefab = yProperty.objectReferenceValue as SpriteRenderer;

        // Initialize the tile arrays, if null
        if (level.tiles == null || level.tiles.Length == 0)
        {
            level.tiles = new TileStateArray [mapSize.vector2IntValue.x];
            for (int i = 0; i < mapSize.vector2IntValue.x; i++)
            {
                level.tiles[i] = new TileStateArray ();
                level.tiles[i].array = new TileState [mapSize.vector2IntValue.y];
            }
        }

        RegenerateSceneTiles ();
        target.ApplyModifiedProperties ();
    }

    /// <summary>
    /// Safe call to the Maze instance 'arena' RegenerateTiles method
    /// </summary>
    private void RegenerateSceneTiles ()
    {
        if (arena == null) 
        {
            arena = SceneAsset.FindObjectOfType<Maze> ();

            if (arena == null) {
                Debug.Log ("No Arena object found in current scene.");
                return;
            }
        }
        else {
            arena.RegenerateTiles ();
        }
    }

    /// <summary>
    /// Put all the tiles in the default state
    /// </summary>
    private void ClearTiles ()
    {
        for (int i = 0; i < mapSize.vector2IntValue.x; i++)
        {
            for (int j = 0; j < mapSize.vector2IntValue.y; j++)
            {
                level.tiles [i].array [j] = TileState.None;
            }
        }
        RegenerateSceneTiles ();
    }

    /// <summary>
    /// Will call a delegate to every tile in the Maze
    /// </summary>
    /// <param name="callback"></param>
    private void ApplyToAllTiles (Func<TileState, TileState> callback)
    {
        for (int i = 0; i < mapSize.vector2IntValue.x; i++)
        {
            for (int j = 0; j < mapSize.vector2IntValue.y; j++)
            {
                level.tiles [i].array [j] = callback.Invoke (level.tiles [i].array [j]);
            }
        }
    }

    private void OnEnable ()
    {
        RegenerateSceneTiles ();
    }

    private void OnDisable ()
    {
        RegenerateSceneTiles ();
    }

    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/
    // Draw Methods
    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/

    // TODO: Create scrollView, to make the tiles visible when docked
    // TODO: This whole window can be done to work in several sizes of grid cells, currently it's
    // cell size is 0.32 units

    private void OnGUI ()
    {
        // If the tile regeneration isn't called enough, just enable the code below to make it happen in an interval of time
        // if (EditorApplication.timeSinceStartup > lastRegenerationTime) {
        //     lastRegenerationTime = EditorApplication.timeSinceStartup + 0.25f;
        //     RegenerateSceneTiles ();
        // }

        // Left and top margin in pixels
        var start = new Vector2 (16, 16);

        // BeginArea is used because EditorGUI.ObjectField can't be used without a SerializedProperty,
        // and level here is used directly as an asset, not a property
        Rect layoutRect = new Rect (start.x, start.y, 350, EditorGUIUtility.singleLineHeight);
        EditorGUI.BeginChangeCheck ();
        GUILayout.BeginArea (layoutRect);
        level = EditorGUILayout.ObjectField (level, typeof(LevelScriptable), false) as LevelScriptable;
        EditorGUILayout.Separator ();
        GUILayout.EndArea ();
        if (EditorGUI.EndChangeCheck ()) {
            UpdateWindow (level);
        }

        if (level == null) {
            // BeginArea is needed again because of the previous GUI element
            layoutRect.y += EditorGUIUtility.singleLineHeight * 1.5f;
            GUILayout.BeginArea (new Rect(layoutRect.x, layoutRect.y, 350, EditorGUIUtility.singleLineHeight * 3));
            EditorGUILayout.HelpBox ("Select a level. Pick from the assets folder or create one through the assets menu.",
                                    MessageType.Warning);
            GUILayout.EndArea ();
            return;
        }

        var mapSize = target.FindProperty ("mapSize");
        GUIStyle labelStyle = new GUIStyle (GUI.skin.label);
        labelStyle.alignment = TextAnchor.MiddleCenter;
        start.y += 16;

        // Text to show the row index
        for (int i = 0; i < mapSize.vector2IntValue.x; i++)
        {
            Rect rect = new Rect (start.x + 32 + (i * 4) + (i * 32), start.y, 32, 32);
            GUI.Label (rect, i.ToString(), labelStyle);
        }

        // Text to show the column index
        for (int i = 0; i < mapSize.vector2IntValue.y; i++)
        {
            Rect rect = new Rect (start.x, start.y + 32 + (i * 4) + (i * 32), 32, 32);
            GUI.Label (rect, i.ToString(), labelStyle);
        }

        start += new Vector2 (32, 32);

        // Ground is drawn first
        for (int i = 0; i < mapSize.vector2IntValue.x; i++)
        {
            for (int j = 0; j < mapSize.vector2IntValue.y; j++)
            {
                Rect rect = new Rect (  start.x + (i * 4) + (i * 32),
                                        start.y + (j * 4) + (j * 32),
                                        32, 32);
                
                // Create underlying buttons that will change the tile state. It overflows the texture size,
                // making just enough space to see underneath the texture
                if (GUI.Button (new Rect (rect.x - 2, rect.y - 2, rect.width + 4, rect.height + 4), string.Empty)) 
                {
                    HandleMouseButton (ref level.tiles[i].array[(mapSize.vector2IntValue.y - 1) - j]);
                }

                // Draw the texture on top of the button
                DrawGround (rect);
            }
        }

        start -= new Vector2 (32, 32);

        // The sorting of the walls on top of the ground, is done through the arrengement
        // of both dictionaries. Sorting GUI textures is not simple.
        Dictionary<Vector2Int, Rect> xWalls = new Dictionary<Vector2Int, Rect> ();
        Dictionary<Vector2Int, Rect> yWalls = new Dictionary<Vector2Int, Rect> ();

        // Walls are drawn on top of the ground
        for (int i = 0; i < mapSize.vector2IntValue.x; i++)
        {
            for (int j = 0; j < mapSize.vector2IntValue.y; j++)
            {
                if (level.tiles[i].array[j] != TileState.None)
                {
                    Rect rect = new Rect (  start.x  + (i * 4) + (i * 32),
                                            (start.y  + (((mapSize.vector2IntValue.y - 1) - j) * 36)),
                                            64, 64);

                    if (level.tiles[i].array[j].HasFlag (TileState.Teleport)) {
                        DrawHole (new Rect (rect.x+32, rect.y+32, 32, 32));
                    }
                    if (level.tiles[i].array[j].HasFlag (TileState.Player01)) {
                        DrawPlayer (rect);
                    }
                    if (level.tiles[i].array[j].HasFlag (TileState.Player02)) {
                        DrawEnemy (rect);
                    }
                    if (level.tiles[i].array [j].HasFlag (TileState.xWall)) {
                        xWalls.Add (new Vector2Int (i, j), rect);
                    }
                    if (level.tiles[i].array [j].HasFlag (TileState.yWall)) {
                        yWalls.Add (new Vector2Int (i, j), rect);
                    }
                }
            }
        }

        // Since y is inverted in the grid positioning, so does the order of drawing wall must be too
        // GUI.depth didn't work, which could made this solution easier
        for (int i = 0; i < mapSize.vector2IntValue.x; i++)
        {
            for (int j = mapSize.vector2IntValue.y; j >= 0; j--)
            {
                Rect rect;

                if (xWalls.TryGetValue (new Vector2Int (i, j), out rect)) {
                    DrawHorizontalWall (rect);
                }

                Rect yrect;
                if (yWalls.TryGetValue (new Vector2Int (i, j), out yrect)) {
                    int index = yWalls.Keys.ToList ().IndexOf (new Vector2Int (i, j));
                    DrawVerticalWall (yrect);
                }
            }
        }

        labelStyle.fontSize = 10;
        start += new Vector2 (32, 32);

        // A text cointaining the tile position, is drawn on top of everything
        for (int i = 0; i < mapSize.vector2IntValue.x; i++)
        {
            for (int j = 0; j < mapSize.vector2IntValue.y; j++)
            {
                Rect rect = new Rect (  start.x + (i * 4) + (i * 32),
                                        start.y + (j * 4) + (j * 32),
                                        32, 32);
                
                // If the current button contains the mouse position, we show the tile row and column.
                // If you choose to place an object, the object's sprite will follow the mouse.
                if (rect.Contains (Event.current.mousePosition)) 
                {
                    if (cursorState == TileState.Teleport) {
                        DrawHole (new Rect (rect.x, rect.y, 32, 32));
                    }
                    if (cursorState == TileState.Player01) {
                        DrawPlayer (new Rect (rect.x-32, rect.y-32, 64, 64));
                    }
                    if (cursorState == TileState.Player02) {
                        DrawEnemy (new Rect (rect.x-32, rect.y-32, 64, 64));
                    }

                    GUI.Label (new Rect (rect.x - 4, rect.y, 40, rect.height), 
                                i.ToString()+","+((mapSize.vector2IntValue.y-1)-j).ToString(), labelStyle);
                    Repaint();
                }
            }
        }

        // This snippet is to show what is being used in the construction of the Level, and how to draw it
        Rect endRect = new Rect ((start.x + 32) + (mapSize.vector2IntValue.x * 36), start.y, 600, 600);
        GUILayout.BeginArea (new Rect (endRect.x, endRect.y, 300, 500));
        EditorGUILayout.PropertyField (mapSize);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField (target.FindProperty ("groundTile"));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField (target.FindProperty ("xWall"));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField (target.FindProperty ("yWall"));
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox ("Left mouse button create a wall on the x axis.\nRight mouse button create a wall on the y axis\n" + 
                                "Middle mouse button delete wall in both axis.",
                                MessageType.Info);
        EditorGUILayout.Space();
        if (GUILayout.Button ("Clear tiles")) {
            ClearTiles ();
        }

        GUILayout.BeginHorizontal ();

        // Player 01 (Theseus) select button
        if (cursorState == TileState.Player01) {
            GUI.color = new Color (0.75f, 0.75f, 0.75f);
        }
        if (DrawButton ("Assets/Tiles/Ti_Theseus.asset", new Vector2 (36, 36), new Vector2 (0.82f, 0.89f))) {
            ApplyToAllTiles ((state) => { return state &= ~TileState.Player01; });
            cursorState = TileState.Player01;
        }
        GUI.color = Color.white;

        // Player 02 (minotaur) select button
        if (cursorState == TileState.Player02) {
            GUI.color = new Color (0.75f, 0.75f, 0.75f);
        }
        if (DrawButton ("Assets/Tiles/Ti_Minotaur.asset", new Vector2 (32, 32), new Vector2 (0.76f, 0.0f))) {
            ApplyToAllTiles ((state) => { return state &= ~TileState.Player02; });
            cursorState = TileState.Player02;
        }
        GUI.color = Color.white;

        // Hole select button
        if (cursorState == TileState.Teleport) {
            GUI.color = new Color (0.75f, 0.75f, 0.75f);
        }
        if (DrawButton ("Assets/Tiles/Ti_Hole.asset", new Vector2 (32, 32), new Vector2 (0.00f, 1.00f))) {
            ApplyToAllTiles ((state) => { return state &= ~TileState.Teleport; });
            cursorState = TileState.Teleport;
        }
        GUI.color = Color.white;

        GUILayout.EndHorizontal ();
        GUILayout.EndArea ();

        target.ApplyModifiedProperties ();
    }

    /// <summary>
    /// Will change the tile state based on the last object the mouse clicked.
    /// </summary>
    /// <param name="state"></param>
    private void HandleMouseButton (ref TileState state)
    {
        if (Event.current.button == 0)
        {
            if (cursorState == TileState.Player01) {
                state ^= TileState.Player01;
            }
            else if (cursorState == TileState.Player02) {
                state ^= TileState.Player02;
            }
            else if (cursorState == TileState.Teleport) {
                state ^= TileState.Teleport;
            }
            else {
                state ^= TileState.xWall;
            }
        }
        else if (Event.current.button == 1) 
        {
            state ^= TileState.yWall;
        }
        else if (Event.current.button == 2)
        {
            state &= ~(TileState.xWall | TileState.yWall);
        }
        
        cursorState = TileState.None;
        RegenerateSceneTiles ();
    }

    // The drawing size, position, pivot and anchoring are numbers which is available on the
    // atlas the sprite came from. Further advancement on this code should provide an automatic
    // way for all sprites, without typing raw numbers here

    private bool DrawButton (string assetPath, Vector2 spriteSize, Vector2 uvOffset)
    {
        Tile tile = (Tile) AssetDatabase.LoadAssetAtPath (assetPath, typeof (Tile));
        Texture2D texture = SpriteUtility.GetSpriteTexture (tile.sprite, false);

        Vector2 textSize = new Vector2 (spriteSize.x / texture.width, spriteSize.y / texture.height);
        Vector2 coordinate = new Vector2 (tile.sprite.uv[0].x - (textSize.x * uvOffset.x), tile.sprite.uv[0].y - (textSize.y * uvOffset.y));
        var textureRect = new Rect (coordinate, textSize);

        GUILayout.Box ("", GUILayout.Width (50), GUILayout.Height (50));
        Rect lastRect = GUILayoutUtility.GetLastRect ();
        bool value = GUI.Button (new Rect (lastRect.x, lastRect.y, lastRect.width, lastRect.height), "");
        GUI.DrawTextureWithTexCoords (new Rect(lastRect.x+5,lastRect.y+5,lastRect.width-10,lastRect.height-10), texture, textureRect);

        return value;
    }

    private void DrawGround (Rect rect)
    {
        Sprite groundSprite = groundTilePrefab.sprite;        
        var groundTexture = SpriteUtility.GetSpriteTexture (groundSprite, false);

        Vector2 coordinate = new Vector2 (groundSprite.uv[0].x, groundSprite.uv[1].y);
        Vector2 size = new Vector2 (32f / groundTexture.width, 32f / groundTexture.height);
        var textureRect = new Rect (coordinate, size);

        GUI.DrawTextureWithTexCoords (rect, groundTexture, textureRect);
    }
    
    private void DrawHorizontalWall (Rect rect)
    {
        var wallSprite = xWallPrefab.sprite;

        Vector2 pivot           = new Vector2 (32, 32);
        Vector2 spriteSize      = new Vector2 (64, 64);
        Vector2 spriteCoord     = new Vector2 (34, 0);
        Vector2Int spriteUv     = new Vector2Int (0, 1);
        Texture2D wallTexture   = SpriteUtility.GetSpriteTexture (wallSprite, false);

        Vector2 size = new Vector2 (spriteSize.x / wallTexture.width, spriteSize.y / wallTexture.height);
        Vector2 coordinate = new Vector2 (  wallSprite.uv [spriteUv.x].x - (spriteCoord.x / wallSprite.texture.width),
                                            wallSprite.uv [spriteUv.y].y - (spriteCoord.y / wallSprite.texture.height));

        GUI.DrawTextureWithTexCoords (rect, wallTexture, new Rect (coordinate, size));
    }

    private void DrawVerticalWall (Rect rect)
    {
        var wallSprite = yWallPrefab.sprite;

        Vector2 pivot           = new Vector2 (32, 32);
        Vector2 spriteSize      = new Vector2 (64, 64);
        Vector2 spriteCoord     = new Vector2 (66, 0);
        Vector2Int spriteUv     = new Vector2Int (0, 1);
        Texture2D wallTexture   = SpriteUtility.GetSpriteTexture (wallSprite, false);

        Vector2 size = new Vector2 (spriteSize.x / wallTexture.width, spriteSize.y / wallTexture.height);
        Vector2 coordinate = new Vector2 (  wallSprite.uv [spriteUv.x].x - (spriteCoord.x / wallSprite.texture.height),
                                            wallSprite.uv [spriteUv.y].y - (spriteCoord.y / wallSprite.texture.height));

        GUI.DrawTextureWithTexCoords (rect, wallTexture, new Rect (coordinate, size));
    }

    private void DrawEnemy (Rect rect)
    {
        Tile tile = (Tile) AssetDatabase.LoadAssetAtPath ("Assets/Tiles/Ti_Minotaur.asset", typeof (Tile));
        Texture2D texture = SpriteUtility.GetSpriteTexture (tile.sprite, false);

        Vector2 textSize = new Vector2 (32f / texture.width, 32f / texture.height);
        Vector2 coordinate = new Vector2 (tile.sprite.uv [0].x - (textSize.x * 0.76f), tile.sprite.uv [0].y - (textSize.y * 0.0f));
        var textureRect = new Rect (coordinate, textSize);

        GUI.DrawTextureWithTexCoords (rect, texture, textureRect);
    }

    private void DrawPlayer (Rect rect)
    {
        Tile tile = (Tile) AssetDatabase.LoadAssetAtPath ("Assets/Tiles/Ti_Theseus.asset", typeof (Tile));
        Texture2D texture = SpriteUtility.GetSpriteTexture (tile.sprite, false);

        Vector2 textSize = new Vector2 (36f / texture.width, 36f / texture.height);
        Vector2 coordinate = new Vector2 (tile.sprite.uv [0].x - (textSize.x * 0.82f), tile.sprite.uv [0].y - (textSize.y * 0.89f));
        var textureRect = new Rect (coordinate, textSize);

        GUI.DrawTextureWithTexCoords (rect, texture, textureRect);
    }

    private void DrawHole (Rect rect)
    {
        Tile tile = (Tile) AssetDatabase.LoadAssetAtPath ("Assets/Tiles/Ti_Hole.asset", typeof (Tile));
        Texture2D texture = SpriteUtility.GetSpriteTexture (tile.sprite, false);

        Vector2 textSize = new Vector2 (32f / texture.width, 32f / texture.height);
        Vector2 coordinate = new Vector2 (tile.sprite.uv [0].x - (textSize.x * 0.00f), tile.sprite.uv [0].y - (textSize.y * 1.0f));
        var textureRect = new Rect (coordinate, textSize);

        GUI.DrawTextureWithTexCoords (rect, texture, textureRect);
    }
}