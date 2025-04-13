using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5;
    [SerializeField] float maxSpeed = 3;
    [SerializeField] float drag = 0.98f;
    [SerializeField] float steerAngle = 200;
    [SerializeField] float traction = 1;

    private Vector2 moveForce;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private float acceleration;
    private float steering;


    void Start()
    {
        // Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("Rigidbody2D component is missing! Please add it to the car.");
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) Debug.LogError("SpriteRenderer component is missing! Please add it to the car.");
    }

    public void SetInputs(float newAcceleration, float newSteering)
    {
        acceleration = newAcceleration;
        steering = newSteering;
    }

    void FixedUpdate()
    {
        // Apply movement force
        Vector2 forwardDirection = transform.up;
        if (acceleration > 0) moveForce += moveSpeed * acceleration * Time.fixedDeltaTime * forwardDirection;

        // Apply drag
        moveForce *= drag;

        // Clamp speed
        moveForce = Vector2.ClampMagnitude(moveForce, maxSpeed);

        // Traction
        moveForce = Vector2.Lerp(moveForce.normalized, transform.up, traction * Time.fixedDeltaTime) * moveForce.magnitude;

        // Move the car using Rigidbody2D
        rb.linearVelocity = moveForce;

        //Debug.Log(moveForce.ToString());

        // Rotate the car
        float rotationAmount = -steering * steerAngle * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation + rotationAmount);
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        float forwardVelocity = Vector2.Dot(rb.linearVelocity, transform.up);
        Vector2 forwardVelocityVector = transform.up * forwardVelocity;
        Vector2 lateralVelocityVector = rb.linearVelocity - forwardVelocityVector;

        // Draw total velocity vector
        DrawLineGizmos(transform.position, rb.linearVelocity * 0.1f, Color.green);

        // Draw forward velocity vector
        //DrawLineGizmos(transform.position, forwardVelocityVector * 0.1f, Color.green);

        // Also draw the lateral vector from the origin for clarity
        DrawLineGizmos(transform.position, lateralVelocityVector * 0.1f, Color.blue);
    }

    private void DrawLineGizmos(Vector3 start, Vector2 direction, Color color)
    {
        if (direction.magnitude < 0.01f) return; // Don't draw tiny arrows

        Vector3 end = start + (Vector3)direction;

        // Draw the main line
        Gizmos.color = color;
        Gizmos.DrawLine(start, end);
    }

    public void ChangeSpriteColor(Color color)
    {
        spriteRenderer.color = color;
    }
}
