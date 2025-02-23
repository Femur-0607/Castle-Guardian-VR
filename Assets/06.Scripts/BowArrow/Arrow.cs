using UnityEngine;

public class Arrow : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Check for collision with an enemy or other object
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Handle damage or effects here
            Debug.Log("Arrow hit the enemy!");
            Destroy(gameObject); // Destroy the arrow on impact
        }
        else
        {
            // Destroy arrow if it hits something else
            Destroy(gameObject, 2f); // Arrow disappears after 2 seconds
        }
    }
}
