using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    private Rigidbody rb;

    // Adjustable parameters
    public float bounceReduction = 0.5f; // How much to reduce bounce
    public float rollFriction = 0.2f;    // Friction to make the ball roll

    void Start()
    {
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();

        // Ensure Rigidbody exists and gravity is enabled
        if (rb == null)
        {
            Debug.LogError("No Rigidbody attached to the ball!");
            return;
        }

        // Set initial Rigidbody settings
        rb.useGravity = true;           // Ensure gravity is enabled
        // rb.drag = rollFriction;         // Friction to slow down rolling
        rb.angularDrag = 0.5f;          // Adjust angular drag for smoother rolling
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Reduce vertical velocity when hitting the ground to minimize bounce
        Vector3 velocity = rb.velocity;
        if (velocity.y > 0.1f || velocity.y < -0.1f)
        {
            velocity.y *= bounceReduction; // Reduce the Y (upward) velocity
            rb.velocity = velocity;
        }
    }
}
