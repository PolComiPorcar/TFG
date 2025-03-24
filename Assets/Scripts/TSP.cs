using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public static class TSP
{
    /*public static List<Vector2> Construct(List<Vector2> points, int seed, int iterations = 2000)
    {
        Random.InitState(seed);

        var list = new List<Vector2>(points);

        list.Add(list[0]);
        int count = list.Count - 1;

        // Track the best solution
        List<Vector2> best = new List<Vector2>(list);
        float bestLen = MeasureLength(list);

        // Simulated Annealing algorithm implementation
        for (int i = 0; i < iterations; i++)
        {
            float len = MeasureLength(list);

            // Select two random distinct indices
            int id1 = Random.Range(0, count);
            int id2;
            do id2 = Random.Range(0, count); while (id2 == id1);

            // Ensure id1 < id2
            if (id2 < id1)
                Swap(ref id1, ref id2);

            // Create new candidate solution by reversing segment
            var newPoints = ReversePoints(list, id1, id2);
            float newLen = MeasureLength(newPoints);

            // Calculate temperature and acceptance probability
            float k = 100;
            float T = ((1.0f + k) / (i + 1 + k)) / 2;
            float p = Mathf.Exp(-(newLen - len) / (T * 100)) / 2;

            // Decide whether to accept new solution
            if (newLen > len && Random.value > p)
            {
                // Reject the new configuration
            }
            else
            {
                // Accept the new configuration
                list = newPoints;

                // Update best solution if this is better
                if (newLen < bestLen)
                {
                    bestLen = newLen;
                    best = new List<Vector2>(list);
                }
            }
        }

        return best;
    }*/

    public static List<Vector2> ConstructPlanarTSP(List<Vector2> points)
    {
        const float maxAngleThreshold = 160f;

        // Step 1: Start with a convex hull of the points
        List<Vector2> hull = ConvexHull.Construct(points);
        List<Vector2> remainingPoints = points.Where(p => !hull.Contains(p)).ToList();

        // Step 2: Iteratively insert remaining points to minimize added distance
        while (remainingPoints.Count > 0)
        {
            int bestPointIndex = -1;
            int bestInsertionPos = -1;
            float bestInsertionCost = float.MaxValue;

            // Find the best insertion
            foreach (var point in remainingPoints)
            {
                for (int i = 0; i < hull.Count; i++)
                {
                    int nextI = (i + 1) % hull.Count;
                    float currentDist = Vector2.Distance(hull[i], hull[nextI]);
                    float newDist = Vector2.Distance(hull[i], point) + Vector2.Distance(point, hull[nextI]);
                    float insertionCost = newDist - currentDist;
                    float angleFormed = Vector2.Angle(point - hull[i], hull[nextI] - point);

                    if (insertionCost < bestInsertionCost)
                    {
                        bestInsertionCost = insertionCost;
                        bestPointIndex = remainingPoints.IndexOf(point);
                        bestInsertionPos = i + 1;
                    }
                }
            }

            if (bestPointIndex == -1) throw new Exception("There is no point that creates an angle below the threshold: " + maxAngleThreshold);

            // Insert the best point
            hull.Insert(bestInsertionPos, remainingPoints[bestPointIndex]);
            remainingPoints.RemoveAt(bestPointIndex);
        }

        int index, nextIndex, nextnextIndex;
        float maxAngle = MaxAngleOfTrack(hull, out index);
        nextIndex = (index + 1) % hull.Count;
        nextnextIndex = (index + 2) % hull.Count;

        Debug.Log("index: " + index + " (" + hull[index] + "), angleMax: " + maxAngle);

        if (maxAngle > maxAngleThreshold)
        {
            if (Vector2.Distance(hull[index], hull[nextIndex]) < Vector2.Distance(hull[nextIndex], hull[nextnextIndex]))
                Swap(hull, index, nextIndex); // swap i and i+1
            else
                Swap(hull, nextIndex, nextnextIndex); //swap i+1 and i+2
        }

        return hull;
    }

    // Retorna l'angle més gran entre els punts i l'index del primer punt que crea un angle amb i+1 i i+2 a la variable index
    private static float MaxAngleOfTrack(List<Vector2> track, out int index)
    {
        float maxAngle = float.NegativeInfinity;
        index = 0;

        for (int i = 0; i < track.Count; i++)
        {
            int nextI = (i + 1) % track.Count;
            int nextnextI = (i + 2) % track.Count;

            float angleFormed = Vector2.Angle(track[nextI] - track[i], track[nextnextI] - track[nextI]);

            if (angleFormed > maxAngle)
            {
                maxAngle = angleFormed;
                index = i;
            }
        }

        return maxAngle;
    }

    private static float MeasureLength(List<Vector2> points)
    {
        float length = 0;
        for (int i = 1; i < points.Count; i++)
        {
            length += Vector2.Distance(points[i], points[i - 1]);
        }
        return length;
    }

    private static void Swap<T>(ref T lhs, ref T rhs)
    {
        T temp = lhs;
        lhs = rhs;
        rhs = temp;
    }

    private static void Swap(List<Vector2> points, int index1, int index2)
    {
        Vector2 temp = points[index1];
        points[index1] = points[index2];
        points[index2] = temp;
    }

    private static List<Vector2> ReversePoints(List<Vector2> points, int start, int stop)
    {
        List<Vector2> result = new List<Vector2>(points.Count);

        // Add points before the reversal segment
        for (int i = 0; i < start; i++)
        {
            result.Add(points[i]);
        }

        // Add the reversed segment
        for (int i = stop; i >= start; i--)
        {
            result.Add(points[i]);
        }

        // Add points after the reversal segment
        for (int i = stop + 1; i < points.Count; i++)
        {
            result.Add(points[i]);
        }

        return result;
    }
}
