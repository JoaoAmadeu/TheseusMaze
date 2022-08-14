using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controller which can be controlled through the keyboard arrows.
/// </summary>
public class PlayerController : Controller
{
    private void Update ()
    {
        if (Input.GetKeyUp (KeyCode.RightArrow)) {
            MoveRight ();
        }
        if (Input.GetKeyUp (KeyCode.LeftArrow)) {
            MoveLeft ();
        }
        if (Input.GetKeyUp (KeyCode.UpArrow)) {
            MoveUp ();
        }
        if (Input.GetKeyUp (KeyCode.DownArrow)) {
            MoveDown ();
        }
        if (Input.GetKeyUp (KeyCode.Return)) {
            DoNothing ();
        }
    }
}
