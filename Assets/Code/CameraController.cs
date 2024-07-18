using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float wasdSpeed = 30.0f;
    public float pannSpeed = 03.0f;
    [Space]
    public float zoomSpeed = 3.0f ;
    public float maxZoom   = 31.0f;      
    public float minZoom   = 11.0f;
    public bool invrseZoom = false;

    [Header("Debugging")]
    public GameObject debugUIPanel;
    public GameObject screenBlockr;
    public GameObject gameplayHUD;

    private Vector2 panStartPos;

    private float minX = 0.0f;
    private float maxX = 0.0f;
    private float minZ = 0.0f;
    private float maxZ = 0.0f;


    void Update()
    {
        processPAN();
        processWASD();
        processScroll();
        processDebug();
    }

    private void processWASD()
    {
        float hInput = Input.GetAxis("Horizontal");
        float vInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(hInput, 0.0f, vInput).normalized;

        Vector3 newPosition = transform.position + moveDirection * wasdSpeed * Time.deltaTime;

        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX); 
        newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);

        transform.position = newPosition;
    }

    private void processScroll()
    {
        float scrollInput  = Input.GetAxis("Mouse ScrollWheel") * (invrseZoom ? 1.0f : -1.0f);
        float newYPosition = transform.position.y + scrollInput * zoomSpeed;

        newYPosition = Mathf.Clamp(newYPosition, minZoom, maxZoom);

        transform.position = new Vector3(transform.position.x, newYPosition, transform.position.z);

        if (Mathf.Abs(scrollInput) > 0.0f)
        {
            CalculateXZMinMax();
        }
    }

    private void processDebug()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleDebugPanel();
        }
    }

    public void ToggleDebugPanel()
    {
        screenBlockr.SetActive(!screenBlockr.activeInHierarchy);
        debugUIPanel.SetActive(!debugUIPanel.activeInHierarchy);

        gameplayHUD.SetActive(!debugUIPanel.activeInHierarchy);
    }

    private void processPAN()
    {
        if (Input.GetMouseButtonDown(1))
        {
            panStartPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(1))
        {
            Vector2 currentMousePos = Input.mousePosition;

            Vector2 deltaMousePos = currentMousePos - panStartPos;

            Vector3 moveDirection = new Vector3(deltaMousePos.x, 0, deltaMousePos.y) * pannSpeed * Time.deltaTime;
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection.y = transform.position.z; // this works ¯\_(ツ)_/¯

            Vector3 newPosition = transform.position + moveDirection;

            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);

            transform.position = newPosition;
        }
    }

    public void CalculateXZMinMax()
    {
        int gridSize = GridManager.instance.GetGridSize();

        minX = 5.0f;
        maxX = (gridSize - 1) * 7.535f;

        minZ = (gridSize * -8.70f) - (transform.position.y / 10.0f) - 5.0f;
        maxZ = -20.0f;
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