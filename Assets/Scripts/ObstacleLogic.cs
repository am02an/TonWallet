using UnityEngine;

public class ObstacleLogic : MonoBehaviour
{
    private Vector3 initialPosition; // Store starting position

    void Start()
    {
        // Save the starting position
        initialPosition = transform.position;
    }

    public void ResetPosition()
    {
        // Reset to the initial position exactly
        transform.position = initialPosition;
    }

    void Update()
    {
        if (GameManager.isPlaying)
        {
            transform.position += new Vector3(-GameManager.ObsVelocity * Time.deltaTime, 0, 0);

            if (transform.position.x < -3)
            {
                // Move to the right edge with a new random height
                transform.position = new Vector3(3, Random.Range(-0.7f, -1.7f), 0);
            }
        }
    }
}
