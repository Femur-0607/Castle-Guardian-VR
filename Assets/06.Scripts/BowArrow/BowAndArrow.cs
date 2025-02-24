using UnityEngine;

public class BowAndArrow : MonoBehaviour
{
    public GameObject arrowPrefab; // Reference to the arrow prefab
    public Transform bowString; // Reference to the bowstring for drawing
    public Transform spawnPoint; // Point where the arrow will be spawned
    public float drawSpeed = 2f; // Speed at which the bowstring is drawn
    public float maxDrawDistance = 3f; // Maximum draw distance for the bowstring
    public float arrowForce = 50f; // Force applied to the arrow when shot
    
    private float drawDistance = 0f; // Current draw distance of the bowstring
    
    void Update()
    {
        // Draw the bowstring when holding the fire button
        if (Input.GetButton("Fire1"))
        {
            DrawBow();
        }
    
        // Shoot the arrow when the fire button is released
        if (Input.GetButtonUp("Fire1") && drawDistance > 0f)
        {
            ShootArrow();
        }
    }
    
    void DrawBow()
    {
        // Increase the draw distance while holding the fire button
        drawDistance = Mathf.Clamp(drawDistance + drawSpeed * Time.deltaTime, 0, maxDrawDistance);
        bowString.localPosition = new Vector3(0, drawDistance, 0);
    }
    
    void ShootArrow()
    {
        // Instantiate and shoot the arrow
        GameObject arrow = Instantiate(arrowPrefab, spawnPoint.position, spawnPoint.rotation);
        Rigidbody arrowRb = arrow.GetComponentInChildren<Rigidbody>();
        arrowRb.AddForce(spawnPoint.forward * arrowForce * drawDistance, ForceMode.VelocityChange);
    
        // Reset the bowstring
        drawDistance = 0f;
        bowString.localPosition = Vector3.zero;
    }
}
