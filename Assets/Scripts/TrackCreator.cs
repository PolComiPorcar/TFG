using UnityEngine;
using System.Collections.Generic;

public class TrackCreator : MonoBehaviour
{
    [SerializeField] GameObject pointPrefab;

    [SerializeField] bool useRandomSeed = false;
    [SerializeField] int seed = 0;

    [SerializeField] int width = 20;
    [SerializeField] int height = 10;

    [SerializeField] int initialNumberOfPoints = 10;

    private List<Vector2> points = new();
    private List<GameObject> drawnPoints = new();

    private LineRenderer lineRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void CreateTrack()
    {
        // Initialize random seed
        if (useRandomSeed) seed = new System.Random().Next(int.MinValue, int.MaxValue);
        Random.InitState(seed);

        // Clear all the points if there are any
        foreach (GameObject point in drawnPoints)
        {
            Destroy(point);
        }

        lineRenderer.positionCount = 0;
        points.Clear();

        // Create the points 
        for (int i = 0; i < initialNumberOfPoints; i++)
        {
            float x_value = Random.Range(-width/2f, width/2f);
            float y_value = Random.Range(-height/2f, height/2f);

            points.Add(new Vector2(x_value, y_value));
        }

        List<Vector2> CHpoints = ConvexHull();

        Draw(CHpoints);        
    }

    private List<Vector2> ConvexHull()
    {
        List<Vector2> hull = new();

        Vector2 leftMost = FindLeftMostPoint(points);
        hull.Add(leftMost);

        Vector2 currentVertex = leftMost;
        Vector2 checkingVertex = Vector2.zero;

        while (checkingVertex != hull[0])
        {
            checkingVertex = points[0];

            for (int j = 1; j < points.Count; j++)
            {
                if (checkingVertex == currentVertex || IsCounterClockwise(currentVertex, checkingVertex, points[j]))
                    checkingVertex = points[j];
            }

            hull.Add(checkingVertex);
            currentVertex = checkingVertex;
        }

        return hull;
    }

    private bool IsCounterClockwise(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        Vector2 a = p1 - p0;
        Vector2 b = p2 - p1;

        float crossZ = a.x * b.y - a.y * b.x;

        return crossZ < 0;
    }

    private Vector2 FindLeftMostPoint(List<Vector2> points)
    {
        Vector2 leftMost = points[0];
        for (int i = 1; i < points.Count; i++)
        {
            if (points[i].x < leftMost.x) leftMost = points[i];
        }

        return leftMost;
    }

    private void Draw(List<Vector2> convexHullPoints)
    {
        lineRenderer.positionCount = convexHullPoints.Count;

        for (int i = 0; i < convexHullPoints.Count; i++)
        {
            Vector3 pointPos = new Vector3(convexHullPoints[i].x, convexHullPoints[i].y, 0);
            lineRenderer.SetPosition(i, pointPos);
        }

        foreach (Vector2 point in points)
        {
            Vector3 pointPos = new Vector3(point.x, point.y, 0);

            GameObject newPoint = Instantiate(pointPrefab, pointPos, Quaternion.identity);
            drawnPoints.Add(newPoint);
        }
    }
}
