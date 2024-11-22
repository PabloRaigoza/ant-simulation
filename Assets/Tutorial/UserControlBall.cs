using UnityEngine;

public class BallController : MonoBehaviour
{
    public float speed = 10f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Vertical");
        float moveVertical = -Input.GetAxis("Horizontal");

        // Create a movement vector
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        // Set velocity for rolling motion
        rb.velocity = movement * speed;

        // Keep the ball rolling in the Z-axis while maintaining the Y-axis velocity
        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);

        // Jump when Space is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * 300f);  // Adjust force for the jump
        }
    }
}
