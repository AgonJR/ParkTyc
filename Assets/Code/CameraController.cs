using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 30.0f;    // Speed of camera movement

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontalInput, 0.0f, verticalInput).normalized;

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }
}