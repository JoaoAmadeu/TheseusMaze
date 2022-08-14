using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Class that gather all the elements for the UI and display the information accordingly.
/// </summary>
public class MazeUi : MonoBehaviour
{
    [SerializeField]
    private Button leftButton;

    [SerializeField]
    private Button rightButton;

    [SerializeField]
    private Button upButton;

    [SerializeField]
    private Button downButton;

    [SerializeField]
    private Button passButton;

    [SerializeField]
    private Button restartButton;

    [SerializeField]
    private Button undoButton;

    [SerializeField]
    private Button redoButton;

    [SerializeField]
    private TextMeshProUGUI turnText;

    [SerializeField]
    private TextMeshProUGUI levelDescriptionText;

    [SerializeField]
    private ScrollRect levelScroll;

    [SerializeField]
    private Button levelButtonPrefab;

    [SerializeField]
    private GameObject startWindow;

    [SerializeField]
    private TextMeshProUGUI windowText;

    [SerializeField]
    private Button windowButton;

    [SerializeField]
    private Central central;

    [SerializeField]
    private MovementTracker movementTracker;

    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/
    // Methods
    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/

    private void Awake ()
    {
        central.onControllerCreate += AttachControllerToUi;
        central.onGameEnd += (controller) => OpenWindow (controller.ToString () + " has won");
        windowButton.onClick.AddListener (central.RestartLevel);
        movementTracker.onControllerTurn += ChangeTurnText;
        turnText.text = "";
        levelDescriptionText.text = "";
    }

    private void Start ()
    {
        for (int i = 0; i < central.Levels.Length; i++)
        {
            var ind = i;
            Button levelButton = Instantiate (levelButtonPrefab, Vector3.zero, Quaternion.identity, levelScroll.content);
            levelButton.onClick.AddListener (() => {central.LoadLevel (ind);});

            var buttonText = levelButton.GetComponentInChildren<TextMeshProUGUI> ();
            if (buttonText != null) {
                buttonText.text = "Level " + i.ToString ("00");
            }
        }

        undoButton.onClick.AddListener (movementTracker.UndoCurrentStep);
        redoButton.onClick.AddListener (movementTracker.RedoCurrentStep);
        restartButton.onClick.AddListener (() => central.LoadLevel (central.LevelIndex));
        levelDescriptionText.text = central.CurrentLevel.description;
        OpenWindow ("Start game");
    }

    private void Update ()
    {
        if (Input.GetKeyUp (KeyCode.U)) {
            movementTracker.UndoCurrentStep ();
        }
        if (Input.GetKeyUp (KeyCode.I)) {
            movementTracker.RedoCurrentStep ();
        }
        if (Input.GetKeyUp (KeyCode.R)) {
            central.RestartLevel ();
        }
    }

    private void OpenWindow (string header)
    {
        startWindow.SetActive (true);
        windowText.text = header;
    }

    private void AttachControllerToUi (Controller controller)
    {
        leftButton.onClick.AddListener (() => controller.MoveLeft ());
        rightButton.onClick.AddListener (() => controller.MoveRight ());
        upButton.onClick.AddListener (() => controller.MoveUp ());
        downButton.onClick.AddListener (() => controller.MoveDown ());
        passButton.onClick.AddListener (controller.DoNothing);

        central.onControllerCreate -= AttachControllerToUi;
    }

    private void ChangeTurnText (Controller controller)
    {
        turnText.text = controller.Pawn.name + "\nTurn";
        StartCoroutine (TextAnimation ());
    }

    private IEnumerator TextAnimation ()
    {
        turnText.transform.localScale = Vector3.zero;
        turnText.transform.rotation = Quaternion.Euler (0, 0, -turnText.transform.rotation.eulerAngles.z);
        float timer = 0.0f;

        while (timer < 1.0f)
        {
            turnText.transform.localScale = Vector3.one * timer;
            timer += Time.deltaTime * 5.0f;
            yield return null;
        }

        turnText.transform.localScale = Vector3.one;
    }
}