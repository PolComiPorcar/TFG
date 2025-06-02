using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.UI;
using TMPro;
using Unity.MLAgents;

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

    [SerializeField] List<AIController> ListAIAgents;
    [SerializeField] PlayerController player;

    private Track track;
    private TrackDrawer drawer;
    private TrackCollider trackCollider;

    private List<List<Checkpoint>> checkpoints = new();

    //DEBUG
    private bool showPoints = false;

    public float TrackWidth
    {
        get { return trackWidth; }
    }

    private void Start()
    {
        drawer = GetComponent<TrackDrawer>();
        trackCollider = GetComponent<TrackCollider>();

        for (int i = 0; i < ListAIAgents.Count; i++)
        {
            GameObject container = new GameObject("Checkpoints " + i);
            container.transform.parent = transform;
        }

        CreateTrack();
    }

    public void CreateTrack()
    {
        // Random seed generator
        if (useRandomSeed) seed = new System.Random().Next(int.MinValue, int.MaxValue);

        track = new Track(seed, curveResolution);
        track.CreateFullTrack(initialNumberOfPoints, width, height, (float)maxAngleThreshold);

        Debug.Log("Track distance: " + track.Distance);

        //drawer.DrawPoints(track, true);
        //drawer.DrawConvexHull(track);
        drawer.DrawCurvedPoints(track, trackWidth, showPoints);
        trackCollider.GenerateTrackCollider(track.CurveResolutionPoints, trackWidth);

        GenerateCheckpoints();

        for (int i = 0; i < ListAIAgents.Count; i++)
        {
            ListAIAgents[i].SetStart(track.CurveResolutionPoints, checkpoints[i]);
        }

        player.SetStart(track.CurveResolutionPoints);
    }

    public void GenerateCheckpoints()
    {
        foreach (List<Checkpoint> listCp in checkpoints)
        {
            foreach (Checkpoint cp in listCp)
            {
                if (cp != null) Destroy(cp.gameObject);
            }
        }
        checkpoints.Clear();

        foreach (AIController agent in ListAIAgents)
        {
            checkpoints.Add(new List<Checkpoint>());
        }

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
                if (checkpoints[0].Count == nCheckpoints) return;

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

        if (checkpoints[0].Count != nCheckpoints) CreateCheckpoint(trackPoints[0], (trackPoints[1] - trackPoints[0]).normalized);
    }

    private void CreateCheckpoint(Vector2 pos, Vector2 dir)
    {
        float checkpointAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        for (int i = 0; i < ListAIAgents.Count; i++)
        {
            // Create checkpoint
            GameObject checkpointObj = Instantiate(checkpointPrefab, pos, Quaternion.identity);
            checkpointObj.transform.localScale = new Vector3(trackWidth, 0.1f, 1);
            checkpointObj.name = $"Checkpoint_{checkpoints[i].Count}";
            checkpointObj.transform.rotation = Quaternion.Euler(0, 0, checkpointAngle);
            checkpointObj.transform.SetParent(this.gameObject.transform.GetChild(i), worldPositionStays: true);
            checkpointObj.layer = LayerMask.NameToLayer("Checkpoint" + (i + 1));
            checkpointObj.SetActive(false);

            // Add event listener
            Checkpoint checkpoint = checkpointObj.GetComponent<Checkpoint>();
            checkpoint.OnCheckpointEntered.AddListener(HandleCheckpointTrigger);

            checkpoints[i].Add(checkpoint);
        }
    }

    private void HandleCheckpointTrigger(Checkpoint cp, Collider2D other)
    {
        int indexAgent = 0;
        while (indexAgent < ListAIAgents.Count)
        {
            if (ListAIAgents[indexAgent].GetComponent<Collider2D>() == other) break;
            else indexAgent++;
        }

        if (indexAgent >= ListAIAgents.Count) return;

        int checkpointIndex = checkpoints[indexAgent].IndexOf(cp);
        ListAIAgents[indexAgent].OnReachCheckpoint(checkpointIndex, checkpoints[indexAgent].Count);
    }

    public void OnBorderTriggerExit(Collider2D other)
    {
        if (other.CompareTag("Car"))
        {
            if (!IsCarInsideTrack(other.gameObject.transform.position))
            {
                if (other.TryGetComponent<PlayerController>(out PlayerController playerController))
                {
                    // Handle player out of track
                    playerController.OnOutOfTrack();
                }
                else
                {
                    int indexAgent = 0;
                    while (indexAgent < ListAIAgents.Count)
                    {
                        if (ListAIAgents[indexAgent].GetComponent<Collider2D>() == other) break;
                        else indexAgent++;
                    }

                    if (indexAgent >= ListAIAgents.Count) return;

                    ListAIAgents[indexAgent].OnOutOfTrack();
                }
                //other.GetComponent<CarController>().ChangeSpriteColor(Color.red); //DEBUG
            }
            //else other.GetComponent<CarController>().ChangeSpriteColor(Color.white); //DEBUG
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
}
