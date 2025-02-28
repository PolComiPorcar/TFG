using System.Collections.Generic;
using UnityEngine;

public static class ConvexHull
{
    public static List<Vector2> Construct(List<Vector2> points)
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

        hull.RemoveAt(hull.Count - 1);

        return hull;
    }

    private static bool IsCounterClockwise(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        Vector2 a = p1 - p0;
        Vector2 b = p2 - p1;

        float crossZ = a.x * b.y - a.y * b.x;

        return crossZ < 0;
    }

    private static Vector2 FindLeftMostPoint(List<Vector2> points)
    {
        Vector2 leftMost = points[0];
        for (int i = 1; i < points.Count; i++)
        {
            if (points[i].x < leftMost.x) leftMost = points[i];
        }

        return leftMost;
    }
}
