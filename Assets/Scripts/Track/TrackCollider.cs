using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackCollider : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private PolygonCollider2D polygonCollider;

    private float lineWidth;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        polygonCollider = GetComponent<PolygonCollider2D>();
        if (polygonCollider == null) polygonCollider = gameObject.AddComponent<PolygonCollider2D>();

        lineWidth = lineRenderer.startWidth;
    }

    public void GenerateTrackCollider()
    {
        Vector3[] positions = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(positions);

        if (positions.Count() >= 2) //If we have enough points to draw a line
        {
            //Get the number of line between two points
            int numberOfLines = positions.Length - 1;

            //Make as many paths for each different line as we have lines
            polygonCollider.pathCount = numberOfLines;

            //Get Collider points between two consecutive points
            for (int i = 0; i < numberOfLines - 1; i++)
            {
                //Get the two next points
                List<Vector2> currentPositions = new List<Vector2> {
                    positions[i],
                    positions[i+1]
                };

                List<Vector2> currentColliderPoints = CalculateColliderPoints(currentPositions);
                polygonCollider.SetPath(i, currentColliderPoints);
            }
        }
        else polygonCollider.pathCount = 0;
    }

    private List<Vector2> CalculateColliderPoints(List<Vector2> positions)
    {

        // m = (y2 - y1) / (x2 - x1)
        float m = (positions[1].y - positions[0].y) / (positions[1].x - positions[0].x);
        float deltaX = (lineWidth / 2f) * (m / Mathf.Pow(m * m + 1, 0.5f));
        float deltaY = (lineWidth / 2f) * (1 / Mathf.Pow(1 + m * m, 0.5f));

        //Calculate Vertex Offset from Line Point
        Vector2[] offsets = new Vector2[2];
        offsets[0] = new Vector2(-deltaX, deltaY);
        offsets[1] = new Vector2(deltaX, -deltaY);

        List<Vector2> colliderPoints = new List<Vector2> {
            positions[0] + offsets[0],
            positions[1] + offsets[0],
            positions[1] + offsets[1],
            positions[0] + offsets[1]
        };

        return colliderPoints;
    }
    

    // Alternativa que no funciona

    /*public void GenerateCollider()
{
    // Obtener todos los puntos del LineRenderer
    Vector3[] linePoints = new Vector3[lineRenderer.positionCount];
    lineRenderer.GetPositions(linePoints);

    // Convertir a Vector2 para el PolygonCollider
    List<Vector2> colliderPoints = new List<Vector2>();

    for (int i = 0; i < linePoints.Length - 1; i++)
    {
        Vector2 currentPoint = linePoints[i];
        Vector2 nextPoint = linePoints[i + 1];

        // Calcular dirección perpendicular
        Vector2 lineDirection = (nextPoint - currentPoint).normalized;
        Vector2 perpendicular = new Vector2(-lineDirection.y, lineDirection.x);

        // Crear puntos del lado izquierdo y derecho del camino
        Vector2 leftOffset = currentPoint + perpendicular * (lineWidth / 2);
        Vector2 rightOffset = currentPoint - perpendicular * (lineWidth / 2);

        colliderPoints.Add(leftOffset);
        colliderPoints.Add(rightOffset);
    }

    // Crear el collider
    polygonCollider.SetPath(0, colliderPoints.ToArray());
}*/
}
