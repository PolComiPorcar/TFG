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

    private Track track;
    private TrackDrawer drawer;

    private void Start()
    {
        drawer = GetComponent<TrackDrawer>();
    }

    public void CreateTrack()
    {
        // Random seed generator
        if (useRandomSeed) seed = new System.Random().Next(int.MinValue, int.MaxValue);

        track = new Track(seed);
        track.CreateFullTrack(initialNumberOfPoints, width, height);
        drawer.DrawCurvedPoints(track);
    }

    /*public void Update()
    {
        if (testPoint != Vector2.zero)
        {
            points[points.Count - 1] = QuadraticBezierCurve(points[points.Count - 3], points[1], points[points.Count - 2], t);
            Draw();
        }
    }*/
}
