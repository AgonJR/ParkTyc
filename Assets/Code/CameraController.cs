using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 30.0f;
   [Space]
    public float zoomSpeed = 3.0f ;
    public float maxZoom   = 31.0f;      
    public float minZoom   = 11.0f;
    public bool invrseZoom = false;

    [Header("Debugging")]
    public GameObject debugUIPanel;
    public GameObject screenBlockr;
    public GameObject gameplayHUD;


    void Update()
    {
        processWASD();
        processScroll();
        processDebug();
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
        float scrollInput  = Input.GetAxis("Mouse ScrollWheel") * (invrseZoom ? 1.0f : -1.0f);
        float newYPosition = transform.position.y + scrollInput * zoomSpeed;

        newYPosition = Mathf.Clamp(newYPosition, minZoom, maxZoom);

        transform.position = new Vector3(transform.position.x, newYPosition, transform.position.z);
    }

    private void processDebug()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            screenBlockr.SetActive(!screenBlockr.activeInHierarchy);
            debugUIPanel.SetActive(!debugUIPanel.activeInHierarchy);

            gameplayHUD.SetActive(!debugUIPanel.activeInHierarchy);
        }
    }

    public void FrameGrid(Transform topLeftTile, Transform botRightTile)
    {
        if (topLeftTile != null && botRightTile != null)
        {
            Bounds bounds = new Bounds(topLeftTile.position, Vector3.zero);
            bounds.Encapsulate(botRightTile.position);

            Camera.main.transform.position = bounds.center - Vector3.forward * 21.0f;
        }
    }

    public void ToggleZoomInverse()
    {
        invrseZoom = !invrseZoom;
    }
}