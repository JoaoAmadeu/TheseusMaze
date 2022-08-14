using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AI that follows a target.
/// </summary>
public class AiController : Controller
{
    /// <summary>
    /// The Transform this AI will try to reach.
    /// </summary>
    [SerializeField]
    [ReadOnly]
    [Tooltip("The Transform this AI will try to reach.")]
    private Transform target;

    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/
    // Methods
    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/

    public void SetTarget (Transform target)
    {
        this.target = target;
    }

    private void OnEnable ()
    {
        if (target == null)
            return;
        
        MakeDecision ();
    }
    
    public void MakeDecision ()
    {
        StartCoroutine (MakeDecisionRoutine ());
    }

    private IEnumerator MakeDecisionRoutine ()
    {
        yield return new WaitForSeconds (0.1f);

        if (target == null) {
            DoNothing ();
        }

        // Solution to avoid floating numbers too small to compare
        Vector3 difference = Pawn.transform.position - target.position;

        // Logic hierarchy used to move the pawn.
        if (difference.x > 0.1f && MoveLeft ()) {
            //
        }
        else if (difference.x < -0.1f && MoveRight ()) {
            //
        }
        else if (difference.y > 0.1f && MoveDown ()) {
            //
        }
        else if (difference.y < -0.1f && MoveUp ()) {
            //
        }

        if (Pawn.IsWalking == false) {
            DoNothing ();
        }
    }
}