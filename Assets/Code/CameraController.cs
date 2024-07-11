using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 30.0f;
   [Space]
    public float zoomSpeed = 3.0f ;
    public float maxZoom   = 31.0f;      
    public float minZoom   = 11.0f;       


    void Update()
    {
        processWASD();
        processScroll();
    }

    private void processWASD()
    {
        float hInput = Input.GetAxis("Horizontal");
        float vInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(hInput, 0.0f, vInput).normalized;

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    private void processScroll()
    {
        float scrollInput  = Input.GetAxis("Mouse ScrollWheel");
        float newYPosition = transform.position.y + scrollInput * zoomSpeed;

        newYPosition = Mathf.Clamp(newYPosition, minZoom, maxZoom);

        transform.position = new Vector3(transform.position.x, newYPosition, transform.position.z);
    }
}