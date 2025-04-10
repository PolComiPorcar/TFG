using UnityEngine;
using System.Collections.Generic;
using System;

public class Track
{
    private const float MIN_CURVE_PERCENTAGE_RANGE = 0.1f;
    private const float MAX_CURVE_PERCENTAGE_RANGE = 0.4f;

    private List<Vector2> _track = new();
    private List<Vector2> _trackCurvePoints = new();
    private List<Vector2> _trackCurveResolutionPoints = new();

    private int _seed;
    private int _curveResolution;
    private float _distance = -1f;

    public Track(int seed, int curveResolution) 
    {
        UnityEngine.Random.InitState(seed);
        _seed = seed;
        _curveResolution = curveResolution;
    }

    public List<Vector2> CurvePoints
    {
        get { return _trackCurvePoints; }
    }
    public List<Vector2> CurveResolutionPoints
    {
        get { return _trackCurveResolutionPoints; }
    }
    public Vector2 this[int i] {
        get { return _track[i]; }
    }
    public int Count
    {
        get { return _track.Count; }
    }
    public float Distance
    {
        get { return _distance; }
    }

    public void CreateFullTrack(int nInitialPoints, float width, float height, float maxAngleThreshold)
    {
        CreateRandomPoints(nInitialPoints, width, height);

        _track = TSP.ConstructPlanarTSP(_track, maxAngleThreshold);

        CreateTrackFromHull();

        CreateResolutionedTrack();

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
                float x_value = UnityEngine.Random.Range(-width / 2f, width / 2f);
                float y_value = UnityEngine.Random.Range(-height / 2f, height / 2f);
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

    private void CreateTrackFromHull()
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
        float percentage = UnityEngine.Random.Range(minPercentageRange, maxPercentageRange);

        return p0 + (p1 - p0) * percentage;
    }

    private void CreateResolutionedTrack()
    {
        Debug.Log("n curve points (no resolution): " + _trackCurvePoints.Count);

        if (_trackCurvePoints.Count % 3 != 1) throw new Exception("The track isn't well formatted to curve the points");

        for (int i = 0; i < _trackCurvePoints.Count; i++)
        {
            Vector3 pointPos = new Vector3(_trackCurvePoints[i].x, _trackCurvePoints[i].y, 0);

            if (i % 3 == 2) // track[i] is a control point, add curve between i-1, i and i+1
            {
                for (int j = 0; j < _curveResolution; j++)
                {
                    float t = (float)(j + 1) / (_curveResolution + 1);
                    Vector2 curvePoint = QuadraticBezierCurve(_trackCurvePoints[i - 1], _trackCurvePoints[i], _trackCurvePoints[i + 1], t);
                    _trackCurveResolutionPoints.Add(curvePoint);
                }
            }
            else _trackCurveResolutionPoints.Add(pointPos); // Not a Bezier control point
        }

        // Set track distance
        _distance = 0;
        for (int i = 1; i < _trackCurveResolutionPoints.Count; i++)
        {
            _distance += Vector2.Distance(_trackCurveResolutionPoints[i], _trackCurveResolutionPoints[i - 1]);
        }
    }

    private Vector2 QuadraticBezierCurve(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        float oneMinusT = 1 - t;

        return oneMinusT * oneMinusT * a + 2 * oneMinusT * t * b + t * t * c;
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
