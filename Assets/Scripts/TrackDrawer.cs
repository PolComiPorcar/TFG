using System;
using System.Collections.Generic;
using UnityEngine;

public class TrackDrawer : MonoBehaviour
{
    [SerializeField] GameObject pointPrefab;

    private LineRenderer lineRenderer;
    private List<GameObject> drawnPoints = new();

    [Range(1, 99)]
    [SerializeField] int curveResolution = 10;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void DrawPoints(Track track, bool drawLines)
    {
        Clear();

        if (drawLines) lineRenderer.positionCount = track.Count;

        for (int i = 0; i < track.Count; i++)
        {
            Vector3 pointPos = new Vector3(track[i].x, track[i].y, 0);

            GameObject newPoint = Instantiate(pointPrefab, pointPos, Quaternion.identity);
            drawnPoints.Add(newPoint);
            lineRenderer.SetPosition(i, pointPos);
        }
    }

    public void DrawCurvedPoints(Track track)
    {
        Clear();

        if (track.Count % 3 != 1) throw new Exception("The track isn't well formatted to curve the points");

        lineRenderer.positionCount = (track.Count / 3) * (curveResolution + 2);
        lineRenderer.SetPosition(0, track[0]);

        int lineIndex = 0;
        for (int i = 1; i < track.Count; i++)
        {
            if (i % 3 == 2) // track[i] is a control point, draw curve between i-1, i and i+1
            {
                for (int j = 0; j < curveResolution; j++)
                {
                    float t = (float)(j + 1) / (curveResolution + 1);
                    Vector2 curvePoint = QuadraticBezierCurve(track[i - 1], track[i], track[i + 1], t);
                    lineRenderer.SetPosition(lineIndex, curvePoint);

                    lineIndex++;
                }
            }
            else // Not a Bezier control point
            {
                Vector3 pointPos = new Vector3(track[i].x, track[i].y, 0);

                GameObject newPoint = Instantiate(pointPrefab, pointPos, Quaternion.identity);
                drawnPoints.Add(newPoint);
                lineRenderer.SetPosition(lineIndex, pointPos);
                lineIndex++;
            }
        }
    }

    private Vector2 QuadraticBezierCurve(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        float oneMinusT = 1 - t;

        return oneMinusT * oneMinusT * a + 2 * oneMinusT * t * b + t * t * c;
    }

    private void Clear()
    {
        foreach (GameObject point in drawnPoints)
        {
            Destroy(point);
        }

        lineRenderer.positionCount = 0;
    }
}
