using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Adjust a Camera size to match the specified number of cells.
/// </summary>
[RequireComponent (typeof (Camera))]
public class CameraGrid : MonoBehaviour
{
    [SerializeField]
    private Vector3 startingPoint;

    [SerializeField]
    private Grid grid;

    [SerializeField]
    private Vector2 cellCount = new Vector2 (15, 15);

    [SerializeField]
    private Vector2 offset;

    private new Camera camera;

    private void OnValidate ()
    {
        if (camera == null) {}
            camera = GetComponent<Camera> ();

        var size = camera.orthographicSize;
            var position = startingPoint;

        camera.orthographicSize = ((cellCount.y * grid.cellSize.y) + offset.y) * 0.5f;
        camera.transform.position = new Vector3 (position.x + size, position.y + size, position.z);
    }

    private void Start ()
    {
        if (camera == null) {}
            camera = GetComponent<Camera> ();

        var size = camera.orthographicSize;
            var position = startingPoint;

        camera.orthographicSize = ((cellCount.y * grid.cellSize.y) + offset.y) * 0.5f;
        camera.transform.position = new Vector3 (position.x + size, position.y + size, position.z);
    }
}