using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CarController carController;

    private Vector2 startingPosition;
    private Quaternion startingRotation;

    private void Awake()
    {
        carController = GetComponent<CarController>();
    }

    void FixedUpdate()
    {
        int forwardAction = 0;
        if (Input.GetKey(KeyCode.W)) forwardAction = 1;

        int turnAction = 0;
        if (Input.GetKey(KeyCode.D)) turnAction = 1;
        if (Input.GetKey(KeyCode.A)) turnAction = -1;

        carController.SetInputs(forwardAction, turnAction);
    }

    public void SetStart(List<Vector2> trackPoints)
    {
        startingPosition = trackPoints[0];
        transform.position = startingPosition;

        Vector2 startingDir = trackPoints[1] - trackPoints[0];
        float startingAngle = Mathf.Atan2(startingDir.y, startingDir.x) * Mathf.Rad2Deg - 90f;
        Quaternion rotation = Quaternion.Euler(0, 0, startingAngle);

        startingRotation = rotation;
        transform.rotation = startingRotation;
    }

    public void OnOutOfTrack()
    {
        carController.ResetVelocity();
        transform.position = startingPosition;
        transform.rotation = startingRotation;
    }
}
