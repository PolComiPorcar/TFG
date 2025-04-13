using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TrackCollider : MonoBehaviour
{
    //private LineRenderer lineRenderer;
    //private PolygonCollider2D polygonCollider;

    [SerializeField] GameObject borderPrefab;

    private GameObject leftBorderObject;
    private GameObject rightBorderObject;

    private float lineWidth;

    public void GenerateTrackCollider(List<Vector2> trackPoints, float width)
    {
        lineWidth = width;

        if (!leftBorderObject) {
            leftBorderObject = Instantiate(borderPrefab, transform.position, Quaternion.identity, transform);
            leftBorderObject.name = "leftBorder";
        }
        if (!rightBorderObject) {
            rightBorderObject = Instantiate(borderPrefab, transform.position, Quaternion.identity, transform);
            rightBorderObject.name = "RightBorder";
        }

        EdgeCollider2D leftBorder = leftBorderObject.GetComponent<EdgeCollider2D>();
        EdgeCollider2D rightBorder = rightBorderObject.GetComponent<EdgeCollider2D>();

        Vector2[] leftBorderPoints = new Vector2[trackPoints.Count()];
        Vector2[] rightBorderPoints = new Vector2[trackPoints.Count()];

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

                // Perpendicular to direction
                normal = new Vector2(-dir.y, dir.x);
            }
            else if (i == 0)
            {
                // For first point
                Vector2 dir = (trackPoints[1] - trackPoints[0]).normalized;
                normal = new Vector2(-dir.y, dir.x);
            }
            else
            {
                // For last point
                Vector2 dir = (trackPoints[i] - trackPoints[i - 1]).normalized;
                normal = new Vector2(-dir.y, dir.x);
            }

            // Apply offset using the normal
            float halfWidth = lineWidth / 2f;
            leftBorderPoints[i] = trackPoints[i] + normal * halfWidth;
            rightBorderPoints[i] = trackPoints[i] - normal * halfWidth;
        }

        // Assign points to colliders
        leftBorder.points = leftBorderPoints;
        rightBorder.points = rightBorderPoints;
    }
}
