using UnityEngine;
using FMODUnity;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
    private Camera _mainCamRef;

    [Header("Data")]
    public float wasdSpeed = 30.0f;
    public float pannSpeed = 03.0f;
    [Space]
    public float zoomSpeed = 3.0f ;
    public float maxZoom   = 31.0f;      
    public float minZoom   = 11.0f;
    public bool invrseZoom = false;
    [Space]
    public float tiltSpeed =  3.0f;
    public float tiltAngle = 45.0f;

    [Header("Alternate Cameras")]
    public KeyCode CameraToggle = KeyCode.C;
    public Camera OverheadCamera;

    [Header("Debugging")]
    public GameObject debugUIPanel;
    public GameObject screenBlockr;
    public GameObject gameplayHUD;

    private Vector2 panStartPos;
    private float panStartHight;

    private float minX = 0.0f;
    private float maxX = 0.0f;
    private float minZ = 0.0f;
    private float maxZ = 0.0f;

    private float currentY;

    void Start()
    {
        // Assume Controller's On Main Camera
        _mainCamRef = GetComponent<Camera>();
    }

    void Update()
    {
        processPAN();
        processWASD();
        processTilt();
        processDebug();
        processScroll();
        processAltToggle();

        currentY = transform.position.y;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("CameraHeight", currentY);
    }

    private void processWASD()
    {
        float hInput = Input.GetAxis("Horizontal");
        float vInput = Input.GetAxis("Vertical");

        if (Mathf.Abs(hInput) > 0.0f || Mathf.Abs(vInput) > 0.0f)
        {
            Vector3 moveDirection = new Vector3(hInput, 0.0f, vInput).normalized;

            Vector3 newPosition = transform.position + moveDirection * wasdSpeed * Time.deltaTime;

            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);

            transform.position = newPosition;
        }
    }

    private void processTilt()
    {
        float tiltDir = 0;
        tiltDir += Input.GetKey(KeyCode.E) ?  1 : 0;
        tiltDir += Input.GetKey(KeyCode.Q) ? -1 : 0;

        Quaternion targetRotation = Quaternion.Euler(50, tiltAngle * tiltDir, 0);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * tiltSpeed);
    }

    private void processScroll()
    {
        float scrollInput  = Input.GetAxis("Mouse ScrollWheel") * (invrseZoom ? 1.0f : -1.0f);

        if (Mathf.Abs(scrollInput) > 0.0f)
        {
            CalculateXZMinMax();

            float newYPosition = transform.position.y + scrollInput * zoomSpeed;

            newYPosition = Mathf.Clamp(newYPosition, minZoom, maxZoom);

            transform.position = new Vector3(transform.position.x, newYPosition, transform.position.z);

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
            panStartHight = transform.position.y;
        }
        else if (Input.GetMouseButton(1))
        {
            Vector2 currentMousePos = Input.mousePosition;

            Vector2 deltaMousePos = currentMousePos - panStartPos;

            Vector3 moveDirection = new Vector3(deltaMousePos.x, 0, deltaMousePos.y) * pannSpeed * Time.deltaTime;
            moveDirection = transform.TransformDirection(moveDirection);

            Vector3 newPosition = transform.position + moveDirection;

            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = panStartHight;
            newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);

            transform.position = newPosition;
        }
    }

    private void processAltToggle()
    {
        if (Input.GetKeyDown(CameraToggle))
        {
            // Only make sense for two cams
            _mainCamRef.enabled = !_mainCamRef.enabled;
            OverheadCamera.enabled = !OverheadCamera.enabled;
        }
    }

    public void CalculateXZMinMax()
    {
        int gridQ = GridManager.instance.GetGridSizeQ();
        int gridR = GridManager.instance.GetGridSizeR();

        minX = 5.0f;
        maxX = (gridQ - 1) * 7.535f;

        minZ = (gridR * -8.70f) - (transform.position.y / 10.0f) - 5.0f;
        maxZ = -20.0f;
    }

    public void FrameGrid(Transform topLeftTile, Transform botRightTile)
    {
        if (topLeftTile != null && botRightTile != null)
        {
            Bounds bounds = new Bounds(topLeftTile.position, Vector3.zero);
            bounds.Encapsulate(botRightTile.position);

            Vector3 camPos = bounds.center - Vector3.forward * 21.0f;
            camPos = new Vector3(camPos.x, transform.position.y, camPos.z);

            transform.position = camPos; // bounds.center - Vector3.forward * 21.0f;
        }
    }

    public void ToggleZoomInverse()
    {
        invrseZoom = !invrseZoom;
    }
}