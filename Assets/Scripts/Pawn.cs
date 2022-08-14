using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves a GameObject while using an Animator.
/// </summary>
public class Pawn : MonoBehaviour
{
    private Animator animator;

    private bool isWalking;
    
    public bool IsWalking {get { return isWalking; } }

    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/
    // Methods
    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/

    private void Awake ()
    {
        animator = GetComponent<Animator> ();
        if (animator == null) {
            animator = GetComponentInChildren<Animator> ();
        }
    }

    /// <summary>
    /// Move this pawn to a certain location, in a specific amount of time, only if possible
    /// </summary>
    /// <param name="position">Location where the pawn will head.</param>
    /// <param name="time"> Time to perform the operation.</param>
    /// <returns>Return if the operation was successuful.</returns>
    public bool Walk (Vector3 position, float time = 1.0f)
    {
        if (isWalking == true) {
            return false;
        }

        Vector3 direction = new Vector3 (position.x, position.y) - transform.position;
        Ray ray = new Ray (transform.position, direction);
        var result = World.Raycast (ray, direction.magnitude);

        if (result.HasFlag (TileState.xWall) || result.HasFlag (TileState.yWall)) {
            return false;
        }
        else {
            StartCoroutine (WalkRoutine (position, time));
            return true;
        }
    }

    /// <summary>
    /// Simple movement done by lerping the position. Also triggers the animator.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    private IEnumerator WalkRoutine (Vector2 position, float time = 1.0f)
    {
        isWalking = true;
        Vector2 start = transform.position;
        time = (1 / time);
        animator.SetFloat ("PlaySpeed", 0.75f);
        float alpha = 0.0f;
        
        // Handle animation
        if (Mathf.Abs (start.y - position.y) > 0.01f) 
        {
            if (position.y > transform.position.y) {
                animator.SetBool ("Up", true);
            }
            else if (position.y < transform.position.y) {
                animator.SetBool ("Down", true);
            }
        }
        else if (Mathf.Abs (start.x - position.x) > 0.01f) 
        {
            if (position.x > transform.position.x) {
                animator.SetBool ("Right", true);
            }
            else {
                animator.SetBool ("Left", true);
            }
        }

        // Lerp position
        while (alpha < 1.0f)
        {
            transform.position = Vector2.Lerp (start, position, alpha);
            alpha += Time.deltaTime * time;
            yield return null;
        }

        animator.SetBool ("Left", false);
        animator.SetBool ("Right", false);
        animator.SetBool ("Up", false);
        animator.SetBool ("Down", false);

        isWalking = false;
    }    
}