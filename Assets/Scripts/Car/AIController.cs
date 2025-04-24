using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UI;

public class AIController : Agent
{
    public const float REWARD_ON_CORRECT_CHECKPOINT = 1;
    public const float REWARD_ON_FINISH_TRACK = 20;
    public const float REWARD_ON_WRONG_CHECKPOINT = -1;
    public const float REWARD_ON_LOSE = -5;
    public const float REWARD_SPEED_FACTOR = 0; //0.002f

    [SerializeField] RaceController raceController;
    private CarController carController;

    private Vector2 startingPosition;
    private Quaternion startingRotation;
    private int checkpointsCrossed = 0;
    private float semiDiagonal;

    private float timerDuration = 15f;
    private float timerValue;

    // DEBUG ONLY
    [SerializeField] private Slider sliderLinearVel;
    [SerializeField] private Slider sliderLateralVel;
    [SerializeField] private Slider sliderDistCenterline;
    [SerializeField] private Slider sliderDistNextCheckpoint;
    [SerializeField] private Slider sliderAngleNextCheckpoint;
    [SerializeField] TMP_Text textReward;
    [SerializeField] TMP_Text textLastEpisodeReward;
    [SerializeField] TMP_Text textNumEpisodes;
    [SerializeField] TMP_Text textTimer;
    [SerializeField] SpriteRenderer SRSquareFinishedRace;


    public int CheckpointsCrossed
    {
        get { return checkpointsCrossed; }
    }

    private void Start()
    {
        carController = GetComponent<CarController>();
        Vector2 spriteSize = GetComponent<SpriteRenderer>().bounds.size;
        semiDiagonal = 0.5f * spriteSize.magnitude;

        timerValue = timerDuration;
    }
    
    private void Update()
    {
        textReward.text = GetCumulativeReward().ToString("F1");
        textNumEpisodes.text = CompletedEpisodes.ToString();
    }

    private void FixedUpdate()
    {
        timerValue -= Time.fixedDeltaTime;

        if (timerValue <= 0)
        {
            ResetTimer();
            AddReward(REWARD_ON_LOSE);
            EndEpisode(false);
        }

        int seconds = Mathf.FloorToInt(timerValue);
        int decimalSeconds = Mathf.FloorToInt((timerValue - seconds) * 100);
        textTimer.text = string.Format("{0:00}:{1:00}", seconds, decimalSeconds);
    }

    public override void OnEpisodeBegin()
    {
        checkpointsCrossed = 0;

        carController.ResetVelocity();
        transform.position = startingPosition;
        transform.rotation = startingRotation;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        int forwardAction = 0;
        if (Input.GetKey(KeyCode.W)) forwardAction = 1;

        int turnAction = 0;
        if (Input.GetKey(KeyCode.D)) turnAction = 1;
        if (Input.GetKey(KeyCode.A)) turnAction = 2;

        discreteActions[0] = forwardAction;
        discreteActions[1] = turnAction;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float linearVelMapped = carController.LinearVel / carController.MaxSpeed; // values are from [0-3] -> [0-1]
        sensor.AddObservation(linearVelMapped);
        sliderLinearVel.value = linearVelMapped;

        float lateralVelMapped = carController.LateralVel / carController.MaxSpeed; // values are from [0-3] -> [0-1]
        sensor.AddObservation(lateralVelMapped);
        sliderLateralVel.value = lateralVelMapped;

        float distCenterlineMapped = raceController.GetDistanceToCenterLine(transform.position) / (raceController.TrackWidth / 2f + semiDiagonal);
        sensor.AddObservation(distCenterlineMapped);
        sliderDistCenterline.value = distCenterlineMapped;

        Vector2 nextCheckpointPos = raceController.Checkpoints[checkpointsCrossed].transform.position;
        float distNextCheckpointMapped = Mathf.Clamp01((Vector2.Distance(transform.position, nextCheckpointPos)) / 7f); // [0, 1]
        sensor.AddObservation(distNextCheckpointMapped);
        sliderDistNextCheckpoint.value = distNextCheckpointMapped;

        Vector2 nextCheckpointDir = raceController.Checkpoints[checkpointsCrossed].transform.up;
        float directionDot = Vector2.Dot(transform.up, nextCheckpointDir); // [-1, 1]
        float directionDotMapped = (directionDot + 1) / 2; // [0, 1]
        sensor.AddObservation(directionDotMapped);
        sliderAngleNextCheckpoint.value = directionDotMapped;

        // Signed Angle Calculation
        /*float angle = Vector2.SignedAngle(transform.up, nextCheckpointDir); // [-180, 180]
        //float angleMapped = (angle + 180) / 360; // [0, 1]
        float angleMapped = angle / 180f; // [-1, 1]*/
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float turnAmount = 0f;
        switch (actions.DiscreteActions[1])
        {
            case 0: turnAmount = 0f; break;
            case 1: turnAmount = 1f; break;
            case 2: turnAmount = -1f; break;
        }

        carController.SetInputs(actions.DiscreteActions[0], turnAmount);

        if (REWARD_SPEED_FACTOR != 0) AddReward(carController.LinearVel * REWARD_SPEED_FACTOR);
    }

    public void EndEpisode(bool endedRace)
    {
        textLastEpisodeReward.text = GetCumulativeReward().ToString("F1");
        
        if (endedRace) SRSquareFinishedRace.color = Color.green;
        else SRSquareFinishedRace.color = Color.red;

        ResetTimer();

        EndEpisode();
    } 

    public void ResetTimer()
    {
        timerValue = timerDuration;
    }

    public void OnReachCheckpoint(int checkpointIndex, int nCheckpoints)
    {
        ResetTimer();

        if (checkpointIndex == checkpointsCrossed) // is the next checkpoint
        {
            if (checkpointIndex == nCheckpoints - 1) // if it is the last checkpoint
            {
                AddReward(REWARD_ON_FINISH_TRACK);
                EndEpisode(true);
            }
            else
            {
                checkpointsCrossed++;
                AddReward(REWARD_ON_CORRECT_CHECKPOINT);
            }
        }
        else // is a wrong checkpoint
        {
            AddReward(REWARD_ON_WRONG_CHECKPOINT);
        }
    }

    public void OnOutOfTrack()
    {
        AddReward(REWARD_ON_LOSE);
        EndEpisode(false);
    }

    public void SetStart(Vector2 position, Quaternion rotation)
    {
        startingPosition = position;
        startingRotation = rotation;
    }
}
