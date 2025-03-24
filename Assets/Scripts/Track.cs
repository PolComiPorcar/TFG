using UnityEngine;
using System.Collections.Generic;

public class Track
{
    private List<Vector2> _track = new();
    private List<Vector2> _trackCurvePoints = new();
    private int _seed;

    private const float MIN_CURVE_PERCENTAGE_RANGE = 0.1f;
    private const float MAX_CURVE_PERCENTAGE_RANGE = 0.4f;

    public Track(int seed) 
    {
        Random.InitState(seed);
        _seed = seed;
    }

    public List<Vector2> CurvePoints
    {
        get { return _trackCurvePoints; }
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
        /*ConstructConvexHull();*/

        //_track = TSP.Construct(_track, _seed);

        _track = TSP.ConstructPlanarTSP(_track);

        CreateTrackFromHull();

        //Debug.Log(GetDisplacementFromCenter());
    }

    public void CreateRandomPoints(int numberOfPoints, float width, float height, float minDistBetweenPoints = 1f)
    {
        _track.Clear();

        for (int i = 0; i < numberOfPoints; i++)
        {
            bool isPointAcceptable = false;
            Vector2 point = Vector2.zero;

            while (!isPointAcceptable)
            {
                float x_value = Random.Range(-width / 2f, width / 2f);
                float y_value = Random.Range(-height / 2f, height / 2f);
                point = new Vector2(x_value, y_value);

                isPointAcceptable = IsPointWithinMinDistance(point, _track, minDistBetweenPoints);
            }

            _track.Add(point);
        }
    }

    private bool IsPointWithinMinDistance(Vector2 point, List<Vector2> pointsToCompare, float minDistThreshold)
    {
        for (int i = 0; i < pointsToCompare.Count; i++)
        {
            if (Vector2.Distance(point, pointsToCompare[i]) < minDistThreshold) return false;
        }

        return true;
    }

    public void ConstructConvexHull()
    {
        _track = ConvexHull.Construct(_track);
    }

    public void CreateTrackFromHull()
    {
        _track.Add(_track[0]);

        for (int i = 0; i < _track.Count - 1; i++)
        {
            // Inner point between the segment (tangent point of the curve)
            Vector2 innerPoint = CalculateCurvePoint(_track[i], _track[i + 1], MIN_CURVE_PERCENTAGE_RANGE, MAX_CURVE_PERCENTAGE_RANGE);
            _trackCurvePoints.Add(innerPoint);

            // Outer point between the segment (tangent point of the curve)
            Vector2 outerPoint = CalculateCurvePoint(_track[i + 1], _track[i], MIN_CURVE_PERCENTAGE_RANGE, MAX_CURVE_PERCENTAGE_RANGE);
            _trackCurvePoints.Add(outerPoint);

            _trackCurvePoints.Add(_track[i + 1]);
        }

        _trackCurvePoints.Add(_trackCurvePoints[0]);
    }

    private Vector2 CalculateCurvePoint(Vector2 p0, Vector2 p1, float minPercentageRange, float maxPercentageRange)
    {
        float percentage = Random.Range(minPercentageRange, maxPercentageRange);

        return p0 + (p1 - p0) * percentage;
    }

    private Vector2 GetDisplacementFromCenter()
    {
        Vector2 displacementFromCenter = Vector2.zero;

        for (int i = 0; i < _track.Count; i++)
        {
            displacementFromCenter += _track[i];
        }

        return displacementFromCenter / _track.Count;
    }
}
