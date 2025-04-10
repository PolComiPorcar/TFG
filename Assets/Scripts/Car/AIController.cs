using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class AIController : Agent
{
    public readonly float rewardOnCheckpoint = 1;

    private CarController carController;

    private void Start()
    {
        carController = GetComponent<CarController>();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        carController.SetInputs(actions.DiscreteActions[0], actions.DiscreteActions[1]);
        
        Debug.Log("throttle: " + actions.DiscreteActions[0] + ", steering: " + actions.DiscreteActions[1]);
    }

    public void OnReachCheckpoint()
    {
        this.AddReward(rewardOnCheckpoint);
    }
}
