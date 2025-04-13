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

    [Range(0f, 0.01f)] [SerializeField] float simplifyCurveTolerance = 0.001f;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    /*public float LineWidth
    {
        get { lineRenderer.startWidth };
    }*/

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

    public void DrawCurvedPoints(Track track, float lineWidth, bool showPoints = true)
    {
        ClearDrawnPoints();

        // Set line renderer width
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        // Set line renderer position count
        List<Vector2> trackCurveResolutionPoints = track.CurveResolutionPoints;
        trackCurveResolutionPoints.Add(new Vector3(trackCurveResolutionPoints[1].x, trackCurveResolutionPoints[1].y, 0));
        lineRenderer.positionCount = trackCurveResolutionPoints.Count;
        
        for (int i = 0; i < trackCurveResolutionPoints.Count; i++)
        {
            lineRenderer.SetPosition(i, trackCurveResolutionPoints[i]);
        }

        int nPointsBeforeSimplify = lineRenderer.positionCount;
        if (simplifyCurveTolerance > 0) lineRenderer.Simplify(simplifyCurveTolerance);
        Debug.Log("(" + nPointsBeforeSimplify + " - " + lineRenderer.positionCount + ") = " + (nPointsBeforeSimplify - lineRenderer.positionCount));


        // Draw points
        if (showPoints)
        {
            List<Vector2> trackCurvePoints = track.CurvePoints;

            for (int i = 0; i < trackCurvePoints.Count; i++)
            {
                Vector3 pointPos = new Vector3(trackCurvePoints[i].x, trackCurvePoints[i].y, 0);

                if (i % 3 == 2) // track[i] is a control point, draw curve between i-1, i and i+1
                {
                    GameObject newPoint = Instantiate(controlPointPrefab, pointPos, Quaternion.identity);
                    drawnPoints.Add(newPoint);
                }
                else // Not a Bezier control point
                {
                    GameObject newPoint = Instantiate(pointPrefab, pointPos, Quaternion.identity);
                    drawnPoints.Add(newPoint);
                }
            }
        }
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
