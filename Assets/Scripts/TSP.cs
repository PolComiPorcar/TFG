using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public static class TSP
{
    public static List<Vector2> ConstructPlanarTSP(List<Vector2> points, float maxAngleThreshold = 160f)
    {
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

                    if (insertionCost < bestInsertionCost)
                    {
                        bestInsertionCost = insertionCost;
                        bestPointIndex = remainingPoints.IndexOf(point);
                        bestInsertionPos = i + 1;
                    }
                }
            }

            // Insert the best point
            hull.Insert(bestInsertionPos, remainingPoints[bestPointIndex]);
            remainingPoints.RemoveAt(bestPointIndex);
        }

        RemoveSteepAngles(hull, maxAngleThreshold);

        return hull;
    }

    private static void RemoveSteepAngles(List<Vector2> points, float maxAngleThreshold = 160f)
    {
        float angle;
        int index;
        (angle, index) = SteepestAngleOfTrack(points);

        int totalPointsEliminated = 0; //DEBUG ONLY
        while (angle > maxAngleThreshold)
        {
            totalPointsEliminated++;

            int nextIndex = (index + 1) % points.Count;
            points.RemoveAt(nextIndex); // Eliminem el punt que genera l'angle conflictiu.

            (angle, index) = SteepestAngleOfTrack(points);
        }

        Debug.Log("Total steep points eliminated: " + totalPointsEliminated);
    }

    // Retorna l'angle més gran entre els punts i l'index del primer punt que crea un angle amb i+1 i i+2 a la variable index
    private static (float angle, int index) SteepestAngleOfTrack(List<Vector2> track)
    {
        float maxAngle = float.NegativeInfinity;
        int index = 0;

        for (int i = 0; i < track.Count; i++)
        {
            int nextI = (i + 1) % track.Count;
            int nextnextI = (i + 2) % track.Count;

            float angleFormed = Vector2.Angle(track[nextI] - track[i], track[nextnextI] - track[nextI]);
            if (angleFormed < 90f) angleFormed = 180f - angleFormed; // Relativitzem l'angle

            if (angleFormed > maxAngle) 
            {
                maxAngle = angleFormed;
                index = i;
            }
        }

        return (maxAngle, index);
    }
}
