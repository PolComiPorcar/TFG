using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UI;

public class AIController : Agent
{
    public const float REWARD_ON_CORRECT_CHECKPOINT = 1;
    public const float REWARD_ON_FINISH_TRACK = 5; //20
    public const float REWARD_ON_WRONG_CHECKPOINT = 0; //-1f
    public const float REWARD_ON_LOSE = -5f;
    public const float REWARD_SPEED_FACTOR = 0; //0.002f

    public const float DIST_POINT_LOOK_AHEAD = 1.5f;

    [SerializeField] RaceController raceController;
    private CarController carController;

    private Vector2 startingPosition;
    private Quaternion startingRotation;
    private List<Checkpoint> trackCheckpoints;
    private List<Vector2> trackPoints;
    private int checkpointsCrossed = 0;
    private float semiDiagonal;

    //private float timerDuration = 15f;
    //private float timerValue;

    private float raceTimerValue;

    // DEBUG ONLY
    [SerializeField] private Slider sliderLinearVel;
    [SerializeField] private Slider sliderLateralVel;
    [SerializeField] private Slider sliderDistCenterline;
    [SerializeField] private Slider sliderAngleNextCheckpoint;
    [SerializeField] private Slider sliderAngleNextCheckpointY;
    [SerializeField] TMP_Text textReward;
    [SerializeField] TMP_Text textLastEpisodeReward;
    [SerializeField] TMP_Text textNumEpisodes;
    //[SerializeField] TMP_Text textTimer;
    [SerializeField] TMP_Text textLastRaceTimer;
    [SerializeField] TMP_Text textActualTimer;
    [SerializeField] SpriteRenderer SRSquareFinishedRace;

    [SerializeField] GameObject testCircle;


    public int CheckpointsCrossed
    {
        get { return checkpointsCrossed; }
    }

    private void Start()
    {
        carController = GetComponent<CarController>();
        Vector2 spriteSize = GetComponent<SpriteRenderer>().bounds.size;
        semiDiagonal = 0.5f * spriteSize.magnitude;

        //timerValue = timerDuration;
        raceTimerValue = 0;
    }
    
    private void Update()
    {
        if (textReward != null) textReward.text = GetCumulativeReward().ToString("F1");
        if (textNumEpisodes != null) textNumEpisodes.text = CompletedEpisodes.ToString();
    }

    private void FixedUpdate()
    {
        raceTimerValue += Time.fixedDeltaTime;

        int seconds = Mathf.FloorToInt(raceTimerValue);
        int decimalSeconds = Mathf.FloorToInt((raceTimerValue - seconds) * 100);
        if (textActualTimer != null) textActualTimer.text = string.Format("{0:00}:{1:00}", seconds, decimalSeconds);

        //timerValue -= Time.fixedDeltaTime;

        /*if (timerValue <= 0)
        {
            ResetTimer();
            AddReward(REWARD_ON_LOSE);
            EndEpisode(false);
        }

        int seconds = Mathf.FloorToInt(timerValue);
        int decimalSeconds = Mathf.FloorToInt((timerValue - seconds) * 100);
        if (textTimer != null) textTimer.text = string.Format("{0:00}:{1:00}", seconds, decimalSeconds);*/
    }

    public override void OnEpisodeBegin()
    {
        checkpointsCrossed = 0;
        raceTimerValue = 0;

        carController.ResetVelocity();
        transform.position = startingPosition;
        transform.rotation = startingRotation;

        trackCheckpoints[0].gameObject.SetActive(true);
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
        if (sliderLinearVel != null) sliderLinearVel.value = linearVelMapped;

        float lateralVelMapped = carController.LateralVel / carController.MaxSpeed; // values are from [0-3] -> [0-1]
        sensor.AddObservation(lateralVelMapped);
        if (sliderLateralVel != null) sliderLateralVel.value = lateralVelMapped;


        (float distCenterline, Vector2 closestPoint, int pointIndex) = GetDistanceToCenterLine(transform.position);

        float distCenterlineMapped = distCenterline / (raceController.TrackWidth / 2f + semiDiagonal);
        sensor.AddObservation(distCenterlineMapped);
        if (sliderDistCenterline != null) sliderDistCenterline.value = distCenterlineMapped;

        /*Vector2 pointAhead = GetDirectionPointAheadByDistance(closestPoint, pointIndex, DIST_POINT_LOOK_AHEAD);
        testCircle.transform.position = pointAhead; // DEBUG ONLY */

        //Vector2 aheadPointDir = GetDirectionPointAheadByDistance(closestPoint, pointIndex, linearVelMapped * DIST_POINT_LOOK_AHEAD);
        /*float directionDot = Vector2.Dot(transform.up, aheadPointDir); // [-1, 1]
        float directionDotMapped = (directionDot + 1) / 2; // [0, 1]
        sensor.AddObservation(directionDotMapped);
        if (sliderAngleNextCheckpoint != null) sliderAngleNextCheckpoint.value = directionDotMapped;*/

        //Vector2 direction = aheadPointDir - new Vector2(transform.up.x, transform.up.y);

        // Add both components as observations
        //sensor.AddObservation(direction.x); // Left/right component [-1, 1]
        //if (sliderAngleNextCheckpoint != null) sliderAngleNextCheckpoint.value = direction.x;
        /*sensor.AddObservation(direction.y);
        if (sliderAngleNextCheckpointY != null) sliderAngleNextCheckpointY.value = direction.y;*/
        //sensor.AddObservation(localAheadDir2D.y); // Forward/backward component [-1, 1]

        /*Vector2 nextCheckpointDir = trackCheckpoints[checkpointsCrossed].transform.up;
        float directionDot = Vector2.Dot(transform.up, nextCheckpointDir); // [-1, 1]
        float directionDotMapped = (directionDot + 1) / 2; // [0, 1]
        sensor.AddObservation(directionDotMapped);
        if (sliderAngleNextCheckpoint != null) sliderAngleNextCheckpoint.value = directionDotMapped;*/

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

        AddReward(carController.LinearVel * REWARD_SPEED_FACTOR);
    }

    public void EndEpisode(bool endedRace)
    {
        if (textLastEpisodeReward != null) textLastEpisodeReward.text = GetCumulativeReward().ToString("F1");

        if (SRSquareFinishedRace != null)
        {
            if (endedRace) SRSquareFinishedRace.color = Color.green;
            else SRSquareFinishedRace.color = Color.red;
        }

        //ResetTimer();

        foreach (Checkpoint cp in trackCheckpoints) {
            cp.gameObject.SetActive(false);
        }

        int secondsRaceTimer = Mathf.FloorToInt(raceTimerValue);
        int decimalRaceTimer = Mathf.FloorToInt((raceTimerValue - secondsRaceTimer) * 100);
        if (textLastRaceTimer != null) textLastRaceTimer.text = string.Format("{0:00}:{1:00}", secondsRaceTimer, decimalRaceTimer);

        EndEpisode();
    } 

    /*public void ResetTimer()
    {
        timerValue = timerDuration;
    }*/

    public void OnReachCheckpoint(int checkpointIndex, int nCheckpoints)
    {
        //ResetTimer();

        if (checkpointIndex == checkpointsCrossed) // is the next checkpoint
        {
            trackCheckpoints[checkpointIndex].gameObject.SetActive(false);

            if (checkpointIndex == nCheckpoints - 1) // if it is the last checkpoint
            {
                AddReward(REWARD_ON_FINISH_TRACK);
                AddReward(LerpClamped(10f, 25f, 10f, 0f, raceTimerValue));
                EndEpisode(true);
            }
            else
            {
                trackCheckpoints[checkpointIndex + 1].gameObject.SetActive(true);

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

    public void SetStart(List<Vector2> points, List<Checkpoint> checkpoints)
    {
        trackPoints = points;
        startingPosition = trackPoints[0];
        trackCheckpoints = checkpoints;

        Vector2 startingDir = trackPoints[1] - trackPoints[0];
        float startingAngle = Mathf.Atan2(startingDir.y, startingDir.x) * Mathf.Rad2Deg - 90f;
        Quaternion rotation = Quaternion.Euler(0, 0, startingAngle);

        startingRotation = rotation;
    }

    public float LerpClamped(float inputMin, float inputMax, float outputMin, float outputMax, float t)
    {
        float tClamped = Mathf.Clamp(t, inputMin, inputMax);
        float output = outputMin + (tClamped - inputMin) * (outputMax - outputMin) / (inputMax - inputMin);

        return output;
    }

    private (float, Vector2, int) GetDistanceToCenterLine(Vector2 position)
    {
        float minDistance = float.MaxValue;
        Vector2 closestPoint = Vector2.zero;
        int pointIndex = 0;

        // Check each segment of the track
        for (int i = 0; i < trackPoints.Count - 1; i++)
        {
            Vector2 segmentStart = trackPoints[i];
            Vector2 segmentEnd = trackPoints[i + 1];

            Vector2 actualClosestPoint = GetClosestPointOnLineSegment(position, segmentStart, segmentEnd);
            float distance = Vector2.Distance(position, actualClosestPoint);

            // Keep track of the minimum distance found
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = actualClosestPoint;
                pointIndex = i;
            }
        }

        return (minDistance, closestPoint, pointIndex);
    }

    private Vector2 GetClosestPointOnLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 lineDirection = lineEnd - lineStart;
        float lineLength = lineDirection.magnitude;

        lineDirection.Normalize();

        // Calculate projection of point onto line
        float projection = Vector2.Dot(point - lineStart, lineDirection);

        // Clamp projection to line segment
        projection = Mathf.Clamp(projection, 0, lineLength);

        // Calculate closest point
        return lineStart + projection * lineDirection;
    }

    private Vector2 GetDirectionPointAheadByDistance(Vector2 startingPos, int pointIndex, float totalDistance)
    {
        int i = pointIndex;
        int nextI = (i + 1) % trackPoints.Count;
        float distTraveled = Vector2.Distance(startingPos, trackPoints[nextI]);

        while (distTraveled < totalDistance)
        {
            i = (i + 1) % trackPoints.Count; // i++
            nextI = (i + 1) % trackPoints.Count;

            distTraveled += Vector2.Distance(trackPoints[i], trackPoints[nextI]);
        }

        //return trackPoints[i]; // DEBUG ONLY

        nextI = (i + 1) % trackPoints.Count;
        Vector2 pointDir = (trackPoints[nextI] - trackPoints[i]).normalized;

        return pointDir;
    }
}
