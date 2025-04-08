using UnityEngine;
using System.Collections.Generic;
using System.Drawing;

public class TrackCreator : MonoBehaviour
{
    [SerializeField] bool useRandomSeed = false;
    [SerializeField] int seed = 0;

    [SerializeField] int width = 16;
    [SerializeField] int height = 9;

    [SerializeField] int initialNumberOfPoints = 10;
    [Range(140, 180)] [SerializeField] int maxAngleThreshold = 160;

    private Track track;
    private TrackDrawer drawer;
    private TrackCollider trackCollider;
    private bool showPoints = false;

    private void Start()
    {
        drawer = GetComponent<TrackDrawer>();
        trackCollider = GetComponent<TrackCollider>();
    }

    public void CreateTrack()
    {
        // Random seed generator
        if (useRandomSeed) seed = new System.Random().Next(int.MinValue, int.MaxValue);

        track = new Track(seed);
        track.CreateFullTrack(initialNumberOfPoints, width, height, (float)maxAngleThreshold);
        //drawer.DrawPoints(track, true);
        drawer.DrawCurvedPoints(track, showPoints);
        trackCollider.GenerateTrackCollider();
    }

    public void ToggleShowPoints()
    {
        showPoints = !showPoints;
        drawer.DrawCurvedPoints(track, showPoints);
    }
}
