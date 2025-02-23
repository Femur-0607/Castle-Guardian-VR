using UnityEngine;

public class BowAiming : MonoBehaviour
{
    public float rotationSpeed = 5f; // Speed at which the bow rotates

    void Update()
    {
        // Rotate the bow based on mouse movement
        float horizontal = Input.GetAxis("Mouse X");
        float vertical = Input.GetAxis("Mouse Y");

        transform.Rotate(Vector3.up * horizontal * rotationSpeed);
        transform.Rotate(Vector3.left * vertical * rotationSpeed);
    }
}
