using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TrackCollider : MonoBehaviour
{
    [SerializeField] GameObject borderPrefab;

    private GameObject leftBorderObject;
    private GameObject rightBorderObject;

    public void GenerateTrackCollider(List<Vector2> trackPoints, float width)
    {
        trackPoints.RemoveRange(trackPoints.Count - 2, 2);

        InitializeGameObjects();

        EdgeCollider2D leftBorder = leftBorderObject.GetComponent<EdgeCollider2D>();
        EdgeCollider2D rightBorder = rightBorderObject.GetComponent<EdgeCollider2D>();

        List<Vector2> leftBorderPoints = new List<Vector2>(trackPoints.Count + 1);
        List<Vector2> rightBorderPoints = new List<Vector2>(trackPoints.Count + 1);

        for (int i = 0; i < trackPoints.Count; i++)
        {
            // Calculate normal vector
            Vector2 normal = Vector2.zero;

            if (i > 0 && i < trackPoints.Count - 1)
            {
                // For middle points, average the normals of the two segments
                Vector2 prevDir = (trackPoints[i] - trackPoints[i - 1]).normalized;
                Vector2 nextDir = (trackPoints[i + 1] - trackPoints[i]).normalized;
                Vector2 dir = (prevDir + nextDir).normalized;

                normal = new Vector2(-dir.y, dir.x); // Perpendicular to direction
            }
            else if (i == 0) // For first point
            {
                Vector2 dir = (trackPoints[1] - trackPoints[0]).normalized;
                normal = new Vector2(-dir.y, dir.x);
            }
            else // For last point
            { 
                Vector2 dir = (trackPoints[i] - trackPoints[i - 1]).normalized;
                normal = new Vector2(-dir.y, dir.x);
            }

            // Apply offset using the normal
            float halfWidth = width / 2f;
            leftBorderPoints.Add(trackPoints[i] + normal * halfWidth);
            rightBorderPoints.Add(trackPoints[i] - normal * halfWidth);
        }

        // Process borders for self-intersections
        ProcessBorderForSelfIntersections(leftBorderPoints);
        ProcessBorderForSelfIntersections(rightBorderPoints);

        leftBorderPoints.Add(leftBorderPoints[0]);
        rightBorderPoints.Add(rightBorderPoints[0]);

        // Assign points to colliders
        leftBorder.points = leftBorderPoints.ToArray();
        rightBorder.points = rightBorderPoints.ToArray();
    }

    private void InitializeGameObjects()
    {
        if (!leftBorderObject)
        {
            leftBorderObject = Instantiate(borderPrefab, transform.position, Quaternion.identity, transform);
            leftBorderObject.name = "leftBorder";
        }
        if (!rightBorderObject)
        {
            rightBorderObject = Instantiate(borderPrefab, transform.position, Quaternion.identity, transform);
            rightBorderObject.name = "RightBorder";
        }
    }

    private void ProcessBorderForSelfIntersections(List<Vector2> borderPoints)
    {
        int i = 0;
        while (i < borderPoints.Count - 3) // Need at least 4 points to form an intersection (2 line segments)
        {
            bool intersectionFound = false;

            // Check current edge against all non-adjacent edges
            for (int j = i + 2; j < borderPoints.Count - 1; j++)
            {
                Vector2 intersection;
                if (LineSegmentsIntersect(borderPoints[i], borderPoints[i + 1], borderPoints[j], borderPoints[j + 1], out intersection))
                {
                    // Found intersection, remove points between i+1 and j
                    borderPoints[i + 1] = intersection; // Replace point i+1 with intersection point
                    borderPoints.RemoveRange(i + 2, j - (i + 1)); // Remove points between i+1 and j
                    intersectionFound = true;
                    break;
                }
            }

            if (!intersectionFound)
            {
                i++; // Only advance if no intersection was found
            }
        }
    }

    private bool LineSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
    {
        intersection = Vector2.zero;

        float denominator = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);

        // Lines are parallel or coincident
        if (Mathf.Approximately(denominator, 0f))
            return false;

        float ua = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / denominator;
        float ub = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / denominator;

        // Check if intersection point is on both line segments
        if (ua < 0f || ua > 1f || ub < 0f || ub > 1f)
            return false;

        // Calculate the intersection point
        intersection.x = p1.x + ua * (p2.x - p1.x);
        intersection.y = p1.y + ua * (p2.y - p1.y);

        return true;
    }
}
