using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackDrawer : MonoBehaviour
{
    [SerializeField] GameObject pointPrefab;
    [SerializeField] GameObject controlPointPrefab;

    private LineRenderer lineRenderer;
    private List<GameObject> drawnPoints = new();

    [Range(1, 99)] [SerializeField] int curveResolution = 10;
    [Range(0f, 0.01f)] [SerializeField] float simplifyCurveTolerance = 0.001f;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void DrawPoints(Track track, bool drawLines)
    {
        ClearDrawnPoints();

        if (drawLines) lineRenderer.positionCount = track.Count;

        for (int i = 0; i < track.Count; i++)
        {
            Vector3 pointPos = new Vector3(track[i].x, track[i].y, 0);

            GameObject newPoint = Instantiate(pointPrefab, pointPos, Quaternion.identity);
            drawnPoints.Add(newPoint);
            lineRenderer.SetPosition(i, pointPos);
        }
    }

    public void DrawCurvedPoints(Track track, bool showPoints = true)
    {
        ClearDrawnPoints();

        List<Vector2> trackCurvePoints = track.CurvePoints;
        print("n curve points (no resolution): " + trackCurvePoints.Count);

        if (trackCurvePoints.Count % 3 != 1) throw new Exception("The track isn't well formatted to curve the points");

        lineRenderer.positionCount = (trackCurvePoints.Count / 3) * (curveResolution + 2) + 2;

        /*print("0: " + trackCurvePoints[0]);
        print("last: " + trackCurvePoints[trackCurvePoints.Count - 1]);
        print("last2: " + trackCurvePoints[trackCurvePoints.Count - 2]);*/

        int lineIndex = 0;
        for (int i = 0; i < trackCurvePoints.Count; i++)
        {
            Vector3 pointPos = new Vector3(trackCurvePoints[i].x, trackCurvePoints[i].y, 0);

            if (i % 3 == 2) // track[i] is a control point, draw curve between i-1, i and i+1
            {
                if (showPoints)
                {
                    GameObject newPoint = Instantiate(controlPointPrefab, pointPos, Quaternion.identity);
                    drawnPoints.Add(newPoint);
                }

                for (int j = 0; j < curveResolution; j++)
                {
                    float t = (float)(j + 1) / (curveResolution + 1);
                    Vector2 curvePoint = QuadraticBezierCurve(trackCurvePoints[i - 1], trackCurvePoints[i], trackCurvePoints[i + 1], t);
                    lineRenderer.SetPosition(lineIndex, curvePoint);

                    lineIndex++;
                }
            }
            else // Not a Bezier control point
            {
                if (showPoints)
                {
                    GameObject newPoint = Instantiate(pointPrefab, pointPos, Quaternion.identity);
                    drawnPoints.Add(newPoint);
                }

                lineRenderer.SetPosition(lineIndex, pointPos);
                lineIndex++;
            }
        }

        lineRenderer.SetPosition(lineIndex, new Vector3(trackCurvePoints[1].x, trackCurvePoints[1].y, 0));

        print(lineRenderer.positionCount);
        if (simplifyCurveTolerance > 0) lineRenderer.Simplify(simplifyCurveTolerance);
        print(lineRenderer.positionCount);
    }

    private Vector2 QuadraticBezierCurve(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        float oneMinusT = 1 - t;

        return oneMinusT * oneMinusT * a + 2 * oneMinusT * t * b + t * t * c;
    }

    private void ClearDrawnPoints()
    {
        foreach (GameObject point in drawnPoints)
        {
            Destroy(point);
        }

        lineRenderer.positionCount = 0;
    }
}
