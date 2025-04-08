using UnityEngine;

public class AIController : MonoBehaviour
{
    private CarController carController;

    private void Awake()
    {
        carController = GetComponent<CarController>();
    }

    void Update()
    {
        carController.SetInputs(0, 1);
    }
}
