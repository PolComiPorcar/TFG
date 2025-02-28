using UnityEngine;
using System.Collections.Generic;

public class Track
{
    private List<Vector2> _track = new();

    private const float MIN_CURVE_PERCENTAGE_RANGE = 0.1f;
    private const float MAX_CURVE_PERCENTAGE_RANGE = 0.4f;

    public Track(int seed) 
    {
        Random.InitState(seed);
    }

    public List<Vector2> Points
    {
        get { return _track; }
    }
    public Vector2 this[int i] {
        get { return _track[i]; }
    }
    public int Count
    {
        get { return _track.Count; }
    }

    public void CreateFullTrack(int nInitialPoints, float width, float height)
    {
        CreateRandomPoints(nInitialPoints, width, height);
        ConstructConvexHull();
        CreateTrackFromHull();
    }

    public void CreateRandomPoints(int numberOfPoints, float width, float height)
    {
        _track.Clear();

        for (int i = 0; i < numberOfPoints; i++)
        {
            float x_value = Random.Range(-width / 2f, width / 2f);
            float y_value = Random.Range(-height / 2f, height / 2f);

            _track.Add(new Vector2(x_value, y_value));
        }
    }

    public void ConstructConvexHull()
    {
        _track = ConvexHull.Construct(_track);
    }

    public void CreateTrackFromHull()
    {
        List<Vector2> pointsCurved = new();
        _track.Add(_track[0]);

        for (int i = 0; i < _track.Count - 1; i++)
        {
            // Inner point between the segment (tangent point of the curve)
            Vector2 innerPoint = CalculateCurvePoint(_track[i], _track[i + 1], MIN_CURVE_PERCENTAGE_RANGE, MAX_CURVE_PERCENTAGE_RANGE);
            pointsCurved.Add(innerPoint);

            // Outer point between the segment (tangent point of the curve)
            Vector2 outerPoint = CalculateCurvePoint(_track[i + 1], _track[i], MIN_CURVE_PERCENTAGE_RANGE, MAX_CURVE_PERCENTAGE_RANGE);
            pointsCurved.Add(outerPoint);

            pointsCurved.Add(_track[i + 1]);
        }

        pointsCurved.Add(pointsCurved[0]);

        _track = pointsCurved;
    }

    private Vector2 CalculateCurvePoint(Vector2 p0, Vector2 p1, float minPercentageRange, float maxPercentageRange)
    {
        float percentage = Random.Range(minPercentageRange, maxPercentageRange);

        return p0 + (p1 - p0) * percentage;
    }
}
