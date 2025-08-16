using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D body;
    public float forceValue = 20f;
    public float maxVerticalVelocity = 10f; // Adjust this value as needed
    private Vector3 initialPosition;
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        if (body == null)
        {
            Debug.LogError("Rigidbody2D component is missing from the bird GameObject.");
        }
        initialPosition = transform.position;
    }

    void Update()
    {
        if (!GameManager.isPlaying)
        { return; }
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            // Only reset vertical velocity, not the whole game state/timer
            if (body.velocity.y <= 0)
            {
                body.velocity = new Vector2(body.velocity.x, 0);
                body.AddForce(Vector2.up * forceValue, ForceMode2D.Impulse);
            }
        }

        // Limit vertical velocity
        if (body.velocity.y > maxVerticalVelocity)
        {
            body.velocity = new Vector2(body.velocity.x, maxVerticalVelocity);
        }

        // Out of bounds = game over
        if (transform.position.y > 1.15f || transform.position.y < -1.15f)
        {
            Debug.Log("Bird out of bounds. Stopping the game.");
            GameManager.isPlaying = false;
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Bird collided with an object. Stopping the game.");
        GameManager.isPlaying = false;
        GameManager.Instance.SubmitFinalScoreToLeaderboard();
    }
    public void ResetPlayerPosition()
    {
        transform.position = initialPosition;
        body.velocity = Vector2.zero;
    }
}
