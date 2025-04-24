using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.UI;
using TMPro;

public class RaceController : MonoBehaviour
{
    [SerializeField] bool useRandomSeed = false;
    [SerializeField] int seed = 0;

    [SerializeField] int width = 16;
    [SerializeField] int height = 9;

    [SerializeField] int initialNumberOfPoints = 10;
    [SerializeField] float trackWidth = 1f;
    [Range(1, 99)][SerializeField] int curveResolution = 10;
    [Range(140, 180)][SerializeField] int maxAngleThreshold = 160;

    [SerializeField] GameObject checkpointPrefab;
    [Range(0.1f, 2f)][SerializeField] float checkpointInterval = 0.5f;

    [SerializeField] AIController AIAgent;

    private Track track;
    private TrackDrawer drawer;
    private TrackCollider trackCollider;

    private List<Checkpoint> checkpoints = new();

    //DEBUG
    private bool showPoints = false;
    private bool showOtherCheckpoints = true;

    public float TrackWidth
    {
        get { return trackWidth; }
    }
    public List<Checkpoint> Checkpoints
    {
        get { return checkpoints; }
    }

    private void Start()
    {
        drawer = GetComponent<TrackDrawer>();
        trackCollider = GetComponent<TrackCollider>();

        CreateTrack();
    }

    public void CreateTrack()
    {
        // Random seed generator
        if (useRandomSeed) seed = new System.Random().Next(int.MinValue, int.MaxValue);

        track = new Track(seed, curveResolution);
        track.CreateFullTrack(initialNumberOfPoints, width, height, (float)maxAngleThreshold);

        Vector2 startingDir = track.CurveResolutionPoints[1] - track.CurveResolutionPoints[0];
        float startingAngle = Mathf.Atan2(startingDir.y, startingDir.x) * Mathf.Rad2Deg - 90f;
        Quaternion startingRotation = Quaternion.Euler(0, 0, startingAngle);
        AIAgent.SetStart(track.CurveResolutionPoints[0], startingRotation);

        Debug.Log("Track distance: " + track.Distance);

        //drawer.DrawPoints(track, true);
        drawer.DrawCurvedPoints(track, trackWidth, showPoints);
        trackCollider.GenerateTrackCollider(track.CurveResolutionPoints, trackWidth);

        GenerateCheckpoints();
    }

    public void GenerateCheckpoints()
    {
        foreach (Checkpoint cp in checkpoints)
        {
            if (cp != null) Destroy(cp.gameObject);
        }
        checkpoints.Clear();

        int nCheckpoints = (int)(track.Distance / checkpointInterval);
        float distanceBetween = track.Distance / nCheckpoints;
        List<Vector2> trackPoints = track.CurveResolutionPoints;

        Debug.Log("Original approximate checkpoint interval: " + checkpointInterval + ", actual interval: " + distanceBetween);
        Debug.Log("number of checkpoints that should be added: " + nCheckpoints);

        float distanceTraveled = 0f;
        float nextCheckpointAt = 0f; // Start with first checkpoint at beginning

        // Loop through track segments
        for (int i = 0; i < trackPoints.Count - 1; i++)
        {
            Vector2 startPos = trackPoints[i];
            Vector2 endPos = trackPoints[i + 1];
            float segmentLength = Vector2.Distance(startPos, endPos);

            // Check if we need to place checkpoints in this segment
            while (nextCheckpointAt >= distanceTraveled && nextCheckpointAt <= distanceTraveled + segmentLength)
            {
                if (checkpoints.Count == nCheckpoints) return;

                // Calculate position within segment
                float segmentPosition = (nextCheckpointAt - distanceTraveled) / segmentLength;
                Vector2 checkpointPosition = Vector2.Lerp(startPos, endPos, segmentPosition);
                Vector2 checkpointDirection = (endPos - startPos).normalized;
                
                if (i != 0) CreateCheckpoint(checkpointPosition, checkpointDirection);

                // Move to next checkpoint position
                nextCheckpointAt += distanceBetween;
            }

            distanceTraveled += segmentLength;
        }

        if (checkpoints.Count != nCheckpoints) CreateCheckpoint(trackPoints[0], (trackPoints[1] - trackPoints[0]).normalized);
    }

    private void CreateCheckpoint(Vector2 pos, Vector2 dir)
    {
        float checkpointAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        // Create checkpoint
        GameObject checkpointObj = Instantiate(checkpointPrefab, pos, Quaternion.identity);
        checkpointObj.name = $"Checkpoint_{checkpoints.Count}";
        checkpointObj.transform.rotation = Quaternion.Euler(0, 0, checkpointAngle);

        // Add event listener
        Checkpoint checkpoint = checkpointObj.GetComponent<Checkpoint>();
        checkpoint.OnCheckpointEntered.AddListener(HandleCheckpointTrigger);

        checkpoints.Add(checkpoint);
    }

    private void HandleCheckpointTrigger(Checkpoint cp, Collider2D other)
    {
        int checkpointIndex = checkpoints.IndexOf(cp);

        if (AIAgent.GetComponent<Collider2D>() == other)
        {            
            AIAgent.OnReachCheckpoint(checkpointIndex, checkpoints.Count);

            //Debug Only
            checkpoints[checkpointIndex].GetComponent<SpriteRenderer>().enabled = showOtherCheckpoints;
            checkpoints[(checkpointIndex + 1) % checkpoints.Count].GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    public void OnBorderTriggerExit(Collider2D other)
    {
        if (other.CompareTag("Car"))
        {
            if (!IsCarInsideTrack(other.gameObject.transform.position))
            {
                if (AIAgent.GetComponent<Collider2D>() == other) AIAgent.OnOutOfTrack();

                //other.GetComponent<CarController>().ChangeSpriteColor(Color.red);
            }
            //else other.GetComponent<CarController>().ChangeSpriteColor(Color.white);
        }
    }

    private bool IsCarInsideTrack(Vector3 position)
    {
        List<Vector2> trackPoints = track.CurveResolutionPoints;

        for (int i = 0; i < trackPoints.Count - 1; i++)
        {
            Vector2 segmentStart = trackPoints[i];
            Vector2 segmentEnd = trackPoints[i + 1];

            // Calculate the closest point on this track segment to the car
            Vector2 closestPoint = GetClosestPointOnLineSegment(position, segmentStart, segmentEnd);

            // Calculate the distance from car to this closest point
            float distance = Vector2.Distance(position, closestPoint);

            // If this distance is less than half track width, car is inside the track
            if (distance <= trackWidth / 2f) return true;
        }

        return false;
    }

    public float GetDistanceToCenterLine(Vector2 position)
    {
        List<Vector2> trackPoints = track.CurveResolutionPoints;
        float minDistance = float.MaxValue;

        // Check each segment of the track
        for (int i = 0; i < trackPoints.Count - 1; i++)
        {
            Vector2 segmentStart = trackPoints[i];
            Vector2 segmentEnd = trackPoints[i + 1];

            Vector2 closestPoint = GetClosestPointOnLineSegment(position, segmentStart, segmentEnd);
            float distance = Vector2.Distance(position, closestPoint);

            // Keep track of the minimum distance found
            if (distance < minDistance) 
                minDistance = distance;
        }

        return minDistance;
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

    public void ToggleShowPoints()
    {
        showPoints = !showPoints;
        drawer.DrawCurvedPoints(track, trackWidth, showPoints);
    }

    public void ToggleShowOtherCheckpoints()
    {
        showOtherCheckpoints = !showOtherCheckpoints;

        for (int i = 0; i < checkpoints.Count; i++)
        {
            if (i != AIAgent.CheckpointsCrossed) checkpoints[i].GetComponent<SpriteRenderer>().enabled = showOtherCheckpoints;
        }
    }
}
