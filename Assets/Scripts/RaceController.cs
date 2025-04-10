using UnityEngine;
using System.Collections.Generic;

public class RaceController : MonoBehaviour
{
    [SerializeField] bool useRandomSeed = false;
    [SerializeField] int seed = 0;

    [SerializeField] int width = 16;
    [SerializeField] int height = 9;

    [SerializeField] int initialNumberOfPoints = 10;
    [Range(1, 99)][SerializeField] int curveResolution = 10;
    [Range(140, 180)][SerializeField] int maxAngleThreshold = 160;

    [SerializeField] GameObject checkpointPrefab;
    [Range(0.1f, 2f)][SerializeField] float checkpointInterval = 0.5f;

    [SerializeField] AIController AIAgent;

    private Track track;
    private TrackDrawer drawer;
    private TrackCollider trackCollider;

    private List<Checkpoint> checkpoints = new();
    private int currentAICheckpointIndex = 0;

    //DEBUG
    private bool showPoints = false;
    private bool showOtherCheckpoints = true;

    private void Start()
    {
        drawer = GetComponent<TrackDrawer>();
        trackCollider = GetComponent<TrackCollider>();
    }

    public void CreateTrack()
    {
        // Random seed generator
        if (useRandomSeed) seed = new System.Random().Next(int.MinValue, int.MaxValue);

        track = new Track(seed, curveResolution);
        track.CreateFullTrack(initialNumberOfPoints, width, height, (float)maxAngleThreshold);

        Debug.Log("Track distance: " + track.Distance);

        //drawer.DrawPoints(track, true);
        drawer.DrawCurvedPoints(track, showPoints);
        trackCollider.GenerateTrackCollider();

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
        float checkpointAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

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

        //TODO: Validar que el Collider2D és la IA i no el jugador

        if (checkpointIndex == currentAICheckpointIndex)
        {
            AIAgent.OnReachCheckpoint();
            checkpoints[checkpointIndex].GetComponent<SpriteRenderer>().enabled = showOtherCheckpoints;
            checkpoints[(checkpointIndex + 1) % checkpoints.Count].GetComponent<SpriteRenderer>().enabled = true;
            Debug.Log("Car crossed the correct checkpoint!");

            currentAICheckpointIndex++;
        }
    }

    public void ToggleShowPoints()
    {
        showPoints = !showPoints;
        drawer.DrawCurvedPoints(track, showPoints);
    }

    public void ToggleShowOtherCheckpoints()
    {
        showOtherCheckpoints = !showOtherCheckpoints;

        for (int i = 0; i < checkpoints.Count; i++)
        {
            if (i != currentAICheckpointIndex) checkpoints[i].GetComponent<SpriteRenderer>().enabled = showOtherCheckpoints;
        }
    }
}
