using UnityEngine;

public class PlayerController : CarController
{
    // Update is called once per frame
    void Update()
    {
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        SetInputs(verticalInput, horizontalInput);
    }
}
