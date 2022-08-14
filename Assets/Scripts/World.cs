using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class World
{
    /// <summary>
    /// Event responsible for checking collider on the Arena.
    /// </summary>
    public static Func<Vector3, TileState> collisionEvent;

    /// <summary>
    /// Event responsible for raycast on the Arena.
    /// </summary>
    public static Func<Ray, float, TileState> raycastEvent;

    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/
    // Methods
    /*////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/

    /// <summary>
    /// Alternative to using Physics2D raycast as it is too costly in performance
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public static TileState GetCollider (Vector3 position)
    {
        return collisionEvent (position);
        
    }

    /// <summary>
    /// Alternative to using Physics2D raycast as it is too costly in performance
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public static TileState Raycast (Ray ray, float distance)
    {
        return raycastEvent (ray, distance);
    }
}