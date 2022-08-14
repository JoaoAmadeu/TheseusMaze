using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class responsible for recording the controller position at each turn. It can undo and redo these steps.
/// </summary>
public class MovementTracker : MonoBehaviour
{
    private List<KeyValuePair<Controller, Vector3>> steps = new List<KeyValuePair<Controller, Vector3>> ();

    [SerializeField][ReadOnly]
    private int index;

    [SerializeField][ReadOnly][Tooltip("How many steps have been recorded")]
    private int counter;

    [SerializeField]
    private Central central;

    private Dictionary<Controller, Movement> tracker = new Dictionary<Controller, Movement> ();

    private int playerIndex;

    public event Action<Controller> onControllerTurn;

    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/
    // Initialization
    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/

    private void Awake ()
    {
        index = -1;
        central.onControllerCreate += TrackController;
        central.onLevelLoaded += (l) => SetControllerActive(0);
    }

    private void SetControllerActive (int index)
    {
        playerIndex = index;
        if (playerIndex >= tracker.Keys.Count) {
            playerIndex = 0;
        }

        var controller = tracker.Keys.ElementAt (playerIndex);
        controller.enabled = true;
        onControllerTurn?.Invoke (controller);
    }

    private void TrackController (Controller controller)
    {        
        controller.onActionDone.AddListener (PlayerActionDone);
        controller.onActionBegin.AddListener (PlayerActionBegin);
        controller.onActionBegin.AddListener (RecordNewStep);
        tracker.Add (controller, GetDefaultMovement (controller));
    }

    private Movement GetDefaultMovement (Controller controller)
    {
        if (controller is AiController) {
            return new Movement (0, 2);
        }

        return new Movement (0, 1);
    }

    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/
    // Movements
    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/

    public void UndoCurrentStep ()
    {
        if (index < 0) {
            return;
        } 
        
        steps [index].Key.Pawn.transform.position = steps [index].Value;
        index--;
    }

    public void RedoCurrentStep ()
    {
        if (index >= counter) {
            return;
        }
        
        index++;
        steps [index].Key.Pawn.transform.position = steps [index].Value;
    }

    private void PlayerActionBegin (Controller controller)
    {
        controller.enabled = false;
    }

    private void PlayerActionDone (Controller controller)
    {
        Movement movement;

        if (tracker.TryGetValue (controller, out movement) == false) {
            return;
        }
        
        movement.counter++;

        // The player is still allowed to continue
        if (movement.counter < movement.limit) {
            controller.enabled = true;
        }
        // player turn has ended
        else {
            movement.counter = 0;
            SetControllerActive (playerIndex + 1);
        }
    }

    private void RecordNewStep (Controller controller)
    {
        // If the index don't represent the last element of the steps list, all the elements
        // after the index value will be wiped
        if (index != counter - 1)
        {
            var range = steps.GetRange (0, index);
            steps.Clear ();
            steps = range;

            counter = steps.Count;
            index = counter - 1;
        }

        steps.Add (new KeyValuePair<Controller, Vector3> (controller, controller.Pawn.transform.position));
        index++;
        counter++;
    }
}