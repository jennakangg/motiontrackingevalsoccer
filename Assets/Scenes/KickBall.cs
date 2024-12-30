using UnityEngine;

public class KickBall : MonoBehaviour
{
    public float kickForce = 20f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Get the Rigidbody component
            Rigidbody rb = GetComponent<Rigidbody>();

            // Direction: Only forward, no vertical (y) component
            Vector3 direction = transform.forward;

            // Apply force in the horizontal direction
            rb.AddForce(-direction * kickForce, ForceMode.Impulse);
        }
    }
}
