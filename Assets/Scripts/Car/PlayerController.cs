using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CarController carController;

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
}
